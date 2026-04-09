// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Kusto.Validation;

/// <summary>
/// Validates user-supplied KQL queries to prevent injection and data exfiltration.
/// This is a defense-in-depth measure applied before executing user queries against
/// Azure Data Explorer clusters. While Kusto is inherently read-only for queries,
/// tautology-based attacks can bypass intended row-level filters to expose sensitive data.
/// </summary>
internal static class KqlQueryValidator
{
    private const int MaxQueryLength = 10000;

    // Regex patterns for detecting boolean tautology injection.
    // These catch patterns like: or 1==1, or 1=1, or true, or '1'=='1', etc.
    private static readonly Regex TautologyPattern = RegexHelper.CreateRegex(
        @"\bor\s+(\d+\s*==?\s*\d+|true|'[^']*'\s*==?\s*'[^']*')",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Dangerous management commands that should never appear in user queries
    private static readonly string[] DangerousCommands =
    [
        ".drop",
        ".alter",
        ".create",
        ".delete",
        ".set",
        ".append",
        ".set-or-append",
        ".set-or-replace",
        ".ingest",
        ".purge",
        ".execute",
    ];

    /// <summary>
    /// Validates the KQL query for safety. Throws <see cref="CommandValidationException"/>
    /// when a dangerous pattern is detected.
    /// </summary>
    public static void ValidateQuerySafety(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new CommandValidationException("Query cannot be empty.", HttpStatusCode.BadRequest);
        }

        if (query.Length > MaxQueryLength)
        {
            throw new CommandValidationException(
                $"Query length exceeds the maximum allowed limit of {MaxQueryLength:N0} characters.",
                HttpStatusCode.BadRequest);
        }

        // Strip string literals before structural analysis to avoid false positives
        var queryWithoutStrings = Regex.Replace(query, "'([^']|'')*'", "'str'", RegexOptions.None, RegexHelper.DefaultRegexTimeout);

        // Detect KQL comments (// line comments)
        if (queryWithoutStrings.Contains("//", StringComparison.Ordinal))
        {
            throw new CommandValidationException(
                "KQL comments are not allowed for security reasons.",
                HttpStatusCode.BadRequest);
        }

        // Detect tautology patterns (e.g., or 1==1, or true)
        if (TautologyPattern.IsMatch(queryWithoutStrings))
        {
            throw new CommandValidationException(
                "Suspicious boolean tautology pattern detected. Conditions like 'or 1==1' or 'or true' are not allowed.",
                HttpStatusCode.BadRequest);
        }

        // Detect management/control commands
        var lower = queryWithoutStrings.Trim().ToLowerInvariant();
        foreach (var cmd in DangerousCommands)
        {
            if (lower.StartsWith(cmd, StringComparison.Ordinal) ||
                lower.Contains($"| {cmd}", StringComparison.Ordinal) ||
                lower.Contains($";{cmd}", StringComparison.Ordinal) ||
                lower.Contains($"; {cmd}", StringComparison.Ordinal))
            {
                throw new CommandValidationException(
                    $"Management command '{cmd}' is not allowed in queries for security reasons.",
                    HttpStatusCode.BadRequest);
            }
        }

        // Detect stacked commands via semicolons (after stripping strings)
        var withoutTrailingSemicolon = queryWithoutStrings.TrimEnd();
        if (withoutTrailingSemicolon.EndsWith(';'))
        {
            withoutTrailingSemicolon = withoutTrailingSemicolon[..^1];
        }

        if (withoutTrailingSemicolon.Contains(';', StringComparison.Ordinal))
        {
            throw new CommandValidationException(
                "Multiple KQL statements are not allowed. Use only a single query.",
                HttpStatusCode.BadRequest);
        }
    }
}
