// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Cosmos.Validation;

/// <summary>
/// Lightweight validator to reduce risk of executing unsafe queries via the Cosmos SQL query tool.
/// Cosmos SQL syntax is inherently read-only (SELECT only), but this validator blocks:
/// 1. Multiple/stacked statements that could bypass security
/// 2. Comments that could hide malicious code
/// 3. Attempts to execute stored procedures/triggers (which CAN modify data)
/// 4. Common SQL injection patterns including boolean tautologies
/// Note: Stored procedures and triggers are executed via SDK APIs, not SQL queries, so they cannot
/// be invoked through this query interface. This is defense-in-depth validation.
/// Prefer using parameterized queries (QueryDefinition with parameters) over string concatenation
/// to prevent injection at the source.
/// </summary>
internal static class CosmosQueryValidator
{
    private const int MaxQueryLength = 5000; // Safety cap similar to Postgres validator.
    private const string StringLiteralPlaceholder = "'str'";

    // Regex to strip string literals, replacing them with a placeholder for safe token analysis.
    private static readonly Regex StringLiteralPattern = RegexHelper.CreateRegex(
        "'([^']|'')*'",
        RegexOptions.Compiled);

    // Matches: OR <word> = <same_word> with optional whitespace around =
    // Catches: OR 1=1, OR 2=2, OR a=a, OR 1 = 1, etc.
    private static readonly Regex TautologyIdentifierPattern = RegexHelper.CreateRegex(
        @"\bor\s+(\w+)\s*=\s*\1\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: OR '<literal>' = '<same_literal>' using a backreference on the original query.
    // Only flags when both sides are identical (e.g., OR 'x'='x'), not different values (OR 'a'='b').
    private static readonly Regex TautologyStringLiteralPattern = RegexHelper.CreateRegex(
        @"\bor\s+'((?:[^']|'')*)'\s*=\s*'\1'",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: OR true / OR TRUE (boolean tautology)
    private static readonly Regex TautologyBooleanPattern = RegexHelper.CreateRegex(
        @"\bor\s+true\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Keywords/patterns that might indicate attempts to execute stored procedures or triggers
    private static readonly HashSet<string> BlockedPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "exec", "execute", "trigger", "sproc", "storedprocedure", "call"
    };

    /// <summary>
    /// Ensures the provided query is a single read-only SELECT statement.
    /// </summary>
    /// <param name="query">The SQL query to validate.</param>
    /// <returns>Null if valid; otherwise, an error message describing the issue.</returns>
    public static string? EnsureReadOnlySelect(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Query cannot be empty.";
        }

        var trimmed = query.Trim();
        if (trimmed.Length > MaxQueryLength)
        {
            return $"Query length exceeds limit of {MaxQueryLength} characters.";
        }

        // Remove a single trailing semicolon (users sometimes add it) then reject others.
        var core = trimmed.EndsWith(';') ? trimmed[..^1] : trimmed;

        // Must start with SELECT (Cosmos queries always start with SELECT ... FROM or SELECT VALUE ...)
        if (!core.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            return "Only single read-only SELECT statements are allowed.";
        }

        // Reject comments (both inline and block) to avoid hiding stacked statements.
        if (core.Contains("--", StringComparison.Ordinal) || core.Contains("/*", StringComparison.Ordinal))
        {
            return "Comments are not allowed in the query.";
        }

        // Reject any additional semicolons (stacked statements) inside content.
        if (core.Contains(';'))
        {
            return "Multiple or stacked SQL statements are not allowed.";
        }

        // Strip string literals before tautology and keyword checks to avoid false positives
        // from values inside quoted strings and to normalize injection patterns that break out of strings.
        var withoutStrings = StringLiteralPattern.Replace(core, StringLiteralPlaceholder);

        // Detect boolean tautology injection patterns (e.g., OR 1=1, OR 2=2, OR a=a, OR 'x'='x', OR true)
        // TautologyStringLiteralPattern uses a backreference so it must run against the original query.
        // All patterns use RegexOptions.IgnoreCase so no lowercasing is needed.
        if (TautologyIdentifierPattern.IsMatch(withoutStrings) ||
            TautologyStringLiteralPattern.IsMatch(core) ||
            TautologyBooleanPattern.IsMatch(withoutStrings))
        {
            return "Suspicious boolean tautology pattern detected.";
        }

        // Check for attempts to execute stored procedures or triggers
        // While these cannot be executed via SQL queries, this is defense-in-depth
        var matches = Regex.Matches(withoutStrings, "[A-Za-z_]+", RegexOptions.Compiled, RegexHelper.DefaultRegexTimeout);

        foreach (Match m in matches)
        {
            var token = m.Value;
            // Check for exact matches or identifiers that start with blocked patterns
            if (BlockedPatterns.Contains(token) || token.StartsWith("sp_", StringComparison.OrdinalIgnoreCase))
            {
                return $"Keyword '{token}' is not permitted. Stored procedures and triggers cannot be executed through SQL queries.";
            }
        }

        return null; // Valid query
    }
}
