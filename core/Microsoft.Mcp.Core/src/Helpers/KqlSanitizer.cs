// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Provides escaping and sanitization utilities for Kusto Query Language (KQL)
/// strings and identifiers, preventing injection attacks when constructing
/// KQL queries with user-supplied values.
/// </summary>
public static class KqlSanitizer
{
    /// <summary>
    /// Escapes a single-quoted KQL string value by doubling any embedded single quotes.
    /// Use this when interpolating a value into a KQL <c>where</c> clause, e.g.:
    /// <c>$"MyTable | where Name == '{KqlSanitizer.EscapeStringValue(name)}'"</c>
    /// </summary>
    public static string EscapeStringValue(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);

    /// <summary>
    /// Wraps a KQL identifier (table name, column name, etc.) in bracket notation
    /// with proper escaping: <c>['identifier']</c>. This prevents injection when
    /// an identifier is supplied by user input.
    /// </summary>
    public static string EscapeIdentifier(string identifier) =>
        $"['{identifier.Replace("'", "''", StringComparison.Ordinal)}']";

    /// <summary>
    /// Sanitizes KQL string literals by parsing each single-quoted literal,
    /// decoding escape sequences, and re-encoding with proper escaping.
    /// This prevents injection through string literal breakout where a crafted
    /// literal value could escape the quote context and inject KQL operators.
    /// </summary>
    public static string SanitizeStringLiterals(string query)
    {
        var result = new StringBuilder(query.Length);
        var i = 0;

        while (i < query.Length)
        {
            if (query[i] == '\'')
            {
                var value = new StringBuilder();
                i++; // skip opening quote

                while (i < query.Length)
                {
                    if (query[i] == '\'' && i + 1 < query.Length && query[i + 1] == '\'')
                    {
                        // KQL doubled-quote escape
                        value.Append('\'');
                        i += 2;
                    }
                    else if (query[i] == '\'')
                    {
                        // End of string literal
                        i++;
                        break;
                    }
                    else
                    {
                        value.Append(query[i]);
                        i++;
                    }
                }

                // Re-encode with proper escaping
                result.Append('\'');
                result.Append(value.ToString().Replace("'", "''", StringComparison.Ordinal));
                result.Append('\'');
            }
            else
            {
                result.Append(query[i]);
                i++;
            }
        }

        return result.ToString();
    }
}
