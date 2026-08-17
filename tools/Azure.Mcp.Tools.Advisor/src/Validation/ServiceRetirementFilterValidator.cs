// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

/// <summary>
/// Validates the service-retirement filter options shared by the Advisor metadata and
/// recommendation list commands: <c>--sub-category</c>, tracking IDs and
/// <c>--retirement-date</c>. Tracking IDs and retirement date are only meaningful for the
/// <c>ServiceUpgradeAndRetirement</c> subcategory, so a conflicting subcategory is rejected.
/// </summary>
internal static class ServiceRetirementFilterValidator
{
    private static readonly string[] AllowedRetirementDateOperators = ["eq", "lt", "le", "gt", "ge"];

    /// <summary>
    /// Adds an error to <paramref name="validationResult"/> for a subcategory that conflicts with the
    /// service-retirement filters, or for a malformed <c>--retirement-date</c> expression.
    /// </summary>
    internal static void Validate(
        ValidationResult validationResult,
        string? subCategory,
        IReadOnlyCollection<string>? trackingIds,
        string? retirementDate)
    {
        var hasServiceRetirementFilter =
            trackingIds?.Any(id => !string.IsNullOrWhiteSpace(id)) == true ||
            !string.IsNullOrWhiteSpace(retirementDate);
        var normalizedSubCategory = subCategory?.Trim();

        if (hasServiceRetirementFilter &&
            !string.IsNullOrWhiteSpace(normalizedSubCategory) &&
            !normalizedSubCategory.Equals(
                RecommendationMetadataFilters.ServiceRetirementSubCategory,
                StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                "Service-retirement filters are only valid with --sub-category " +
                $"{RecommendationMetadataFilters.ServiceRetirementSubCategory}.");
        }

        if (!TryParseRetirementDate(retirementDate, out _, out _, out var retirementDateError))
        {
            validationResult.Errors.Add(retirementDateError!);
        }
    }

    /// <summary>
    /// Parses a <c>--retirement-date</c> expression in <c>&lt;operator&gt;:&lt;yyyy-MM-dd&gt;</c> form.
    /// An absent expression is valid and yields no operator or date.
    /// </summary>
    internal static bool TryParseRetirementDate(
        string? expression,
        out string? comparisonOperator,
        out DateOnly? retirementDate,
        out string? error)
    {
        comparisonOperator = null;
        retirementDate = null;
        error = null;

        if (string.IsNullOrWhiteSpace(expression))
        {
            return true;
        }

        var parts = expression.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            !AllowedRetirementDateOperators.Contains(parts[0], StringComparer.OrdinalIgnoreCase))
        {
            error = "Invalid --retirement-date value. Use '<operator>:<yyyy-MM-dd>' with operator eq, lt, le, gt, or ge; for example, --retirement-date ge:2026-03-01.";
            return false;
        }

        if (!DateOnly.TryParseExact(
            parts[1],
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate))
        {
            error = "Invalid --retirement-date date. Use ISO date format yyyy-MM-dd, for example ge:2026-03-31.";
            return false;
        }

        comparisonOperator = parts[0].ToLowerInvariant();
        retirementDate = parsedDate;
        return true;
    }
}
