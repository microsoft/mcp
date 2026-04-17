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
