// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class ServiceRetirementFilterValidator
{
    private static readonly string[] AllowedRetirementDateOperators = ["eq", "lt", "le", "gt", "ge"];

    internal static void Validate(
        ValidationResult validationResult,
        string? subCategory,
        IReadOnlyCollection<string>? trackingIds,
        string? retirementDate,
        bool serviceRetirementOnly = false)
    {
        var hasServiceRetirementFilter =
            serviceRetirementOnly ||
            trackingIds?.Any(id => !string.IsNullOrWhiteSpace(id)) == true ||
            !string.IsNullOrWhiteSpace(retirementDate);

        if (hasServiceRetirementFilter &&
            !string.IsNullOrWhiteSpace(subCategory) &&
            !subCategory.Trim().Equals(
                RecommendationMetadataFilters.ServiceRetirementSubCategory,
                StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                "When --sub-category is specified with service-retirement filters or grouping, it must be " +
                $"{RecommendationMetadataFilters.ServiceRetirementSubCategory}.");
        }

        if (!TryParseRetirementDate(retirementDate, out _, out _, out var retirementDateError))
        {
            validationResult.Errors.Add(retirementDateError!);
        }
    }

    internal static bool TryParseRetirementDate(
        string? expression,
        out string? comparisonOperator,
        out DateOnly? retirementDate,
        out string? error)
    {
        comparisonOperator = null;
        retirementDate = null;
        error = null;

        if (expression is null)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            error = "--retirement-date cannot be empty.";
            return false;
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

    internal static string? ResolveSubCategory(string? subCategory, bool serviceRetirementOnly)
    {
        var normalized = string.IsNullOrWhiteSpace(subCategory) ? null : subCategory.Trim();
        if (!serviceRetirementOnly)
        {
            return normalized;
        }

        if (normalized is not null &&
            !normalized.Equals(
                RecommendationMetadataFilters.ServiceRetirementSubCategory,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Service-retirement filters and grouping require the " +
                $"{RecommendationMetadataFilters.ServiceRetirementSubCategory} subcategory.",
                nameof(subCategory));
        }

        return RecommendationMetadataFilters.ServiceRetirementSubCategory;
    }

    internal static string GetKqlComparisonOperator(string comparisonOperator) =>
        comparisonOperator.ToLowerInvariant() switch
        {
            "eq" => "==",
            "lt" => "<",
            "le" => "<=",
            "gt" => ">",
            "ge" => ">=",
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparisonOperator),
                comparisonOperator,
                "Unsupported retirement-date comparison operator.")
        };
}
