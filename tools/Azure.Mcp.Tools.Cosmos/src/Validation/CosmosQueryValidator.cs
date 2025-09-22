// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Core.Exceptions;

namespace Azure.Mcp.Tools.Cosmos.Validation;

/// <summary>
/// Lightweight validator to reduce risk of executing unsafe Cosmos SQL statements entered via the tool.
/// Only a single read-only SELECT statement targeting container aliases (e.g., c) is allowed.
/// Disallows DDL/DML, multiple statements, comments that could hide stacked statements, and UNION/INTERSECT/EXCEPT.
/// This is intentionally conservative; relax only with strong justification.
/// </summary>
internal static class CosmosQueryValidator
{
    private const int MaxQueryLength = 5000; // Safety cap similar to Postgres validator.
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(3); // Prevent ReDoS attacks

    // Allowed (case-insensitive) Cosmos SQL keywords / functions in simple read-only queries.
    private static readonly HashSet<string> AllowedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "select","value","distinct","top","from","where","and","or","not","in","between","is","null",
        "like","order","by","asc","desc","offset","limit","join","as","count","sum","avg","min","max",
        "case","when","then","else","end"
    };

    // Known Cosmos SQL keywords that should be validated (both allowed and dangerous ones)
    private static readonly HashSet<string> KnownCosmosKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "select","value","distinct","top","from","where","and","or","not","in","between","is","null",
        "like","order","by","asc","desc","offset","limit","join","as","count","sum","avg","min","max",
        "case","when","then","else","end",
        "insert","update","delete","drop","alter","create","replace","upsert","grant","revoke","truncate",
        "union","intersect","except","exec","execute"
    };

    /// <summary>
    /// Ensures the provided query is a single read-only SELECT statement.
    /// Throws <see cref="CommandValidationException"/> for invalid input so caller surfaces 400.
    /// </summary>
    public static void EnsureReadOnlySelect(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new CommandValidationException("Query cannot be empty.");
        }

        var trimmed = query.Trim();
        if (trimmed.Length > MaxQueryLength)
        {
            throw new CommandValidationException($"Query length exceeds limit of {MaxQueryLength} characters.");
        }

        // Remove a single trailing semicolon (users sometimes add it) then reject others.
        var core = trimmed.EndsWith(';') ? trimmed[..^1] : trimmed;

        // Must start with SELECT (Cosmos queries always start with SELECT ... FROM or SELECT VALUE ...)
        if (!core.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException("Only single read-only SELECT statements are allowed.");
        }

        // Reject comments (both inline and block) to avoid hiding stacked statements.
        if (core.Contains("--", StringComparison.Ordinal) || core.Contains("/*", StringComparison.Ordinal))
        {
            throw new CommandValidationException("Comments are not allowed in the query.");
        }

        // Reject any additional semicolons (stacked statements) inside content.
        if (core.Contains(';'))
        {
            throw new CommandValidationException("Multiple or stacked SQL statements are not allowed.");
        }

        var lower = core.ToLowerInvariant();
        if (lower.Contains(" or 1=1") || lower.Contains("' or '1'='1"))
        {
            throw new CommandValidationException("Suspicious boolean tautology pattern detected.");
        }

        // Strip single-quoted string literals to avoid flagging keywords inside them.
        var withoutStrings = Regex.Replace(core, "'([^']|'')*'", "'str'", RegexOptions.Compiled, RegexTimeout);

        // Tokenize: letters / underscore. Numbers & punctuation ignored.
        var matches = Regex.Matches(withoutStrings, "[A-Za-z_]+", RegexOptions.Compiled, RegexTimeout);
        if (matches.Count == 0)
        {
            throw new CommandValidationException("Query must contain a SELECT statement.");
        }

        // First token must be select (already checked; double guard)
        if (!matches[0].Value.Equals("select", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException("Only single read-only SELECT statements are allowed.");
        }

        foreach (Match m in matches)
        {
            var token = m.Value;

            // Only validate tokens that are recognized Cosmos SQL keywords
            // This allows table names, column names, and other identifiers that aren't SQL keywords
            if (KnownCosmosKeywords.Contains(token))
            {
                // It's a recognized Cosmos SQL keyword - ensure it's in our allow list
                if (!AllowedKeywords.Contains(token))
                {
                    throw new CommandValidationException($"Keyword '{token}' is not permitted in this query context.");
                }
            }
            // If it's not a known Cosmos SQL keyword, treat it as an identifier and allow it
        }
    }
}
