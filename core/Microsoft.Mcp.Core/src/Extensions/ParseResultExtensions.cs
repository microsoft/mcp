// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Extensions;

public static class ParseResultExtensions
{
    public static bool TryGetValue<T>(this ParseResult parseResult, Option<T> option, out T? value)
        => TryGetValue(parseResult, option.Name, out value);

    public static bool TryGetValue<T>(this ParseResult parseResult, string optionName, out T? value)
        => parseResult.CommandResult.TryGetValue(optionName, out value);

    public static T? GetValueOrDefault<T>(this ParseResult parseResult, Option<T> option)
        => GetValueOrDefault<T>(parseResult, option.Name);

    /// <summary>
    /// Special version for GetValueOrDefault for new option binding as the Option is immutable once created.
    /// <para>
    /// The name-based lookup was needed because the static Option would be re-created in RegisterOptions when changing
    /// requiredness. ParseResult result retrieval uses the Option instance references as a Dictionary key. So,
    /// changing requiredness in RegisterOptions and using the static Option in BindOptions would break silently and
    /// necessitated the name based retrieval. That is no longer necessary with the new option binding approach and
    /// this more performant approach should be used.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the option value</typeparam>
    /// <param name="parseResult">The parse result</param>
    /// <param name="option">The option</param>
    /// <returns>The value of the option, or the default value if not found or not set</returns>
    internal static T? GetValueOrDefaultWithoutName<T>(this ParseResult parseResult, Option<T> option)
        => parseResult.CommandResult.GetValueOrDefaultWithoutName<T>(option);

    /// <summary>
    /// Gets the value of an option by name, returning default if not found or not set
    /// </summary>
    public static T? GetValueOrDefault<T>(this ParseResult parseResult, string optionName)
        => parseResult.CommandResult.GetValueOrDefault<T>(optionName);

    public static bool HasAnyRetryOptions(this ParseResult parseResult)
        => parseResult.CommandResult.HasOptionResult(OptionDefinitions.RetryPolicy.Delay) ||
           parseResult.CommandResult.HasOptionResult(OptionDefinitions.RetryPolicy.MaxDelay) ||
           parseResult.CommandResult.HasOptionResult(OptionDefinitions.RetryPolicy.MaxRetries) ||
           parseResult.CommandResult.HasOptionResult(OptionDefinitions.RetryPolicy.Mode) ||
           parseResult.CommandResult.HasOptionResult(OptionDefinitions.RetryPolicy.NetworkTimeout);
}
