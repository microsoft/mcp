// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Postgres.Validation;

/// <summary>
/// Lightweight validator to reduce risk of executing unsafe SQL statements entered via the tool.
/// Implements a conservative ALLOW list: only a single read-only SELECT statement with common, non-destructive
/// clauses is permitted. No subqueries, CTEs, UNION/INTERSECT/EXCEPT, DDL/DML, or procedural/privileged commands.
/// Identifiers (table / column / alias) are allowed if they don't collide with an explicitly disallowed keyword.
/// Dangerous PostgreSQL functions and system catalogs are explicitly blocked.
/// This is intentionally strict to minimize risk; relax only with strong justification.
/// </summary>
internal static class SqlQueryValidator
{
    private const int MaxQueryLength = 5000; // Arbitrary safety cap to avoid extremely large inputs.
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(3); // 3 second timeout for regex operations

    /// <summary>
    /// SQL set-operation keywords that must be blocked to prevent data-exfiltration attacks
    /// (e.g. SELECT 1 UNION SELECT usename FROM pg_shadow).
    /// </summary>
    private static readonly HashSet<string> DangerousKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "UNION",
        "INTERSECT",
        "EXCEPT",
    };

    /// <summary>
    /// Dangerous PostgreSQL functions and system catalogs that must be blocked even in SELECT queries.
    /// These can read arbitrary files, list directories, access credentials, or enable lateral movement.
    /// </summary>
    private static readonly HashSet<string> DangerousIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        // File system read functions
        "pg_read_file",
        "pg_read_binary_file",

        // Directory listing functions
        "pg_ls_dir",
        "pg_ls_logdir",
        "pg_ls_waldir",
        "pg_ls_tmpdir",
        "pg_ls_archive_statusdir",

        // File write / program execution
        "pg_file_write",
        "pg_execute_server_program",

        // Large object functions (can read/write files via OIDs)
        "lo_import",
        "lo_export",
        "lo_get",
        "lo_put",
        "lo_from_bytea",

        // Credential / auth system catalogs
        "pg_shadow",
        "pg_authid",
        "pg_user",
        "pg_roles",

        // External database access (lateral movement)
        "dblink",
        "dblink_connect",
        "dblink_exec",
        "dblink_send_query",

        // Copy-based exfiltration
        "pg_copy_to",
        "pg_copy_from",

        // Extension management
        "pg_create_extension",

        // Advisory lock abuse
        "pg_advisory_lock",
        "pg_advisory_unlock",

        // File metadata
        "pg_stat_file",

        // Session termination (DoS)
        "pg_terminate_backend",
        "pg_cancel_backend",

        // Server configuration manipulation
        "pg_reload_conf",
        "set_config",
        "current_setting",

        // Denial-of-service
        "pg_sleep",
        "generate_series",

        // Cross-session information leak
        "pg_stat_activity",

        // TLS connection metadata exposure
        "pg_stat_ssl",

        // Foreign data wrapper credential exposure
        "pg_user_mappings",
    };

    /// <summary>
    /// Ensures the provided query is a single, read-only SELECT statement (no comments, no stacked statements).
    /// Throws <see cref="CommandValidationException"/> when validation fails so callers receive a 400 response.
    /// </summary>
    public static void EnsureReadOnlySelect(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new CommandValidationException("Query cannot be empty.", HttpStatusCode.BadRequest);
        }

        var trimmed = query.Trim();

        if (trimmed.Length > MaxQueryLength)
        {
            throw new CommandValidationException($"Query length exceeds limit of {MaxQueryLength} characters.", HttpStatusCode.BadRequest);
        }

        // Allow an optional trailing semicolon; remove for further checks.
        var core = trimmed.EndsWith(';') ? trimmed[..^1] : trimmed;

        // Must start with SELECT (ignoring leading whitespace already trimmed)
        if (!core.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException("Only single read-only SELECT statements are allowed.", HttpStatusCode.BadRequest);
        }

        // Strip single-quoted string literals before checking for comment markers to avoid
        // false positives (e.g., 'foo--bar' or '/* not a comment */' are not comments).
        // Standard literals use only doubled quotes ('') as escape; backslash is literal
        // (standard_conforming_strings = on, the default since PostgreSQL 9.1).
        // E-prefixed strings (E'...') additionally support backslash escapes (e.g., \').
        // The E-string pattern must appear first so the alternation matches it before
        // the standard pattern consumes the opening quote.
        var withoutStrings = Regex.Replace(core, "[eE]'([^'\\\\]|\\\\.|'')*'|'([^']|'')*'", "'str'", RegexOptions.Compiled, RegexTimeout);

        // Reject inline / block comments which can hide stacked statements or alter logic.
        if (withoutStrings.Contains("--", StringComparison.Ordinal) || withoutStrings.Contains("/*", StringComparison.Ordinal))
        {
            throw new CommandValidationException("Comments are not allowed in the query.", HttpStatusCode.BadRequest);
        }

        // Reject any additional semicolons (stacked statements) inside the core content.
        if (core.Contains(';'))
        {
            throw new CommandValidationException("Multiple or stacked SQL statements are not allowed.", HttpStatusCode.BadRequest);
        }

        var lower = core.ToLowerInvariant();

        // Naive detection of tautology patterns still applied before token-level allow list.
        if (lower.Contains(" or 1=1") || lower.Contains(" or '1'='1"))
        {
            throw new CommandValidationException("Suspicious boolean tautology pattern detected.", HttpStatusCode.BadRequest);
        }

        // Tokenize: capture word tokens (letters / underscore). Numerics & punctuation ignored.
        var matches = Regex.Matches(withoutStrings, "[A-Za-z_]+", RegexOptions.Compiled, RegexTimeout);
        if (matches.Count == 0)
        {
            throw new CommandValidationException("Query must contain a SELECT statement.", HttpStatusCode.BadRequest);
        }

        // First significant token must be SELECT.
        if (!matches[0].Value.Equals("select", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException("Only single read-only SELECT statements are allowed.", HttpStatusCode.BadRequest);
        }

        // Check all tokens against blocklist of dangerous functions and system catalogs.
        foreach (Match match in matches)
        {
            if (DangerousIdentifiers.Contains(match.Value))
            {
                throw new CommandValidationException(
                    $"Function or identifier '{match.Value}' is not allowed.",
                    HttpStatusCode.BadRequest);
            }

            if (DangerousKeywords.Contains(match.Value))
            {
                throw new CommandValidationException(
                    $"Query contains dangerous keyword '{match.Value.ToUpperInvariant()}' which is not allowed for security reasons.",
                    HttpStatusCode.BadRequest);
            }
        }
    }
}
