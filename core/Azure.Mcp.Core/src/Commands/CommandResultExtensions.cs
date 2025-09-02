// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Commands;

public static class CommandResultExtensions
{
    public static SymbolResult? GetOptionResult(this CommandResult commandResult, Option option)
        => commandResult.GetResult(option);

    public static bool HasOptionResult(this CommandResult commandResult, Option option)
    {
        var result = commandResult.GetResult(option);
        if (result is null)
            return false;

        // Consider explicitly present if we have an identifier token (e.g., --option)
        if (result.IdentifierTokenCount > 0)
            return true;

        // Or if there are non-empty value tokens (covers edge inputs)
        var hasValueTokens = result.Tokens is { Count: > 0 } && result.Tokens.Any(t => !string.IsNullOrWhiteSpace(t.Value));
        if (hasValueTokens)
            return true;

        // Otherwise, treat as not present (likely implicit/default)
        return false;
    }

    public static bool HasOptionResult<T>(this CommandResult commandResult, Option<T> option)
        => HasOptionResult(commandResult, (Option)option);

    public static bool TryGetOptionValue<T>(this CommandResult commandResult, Option<T> option, out T? value)
    {
        // If the option has any result (explicit or implicit), attempt to read its value.
        var result = commandResult.GetResult(option);
        if (result is not null)
        {
            try
            {
                value = commandResult.GetValue(option);
                return true;
            }
            catch
            {
                // Fall through to check default value below
            }
        }

        // If no result (or GetValue failed), return the option's default when available.
        if (option.HasDefaultValue)
        {
            var def = option.GetDefaultValue();
            if (def is T typed)
            {
                value = typed;
                return true;
            }
        }

        value = default;
        return false;
    }
}
