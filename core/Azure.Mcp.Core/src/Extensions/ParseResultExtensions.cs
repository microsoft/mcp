// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Extensions;

public static class ParseResultExtensions
{
    public static bool TryGetValue<T>(this ParseResult parseResult, Option<T> option, out T? value)
        => parseResult.CommandResult.TryGetValue(option, out value);

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
}
