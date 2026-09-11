// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Postgres.Validation;

/// <summary>
/// Lightweight structural validator for SQL statements entered via the tool.
/// It does not restrict which SQL verbs may be executed; the command is marked destructive and the
/// caller's database permissions are the authority on what is allowed. Validation is limited to
/// rejecting empty or oversized input, SQL comments, and multiple / stacked statements.
/// </summary>
internal static class SqlQueryValidator
{
    private const int MaxQueryLength = 5000; // Arbitrary safety cap to avoid extremely large inputs.

    /// <summary>
    /// Ensures the provided query is a single statement without comments and within the size limit.
    /// Throws <see cref="CommandValidationException"/> when validation fails so callers receive a 400 response.
    /// </summary>
    public static void ValidateQuery(string? query)
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

        // Strip single-quoted string literals before checking for comment markers to avoid
        // false positives (e.g., 'foo--bar' or '/* not a comment */' are not comments).
        // Standard literals use only doubled quotes ('') as escape; backslash is literal
        // (standard_conforming_strings = on, the default since PostgreSQL 9.1).
        // E-prefixed strings (E'...') additionally support backslash escapes (e.g., \').
        // The E-string pattern must appear first so the alternation matches it before
        // the standard pattern consumes the opening quote.
        var withoutStrings = Regex.Replace(core, "[eE]'([^'\\\\]|\\\\.|'')*'|'([^']|'')*'", "'str'", RegexOptions.Compiled, RegexHelper.DefaultRegexTimeout);

        // Reject inline / block comments which can hide stacked statements or alter logic.
        if (withoutStrings.Contains("--", StringComparison.Ordinal) || withoutStrings.Contains("/*", StringComparison.Ordinal))
        {
            throw new CommandValidationException("Comments are not allowed in the query.");
        }

        // Reject any additional semicolons (stacked statements) inside the core content.
        if (core.Contains(';'))
        {
            throw new CommandValidationException("Multiple or stacked SQL statements are not allowed.");
        }
    }
}
