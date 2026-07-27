// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;

namespace Microsoft.Mcp.Core.Extensions;

internal static class CommandResultExtensions
{
    /// <summary>
    /// Checks if an option has a result in the command result.
    /// </summary>
    /// <param name="commandResult">The command result to check.</param>
    /// <param name="option">The option to check for.</param>
    /// <returns>True if the option has a result, false otherwise.</returns>
    internal static bool HasOptionResult(this CommandResult commandResult, Option option)
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

    /// <summary>
    /// Attempts to retrieve the commandResult value for the Option. If found it is returned, otherwise the default
    /// value for T is returned.
    /// </summary>
    /// <typeparam name="T">The type of the option value</typeparam>
    /// <param name="commandResult">The command result</param>
    /// <param name="option">The option</param>
    /// <returns>The value of the option, or the default value if not found or not set</returns>
    internal static T? GetValueOrDefault<T>(this CommandResult commandResult, Option<T> option)
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
}
