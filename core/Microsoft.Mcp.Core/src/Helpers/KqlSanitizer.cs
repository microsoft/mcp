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
    public static string EscapeStringValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    /// <summary>
    /// Wraps a KQL identifier (table name, column name, etc.) in bracket notation
    /// with proper escaping: <c>['identifier']</c>. This prevents injection when
    /// an identifier is supplied by user input.
    /// </summary>
    public static string EscapeIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        return $"['{identifier.Replace("'", "''", StringComparison.Ordinal)}']";
    }

    /// <summary>
    /// Sanitizes KQL string literals by parsing each single-quoted literal,
    /// normalizing doubled-quote escapes, and re-encoding with proper escaping.
    /// This prevents injection through string literal breakout where a crafted
    /// literal value could escape the quote context and inject KQL operators.
    /// Correctly skips double-quoted strings, verbatim strings (@'...', @"..."),
    /// obfuscated strings (h'...', h@'...'), and line comments (//).
    /// </summary>
    public static string SanitizeStringLiterals(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var result = new StringBuilder(query.Length);
        var i = 0;

        while (i < query.Length)
        {
            // Handle h prefix (obfuscated strings): h'...', h"...", h@'...', h@"..."
            if ((query[i] == 'h' || query[i] == 'H') && i + 1 < query.Length &&
                (query[i + 1] == '\'' || query[i + 1] == '"' || query[i + 1] == '@'))
            {
                if (query[i + 1] == '@' && i + 2 < query.Length && (query[i + 2] == '\'' || query[i + 2] == '"'))
                {
                    // h@'...' or h@"..." — obfuscated verbatim string
                    var quoteChar = query[i + 2];
                    result.Append(query[i]);     // h
                    result.Append(query[i + 1]); // @
                    result.Append(query[i + 2]); // opening quote
                    i += 3;
                    SkipQuotedContent(query, result, ref i, quoteChar);
                }
                else if (query[i + 1] == '\'' || query[i + 1] == '"')
                {
                    // h'...' or h"..." — obfuscated string
                    var quoteChar = query[i + 1];
                    result.Append(query[i]);     // h
                    result.Append(query[i + 1]); // opening quote
                    i += 2;
                    SkipQuotedContent(query, result, ref i, quoteChar);
                }
                else
                {
                    result.Append(query[i]);
                    i++;
                }
            }
            // Handle @ prefix (verbatim strings): @'...', @"..."
            else if (query[i] == '@' && i + 1 < query.Length && (query[i + 1] == '\'' || query[i + 1] == '"'))
            {
                var quoteChar = query[i + 1];
                result.Append(query[i]);     // @
                result.Append(query[i + 1]); // opening quote
                i += 2;
                SkipQuotedContent(query, result, ref i, quoteChar);
            }
            // Handle double-quoted strings: "..."
            else if (query[i] == '"')
            {
                result.Append(query[i]);
                i++;
                SkipQuotedContent(query, result, ref i, '"');
            }
            // Handle line comments: // to end of line
            else if (query[i] == '/' && i + 1 < query.Length && query[i + 1] == '/')
            {
                while (i < query.Length && query[i] != '\n')
                {
                    result.Append(query[i]);
                    i++;
                }
            }
            // Handle single-quoted string literals — sanitize these
            else if (query[i] == '\'')
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

    /// <summary>
    /// Copies characters from <paramref name="query"/> into <paramref name="result"/>
    /// until the matching closing <paramref name="quoteChar"/> is found.
    /// Doubled quotes are treated as escape sequences and preserved as-is.
    /// </summary>
    private static void SkipQuotedContent(string query, StringBuilder result, ref int i, char quoteChar)
    {
        while (i < query.Length)
        {
            result.Append(query[i]);
            if (query[i] == quoteChar)
            {
                i++;
                if (i < query.Length && query[i] == quoteChar)
                {
                    // Doubled quote escape — continue
                    result.Append(query[i]);
                    i++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                i++;
            }
        }
    }
}
