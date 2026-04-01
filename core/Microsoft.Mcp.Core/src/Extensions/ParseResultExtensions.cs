// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Extensions;

public static class ParseResultExtensions
{
    public static bool TryGetValue<T>(this ParseResult parseResult, Option<T> option, out T? value)
        => parseResult.CommandResult.TryGetValue(option, out value);

    public static bool TryGetValue<T>(this ParseResult parseResult, string optionName, out T? value)
    {
        // Find the option by name in the command
        var command = parseResult.CommandResult.Command;
        var option = command.Options.OfType<Option<T>>()
            .FirstOrDefault(o => o.Name == optionName || o.Aliases.Contains(optionName));

        if (option != null)
        {
            return parseResult.CommandResult.TryGetValue(option, out value);
        }

        value = default;
        return false;
    }

    public static T? GetValueOrDefault<T>(this ParseResult parseResult, Option<T> option)
        => parseResult.CommandResult.GetValueOrDefault(option);

    /// <summary>
    /// Gets the value of an option by name, returning default if not found or not set
    /// </summary>
    public static T? GetValueOrDefault<T>(this ParseResult parseResult, string optionName)
    {
        // Find the option by name in the command
        var command = parseResult.CommandResult.Command;
        var option = command.Options.OfType<Option<T>>()
            .FirstOrDefault(o => o.Name == optionName || o.Aliases.Contains(optionName));

        return option != null ? parseResult.GetValueOrDefault(option) : default;
    }

    public static bool HasAnyRetryOptions(this ParseResult parseResult)
    {
        foreach (var child in parseResult.CommandResult.Children)
        {
            if (child is OptionResult optionResult)
            {
                var option = optionResult.Option;
                if (option is null)
                {
                    continue;
                }

                var name = NameNormalization.NormalizeOptionName(option.Name);
                if (RetryOptionNames.Contains(name))
                    return true;

                var aliases = option.Aliases ?? [];
                foreach (var alias in aliases)
                {
                    var normalized = NameNormalization.NormalizeOptionName(alias);
                    if (RetryOptionNames.Contains(normalized))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static readonly HashSet<string> RetryOptionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        NameNormalization.NormalizeOptionName(OptionDefinitions.RetryPolicy.DelayName),
        NameNormalization.NormalizeOptionName(OptionDefinitions.RetryPolicy.MaxDelayName),
        NameNormalization.NormalizeOptionName(OptionDefinitions.RetryPolicy.MaxRetriesName),
        NameNormalization.NormalizeOptionName(OptionDefinitions.RetryPolicy.ModeName),
        NameNormalization.NormalizeOptionName(OptionDefinitions.RetryPolicy.NetworkTimeoutName),
    };
}
