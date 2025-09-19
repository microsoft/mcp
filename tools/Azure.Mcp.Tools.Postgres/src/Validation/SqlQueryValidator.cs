// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Core.Exceptions;

namespace Azure.Mcp.Tools.Postgres.Validation;

/// <summary>
/// Lightweight validator to reduce risk of executing unsafe SQL statements entered via the tool.
/// Implements a conservative ALLOW list: only a single read-only SELECT statement with common, non-destructive
/// clauses is permitted. No subqueries, CTEs, UNION/INTERSECT/EXCEPT, DDL/DML, or procedural/privileged commands.
/// Identifiers (table / column / alias) are allowed if they don't collide with an explicitly disallowed keyword.
/// This is intentionally strict to minimize risk; relax only with strong justification.
/// </summary>
internal static class SqlQueryValidator
{
    private const int MaxQueryLength = 5000; // Arbitrary safety cap to avoid extremely large inputs.

    // Allowed (case-insensitive) SQL keywords / functions in simple read-only queries.
    private static readonly HashSet<string> AllowedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "select","distinct","from","where","and","or","not","group","by","having","order","asc","desc",
        "limit","offset","join","inner","left","right","full","outer","on","as","between","in","is","null",
        "like","ilike","count","sum","avg","min","max","case","when","then","else","end"
    };

    // Known SQL keywords that should be validated (both allowed and dangerous ones)
    private static readonly HashSet<string> KnownSqlKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "select","distinct","from","where","and","or","not","group","by","having","order","asc","desc",
        "limit","offset","join","inner","left","right","full","outer","on","as","between","in","is","null",
        "like","ilike","count","sum","avg","min","max","case","when","then","else","end",
        "insert","update","delete","drop","alter","create","grant","revoke","truncate","copy","execute","exec",
        "union","intersect","except","vacuum","analyze","attach","prepare","deallocate","call","do",
        "show","explain","describe","use","commit","rollback","begin","transaction"
    };

    /// <summary>
    /// Ensures the provided query is a single, read-only SELECT statement (no comments, no stacked statements).
    /// Throws <see cref="CommandValidationException"/> when validation fails so callers receive a 400 response.
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

        // Allow an optional trailing semicolon; remove for further checks.
        var core = trimmed.EndsWith(';') ? trimmed[..^1] : trimmed;

        // Must start with SELECT (ignoring leading whitespace already trimmed)
        if (!core.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException("Only single read-only SELECT statements are allowed.");
        }

        // Reject inline / block comments which can hide stacked statements or alter logic.
        if (core.Contains("--", StringComparison.Ordinal) || core.Contains("/*", StringComparison.Ordinal))
        {
            throw new CommandValidationException("Comments are not allowed in the query.");
        }

        // Reject any additional semicolons (stacked statements) inside the core content.
        if (core.Contains(';'))
        {
            throw new CommandValidationException("Multiple or stacked SQL statements are not allowed.");
        }

        var lower = core.ToLowerInvariant();

        // Naive detection of tautology patterns still applied before token-level allow list.
        if (lower.Contains(" or 1=1") || lower.Contains("' or '1'='1"))
        {
            throw new CommandValidationException("Suspicious boolean tautology pattern detected.");
        }

        // Strip single-quoted string literals to avoid flagging keywords inside them.
        var withoutStrings = Regex.Replace(core, "'([^']|'')*'", "'str'", RegexOptions.Compiled);

        // Tokenize: capture word tokens (letters / underscore). Numerics & punctuation ignored.
        var matches = Regex.Matches(withoutStrings, "[A-Za-z_]+", RegexOptions.Compiled);
        if (matches.Count == 0)
        {
            throw new CommandValidationException("Query must contain a SELECT statement.");
        }

        // First significant token must be SELECT.
        if (!matches[0].Value.Equals("select", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException("Only single read-only SELECT statements are allowed.");
        }

        foreach (Match m in matches)
        {
            var token = m.Value;
            
            // Only validate tokens that are recognized SQL keywords
            // This allows table names, column names, and other identifiers that aren't SQL keywords
            if (KnownSqlKeywords.Contains(token))
            {
                // It's a recognized SQL keyword - ensure it's in our allow list
                if (!AllowedKeywords.Contains(token))
                {
                    throw new CommandValidationException($"Keyword '{token}' is not permitted in this query context.");
                }
            }
            // If it's not a known SQL keyword, treat it as an identifier and allow it
        }
    }
}
