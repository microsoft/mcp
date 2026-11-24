// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Validation;

internal static class TraceQueryValidator
{
    internal static bool IsFieldReference(string? value) =>
        value is { Length: > 0 and <= 1024 } &&
        Regex.IsMatch(value, @"\A[a-zA-Z_][a-zA-Z0-9_.]*\z", RegexOptions.NonBacktracking);

    public static void ValidateWindow(DateTimeOffset start, DateTimeOffset end, ValidationResult result)
    {
        if (start >= end || end - start > TimeSpan.FromDays(1))
        {
            result.AddError("The time range must be positive and no longer than one day.", "Invalid time range.");
        }
    }

    public static void ValidateSubtreePrefixes(
        string[]? excludePrefixes,
        string[]? deprioritizePrefixes,
        Action<string, string> addError)
    {
        if (excludePrefixes?.Any(string.IsNullOrWhiteSpace) == true)
        {
            addError("Exclude subtree prefixes must not be empty or whitespace.", "Invalid exclude subtree prefixes.");
        }
        if (deprioritizePrefixes?.Any(string.IsNullOrWhiteSpace) == true)
        {
            addError("Deprioritize subtree prefixes must not be empty or whitespace.", "Invalid deprioritize subtree prefixes.");
        }
        if (excludePrefixes is { } excludes
            && deprioritizePrefixes is { } lowerPriorityPrefixes
            && !excludes.Any(string.IsNullOrWhiteSpace)
            && !lowerPriorityPrefixes.Any(string.IsNullOrWhiteSpace)
            && excludes.Intersect(lowerPriorityPrefixes, StringComparer.Ordinal).Any())
        {
            addError("The exact same subtree prefix cannot appear in both prefix lists.", "Conflicting subtree prefixes.");
        }
    }
}
