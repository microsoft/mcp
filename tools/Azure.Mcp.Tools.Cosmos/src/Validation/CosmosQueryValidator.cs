// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Cosmos.Validation;

/// <summary>
/// Lightweight structural validator for Cosmos SQL queries entered via the tool.
/// Cosmos SQL syntax is inherently read-only, so this validator does not restrict SQL keywords,
/// verbs, or tautology patterns. Validation is limited to structural checks: rejecting empty or
/// oversized input, SQL comments, and multiple / stacked statements.
/// </summary>
internal static class CosmosQueryValidator
{
    private const int MaxQueryLength = 5000; // Safety cap similar to Postgres/MySQL validator.
    private const string StringLiteralPlaceholder = "'str'";

    // Regex to strip string literals, replacing them with a placeholder for safe token analysis.
    // Handles standard single-quoted literals and double-quoted strings.
    private static readonly Regex StringLiteralPattern = RegexHelper.CreateRegex(
        @"'([^']|'')*'|""([^""]|"""")*""",
        RegexOptions.Compiled);

    /// <summary>
    /// Validates the structure of the provided query.
    /// </summary>
    /// <param name="query">The SQL query to validate.</param>
    /// <returns>Null if valid; otherwise, an error message describing the issue.</returns>
    public static string? ValidateQuery(string? query)
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

        // Allow an optional trailing semicolon; remove for further checks.
        var core = trimmed.EndsWith(';') ? trimmed[..^1] : trimmed;

        // Strip string literals before checking for comments to avoid false positives
        // (e.g., 'foo--bar' or '/* not a comment */' inside strings are valid values).
        var withoutStrings = StringLiteralPattern.Replace(core, StringLiteralPlaceholder);

        // Reject comments (both inline and block) which can hide stacked statements.
        if (withoutStrings.Contains("--", StringComparison.Ordinal) || withoutStrings.Contains("/*", StringComparison.Ordinal))
        {
            return "Comments are not allowed in the query.";
        }

        // Reject any additional semicolons (stacked statements) inside content.
        if (core.Contains(';'))
        {
            return "Multiple or stacked SQL statements are not allowed.";
        }

        return null;
    }

    /// <summary>
    /// Ensures the provided query is a valid single statement without comments or stacked statements.
    /// </summary>
    [Obsolete("Use ValidateQuery instead.")]
    public static string? EnsureReadOnlySelect(string? query) => ValidateQuery(query);
}
