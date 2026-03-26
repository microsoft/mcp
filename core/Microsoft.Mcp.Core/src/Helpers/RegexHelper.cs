// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Microsoft.Mcp.Core.Helpers;

public static class RegexHelper
{
    public static readonly TimeSpan DefaultRegexTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Creates a Regex object with a specified pattern and options, and a default timeout to prevent excessive backtracking.
    /// </summary>
    /// <param name="pattern">The regex pattern.</param>
    /// <param name="options">Regex options.</param>
    /// <returns>The regex.</returns>
    /// <exception cref="ArgumentException">If the regex pattern is invalid.</exception>
    public static Regex CreateRegex(string pattern, RegexOptions options)
    {
        try
        {
            return new Regex(pattern, options, DefaultRegexTimeout);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: '{pattern}'", nameof(pattern), ex);
        }
    }
}
