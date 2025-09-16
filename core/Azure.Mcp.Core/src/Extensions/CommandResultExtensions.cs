// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Extensions;

public static class CommandResultExtensions
{
    public static SymbolResult? GetOptionResult(this CommandResult commandResult, Option option)
        => commandResult.GetResult(option);

    public static bool HasOptionResult(this CommandResult commandResult, Option option)
    {
        var result = commandResult.GetResult(option);
        if (result is null)
            return false;

        // Bool options (nullable or non-nullable) can work as switches or with explicit values
        // Check if this is a bool option by looking at the value type
        var isBoolOption = option.ValueType == typeof(bool) || option.ValueType == typeof(bool?);
        if (isBoolOption)
        {
            // For bool options, consider present if:
            // 1. Identifier was provided (switch usage: --verbose)
            // 2. OR explicit value tokens exist (explicit usage: --verbose true)
            return result.IdentifierTokenCount > 0;
        }

        // For other zero-arity options, identifier presence indicates explicit usage
        var expectsValue = option.Arity.MaximumNumberOfValues > 0;
        if (!expectsValue)
        {
            return result.IdentifierTokenCount > 0;
        }

        // For value-taking options, consider present only if there is at least one non-empty value token
        var hasNonEmptyValue = result.Tokens is { Count: > 0 } && result.Tokens.Any(t => !string.IsNullOrWhiteSpace(t.Value));
        return hasNonEmptyValue;
    }

    public static bool HasOptionResult<T>(this CommandResult commandResult, Option<T> option)
        => HasOptionResult(commandResult, (Option)option);

    public static bool TryGetValue<T>(this CommandResult commandResult, Option<T> option, out T? value)
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

    public static T? GetValueOrDefault<T>(this CommandResult commandResult, Option<T> option)
    {
        // If the option was explicitly provided, return its value (including empty strings)
        if (commandResult.HasOptionResult(option) && commandResult.TryGetValue(option, out T? value))
        {
            return value;
        }

        // If not explicitly provided but option has a default value, use the default
        if (option.HasDefaultValue)
        {
            var defaultValue = option.GetDefaultValue();
            if (defaultValue is T typed)
            {
                return typed;
            }
        }

        // No explicit result and no option default; return null to indicate absence
        return default(T?);
    }
}
