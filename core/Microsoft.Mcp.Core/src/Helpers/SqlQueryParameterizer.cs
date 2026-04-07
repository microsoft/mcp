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
    /// Extracts single-quoted string literals from <paramref name="query"/> and replaces
    /// each one with a numbered parameter placeholder (@p0, @p1, …).
    /// </summary>
    /// <param name="query">The SQL query containing string literals.</param>
    /// <param name="dialect">Which escape sequences to decode. Defaults to <see cref="SqlDialect.Standard"/>.</param>
    /// <returns>
    /// A tuple of the rewritten query and the list of extracted (name, value) pairs.
    /// </returns>
    public static (string Query, List<(string Name, string Value)> Parameters) Parameterize(
        string query,
        SqlDialect dialect = SqlDialect.Standard)
    {
        var parameters = new List<(string Name, string Value)>();
        var result = new StringBuilder(query.Length);
        var paramIndex = 0;
        var i = 0;

        while (i < query.Length)
        {
            if (query[i] == '\'')
            {
                var paramName = $"@p{paramIndex++}";
                var value = new StringBuilder();
                i++; // skip opening quote

                while (i < query.Length)
                {
                    if (dialect == SqlDialect.MySql && query[i] == '\\' && i + 1 < query.Length)
                    {
                        // MySQL backslash escape sequences
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

                parameters.Add((paramName, value.ToString()));
                result.Append(paramName);
            }
            else
            {
                result.Append(query[i]);
                i++;
            }
        }

        return (result.ToString(), parameters);
    }
}
