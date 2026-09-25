// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Parses single-quoted string literals out of SQL queries and replaces them
/// with numbered parameter placeholders (@p0, @p1, …). The extracted literal
/// values can then be bound as database parameters, preventing SQL injection
/// through string literal manipulation.
/// </summary>
public static class SqlQueryParameterizer
{
    /// <summary>
    /// Controls which escape sequences are recognized inside string literals.
    /// </summary>
    public enum SqlDialect
    {
        /// <summary>
        /// SQL-standard escaping only: doubled single quotes ('') represent a literal quote.
        /// Use for PostgreSQL (standard_conforming_strings = on), Cosmos DB SQL, and similar.
        /// </summary>
        Standard,

        /// <summary>
        /// MySQL-compatible escaping: both doubled quotes ('') and backslash sequences
        /// (\', \\, \n, \r, \t, \0, \b, \Z) are recognized.
        /// </summary>
        MySql,
    }

    /// <summary>
    /// SQL keywords that introduce typed literals where the string token is part
    /// of the type syntax and cannot be replaced with a parameter placeholder.
    /// For example: <c>INTERVAL '1 day'</c>, <c>DATE '2024-01-01'</c>.
    /// </summary>
    private static readonly HashSet<string> TypedLiteralKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "DATE", "TIME", "TIMESTAMP", "TIMESTAMPTZ", "TIMETZ", "INTERVAL",
    };

    /// <summary>
    /// Extracts single-quoted string literals from <paramref name="query"/> and replaces
    /// each one with a numbered parameter placeholder (@p0, @p1, …).
    /// Literals that follow typed-literal keywords (e.g. DATE, INTERVAL) are preserved
    /// as-is because the SQL parser requires a literal token in those positions.
    /// </summary>
    /// <param name="query">The SQL query containing string literals.</param>
    /// <param name="dialect">Which escape sequences to decode. Defaults to <see cref="SqlDialect.Standard"/>.</param>
    /// <returns>
    /// A tuple of the rewritten query and the list of extracted (name, value) pairs.
    /// </returns>
    public static (string Query, List<(string Name, string Value)> Parameters) Parameterize(
        string query,
        SqlDialect dialect)
    {
        ArgumentNullException.ThrowIfNull(query);

        var parameters = new List<(string Name, string Value)>();
        var result = new StringBuilder(query.Length);
        var paramIndex = 0;
        var i = 0;

        while (i < query.Length)
        {
            if (query[i] == '\'')
            {
                // Detect PostgreSQL E'...' escape string prefix
                var isEscapeString = false;
                if (i > 0 && (query[i - 1] == 'E' || query[i - 1] == 'e'))
                {
                    // Ensure E/e is a standalone prefix, not part of a longer identifier
                    if (i - 1 == 0 || !(char.IsLetterOrDigit(query[i - 2]) || query[i - 2] == '_'))
                    {
                        isEscapeString = true;
                        // Remove the E/e already appended to result
                        result.Length--;
                    }
                }

                var isTypedLiteral = IsTypedLiteralContext(query, i);
                var literalStart = i;
                var value = new StringBuilder();
                i++; // skip opening quote

                while (i < query.Length)
                {
                    if ((dialect == SqlDialect.MySql || isEscapeString) && query[i] == '\\' && i + 1 < query.Length)
                    {
                        // Backslash escape sequences (MySQL or PostgreSQL E'...')
                        var next = query[i + 1];
                        value.Append(next switch
                        {
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            '0' => '\0',
                            'b' => '\b',
                            'Z' => '\x1A',
                            _ => next // \', \\, \%, \_, and any other \X → X
                        });
                        i += 2;
                    }
                    else if (query[i] == '\'' && i + 1 < query.Length && query[i + 1] == '\'')
                    {
                        // SQL-standard doubled-quote escape
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

                if (isTypedLiteral)
                {
                    // Preserve original literal text for typed-literal contexts
                    if (isEscapeString)
                    {
                        // Re-insert the stripped E/e prefix
                        result.Append(query[literalStart - 1]);
                    }

                    result.Append(query.AsSpan(literalStart, i - literalStart));
                }
                else
                {
                    var paramName = $"@p{paramIndex++}";
                    parameters.Add((paramName, value.ToString()));
                    result.Append(paramName);
                }
            }
            else if (query[i] == '"')
            {
                // Skip double-quoted identifier (standard SQL, Cosmos DB, PostgreSQL)
                result.Append(query[i]);
                i++;
                while (i < query.Length)
                {
                    result.Append(query[i]);
                    if (query[i] == '"')
                    {
                        i++;
                        if (i < query.Length && query[i] == '"')
                        {
                            // Escaped double quote ("") inside identifier
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
            else if (query[i] == '[')
            {
                // Skip bracket-quoted identifier (Cosmos DB, SQL Server)
                result.Append(query[i]);
                i++;
                while (i < query.Length)
                {
                    result.Append(query[i]);
                    if (query[i] == ']')
                    {
                        i++;
                        if (i < query.Length && query[i] == ']')
                        {
                            // Escaped bracket (]]) inside identifier
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
            else if (dialect == SqlDialect.MySql && query[i] == '`')
            {
                // Skip backtick-quoted identifier (MySQL)
                result.Append(query[i]);
                i++;
                while (i < query.Length)
                {
                    result.Append(query[i]);
                    if (query[i] == '`')
                    {
                        i++;
                        if (i < query.Length && query[i] == '`')
                        {
                            // Escaped backtick (``) inside identifier
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
            else if (query[i] == '-' && i + 1 < query.Length && query[i + 1] == '-')
            {
                // Skip single-line comment (-- to end of line)
                while (i < query.Length && query[i] != '\n')
                {
                    result.Append(query[i]);
                    i++;
                }
            }
            else if (query[i] == '/' && i + 1 < query.Length && query[i + 1] == '*')
            {
                // Skip block comment (/* ... */)
                result.Append(query[i]);
                i++;
                result.Append(query[i]);
                i++;
                while (i < query.Length)
                {
                    if (query[i] == '*' && i + 1 < query.Length && query[i + 1] == '/')
                    {
                        result.Append(query[i]);
                        i++;
                        result.Append(query[i]);
                        i++;
                        break;
                    }

                    result.Append(query[i]);
                    i++;
                }
            }
            else
            {
                result.Append(query[i]);
                i++;
            }
        }

        return (result.ToString(), parameters);
    }

    /// <summary>
    /// Checks whether the string literal at <paramref name="quoteIndex"/> is preceded
    /// (ignoring whitespace) by a typed-literal keyword such as DATE or INTERVAL.
    /// </summary>
    private static bool IsTypedLiteralContext(string query, int quoteIndex)
    {
        var j = quoteIndex - 1;

        // Skip whitespace between keyword and opening quote
        while (j >= 0 && char.IsWhiteSpace(query[j]))
        {
            j--;
        }

        if (j < 0 || !(char.IsLetterOrDigit(query[j]) || query[j] == '_'))
        {
            return false;
        }

        var wordEnd = j + 1;
        while (j >= 0 && (char.IsLetterOrDigit(query[j]) || query[j] == '_'))
        {
            j--;
        }

        var word = query.Substring(j + 1, wordEnd - j - 1);
        return TypedLiteralKeywords.Contains(word);
    }
}
