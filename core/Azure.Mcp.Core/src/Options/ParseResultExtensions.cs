// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Options;

public static class ParseResultExtensions
{
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
        NameNormalization.NormalizeOptionName(Models.Option.OptionDefinitions.RetryPolicy.DelayName),
        NameNormalization.NormalizeOptionName(Models.Option.OptionDefinitions.RetryPolicy.MaxDelayName),
        NameNormalization.NormalizeOptionName(Models.Option.OptionDefinitions.RetryPolicy.MaxRetriesName),
        NameNormalization.NormalizeOptionName(Models.Option.OptionDefinitions.RetryPolicy.ModeName),
        NameNormalization.NormalizeOptionName(Models.Option.OptionDefinitions.RetryPolicy.NetworkTimeoutName),
    };
}
