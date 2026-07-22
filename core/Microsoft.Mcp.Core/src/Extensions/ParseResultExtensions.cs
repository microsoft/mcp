// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Microsoft.Mcp.Core.Extensions;

internal static class ParseResultExtensions
{
    /// <summary>
    /// Attempts to retrieve the ParseReult value for the Option. If found it is returned, otherwise the default
    /// value for T is returned.
    /// </summary>
    /// <typeparam name="T">The type of the option value</typeparam>
    /// <param name="parseResult">The parse result</param>
    /// <param name="option">The option</param>
    /// <returns>The value of the option, or the default value if not found or not set</returns>
    internal static T? GetValueOrDefault<T>(this ParseResult parseResult, Option<T> option)
        => parseResult.CommandResult.GetValueOrDefault(option);
}
