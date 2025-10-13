// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Extensions;

public static class CommandResultExtensions
{
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

    /// <summary>
    /// Checks if an option with the specified name has a result in the command result.
    /// </summary>
    /// <param name="commandResult">The command result to check.</param>
    /// <param name="optionName">The name of the option to check for (including -- prefix).</param>
    /// <returns>True if the option has a result, false otherwise.</returns>
    public static bool HasOptionResult(this CommandResult commandResult, string optionName)
    {
        // Find the option by name in the command
        var option = commandResult.Command.Options
            .FirstOrDefault(o => o.Name == optionName || o.Aliases.Contains(optionName));

        return option != null && HasOptionResult(commandResult, option);
    }

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
            // Handle nullable types explicitly - null is a valid value for nullable types
            if (def is null && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                value = default; // This will be null for nullable types
                return true;
            }
            if (def is T typed)
            {
                value = typed;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static bool TryGetValue<T>(this CommandResult commandResult, string optionName, out T? value)
    {
        // Find the option by name in the command
        var option = FindOptionTByName<T>(commandResult, optionName);

        if (option is null)
        {
            value = default;
            return false;
        }

        return TryGetValue(commandResult, option, out value);
    }

    public static T? GetValueOrDefault<T>(this CommandResult commandResult, Option<T> option)
    {
        ArgumentNullException.ThrowIfNull(commandResult);
        ArgumentNullException.ThrowIfNull(option);

        // Find the OptionResult in the parse tree
        var optionResult = commandResult.GetResult(option);

        // If the option was not provided (null) OR it was implicitly assigned (no token supplied),
        // check if there's a default value before returning null
        if (optionResult is null || optionResult.Implicit)
        {
            // If the option has a default value, return it
            if (option.HasDefaultValue)
            {
                var def = option.GetDefaultValue();
                // Handle nullable types explicitly - null is a valid value for nullable types
                if (def is null && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return default; // This will be null for nullable types
                }
                if (def is T typed)
                {
                    return typed;
                }
            }
            return default; // For value types, this is default(T?) => null; for refs => null
        }

        // At this point it was explicitly supplied by the user; get its value.
        // Using the System.CommandLine API directly to avoid accidental recursion.
        return optionResult.GetValueOrDefault<T>();
    }

    public static T? GetValueOrDefault<T>(this CommandResult commandResult, string optionName)
    {
        // Find the option by name in the command
        var option = FindOptionTByName<T>(commandResult, optionName);

        if (option is null)
        {
            return default;
        }

        return GetValueOrDefault(commandResult, option);
    }

    public static T? GetValueWithoutDefault<T>(this CommandResult commandResult, Option<T> option)
    {
        ArgumentNullException.ThrowIfNull(commandResult);
        ArgumentNullException.ThrowIfNull(option);

        // Find the OptionResult in the parse tree
        var optionResult = commandResult.GetResult(option);

        // If the option was not provided (null) OR it was implicitly assigned (no token supplied),
        // return without considering option default values
        if (optionResult is null || optionResult.Implicit)
        {
            return default; // For value types, this is default(T?) => null; for refs => null
        }

        // At this point it was explicitly supplied by the user; get its value.
        // Using the System.CommandLine API directly to avoid accidental recursion.
        return optionResult.GetValueOrDefault<T>();
    }

    public static T? GetValueWithoutDefault<T>(this CommandResult commandResult, string optionName)
    {
        // Find the option by name in the command
        var option = FindOptionTByName<T>(commandResult, optionName);

        if (option is null)
        {
            return default;
        }

        return GetValueWithoutDefault(commandResult, option);
    }

    private static Option<T>? FindOptionTByName<T>(CommandResult commandResult, string optionName)
        => commandResult.Command.Options.OfType<Option<T>>()
            .FirstOrDefault(o => o.Name == optionName || o.Aliases.Contains(optionName));
}
