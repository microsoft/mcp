// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationFilterValidator
{
    internal const string PrioritizationSignalsRequiresPrioritized =
        "--show-prioritization-signals true requires --prioritized true.";
    internal const string PrioritizedRequiresRecommendationType =
        "--prioritized true requires exactly one distinct --recommendation-type-ids value. Use recommendation summary grouped by recommendation-type for initial cross-type prioritization, then list the selected type.";

    internal static readonly string[] AllowedCategories =
    [
        "Cost",
        "HighAvailability",
        "Security",
        "Performance",
        "OperationalExcellence",
    ];

    internal static readonly string[] AllowedImpacts = ["High", "Medium", "Low"];

    private static readonly string[] AllowedStatuses =
    [
        nameof(RecommendationStatus.New),
        nameof(RecommendationStatus.Postponed),
        nameof(RecommendationStatus.Dismissed),
        nameof(RecommendationStatus.Completed),
    ];

    internal static void Validate(RecommendationListOptions options, ValidationResult validationResult)
    {
        if (options.ShowPrioritizationSignals == true && options.Prioritized != true)
        {
            validationResult.Errors.Add(PrioritizationSignalsRequiresPrioritized);
        }

        var recommendationTypeIds = options.RecommendationTypeIds;
        if (options.Prioritized == true && NormalizeRecommendationTypeIds(recommendationTypeIds).Count != 1)
        {
            validationResult.Errors.Add(PrioritizedRequiresRecommendationType);
        }

        ValidateAllowedValue("--category", options.Category, AllowedCategories, validationResult);
        ValidateAllowedValue("--impact", options.Impact, AllowedImpacts, validationResult);
        ValidateAllowedValue("--status", options.Status?.ToString(), AllowedStatuses, validationResult);
        ValidateOptionalValues("--resource-types", options.ResourceTypes, validationResult);
        ValidateOptionalValue("--product-family", options.ProductFamily, validationResult);
        ValidateOptionalValue("--resource", options.Resource, validationResult);
        ValidateOptionalValue("--search", options.Search, validationResult);
        ValidateOptionalValue("--sub-category", options.SubCategory, validationResult);
        ValidateRecommendationTypeIds(recommendationTypeIds, validationResult);

        if (options.TrackingIds?.Any(string.IsNullOrWhiteSpace) == true)
        {
            validationResult.Errors.Add("--tracking-ids cannot contain empty values.");
        }

        ServiceRetirementFilterValidator.Validate(
            validationResult,
            options.SubCategory,
            options.TrackingIds,
            options.RetirementDate);
    }

    internal static void ValidateCommon(
        ValidationResult validationResult,
        string? category,
        string? impact,
        IEnumerable<string>? recommendationTypeIds,
        IEnumerable<string>? resourceTypes,
        string? productFamily,
        string? resource,
        string? search,
        string? subCategory,
        string? retirementDate,
        bool serviceRetirementOnly = false)
    {
        ValidateAllowedValue("--category", category, AllowedCategories, validationResult);
        ValidateAllowedValue("--impact", impact, AllowedImpacts, validationResult);
        ValidateOptionalValues("--resource-types", resourceTypes, validationResult);
        ValidateOptionalValue("--product-family", productFamily, validationResult);
        ValidateOptionalValue("--resource", resource, validationResult);
        ValidateOptionalValue("--search", search, validationResult);
        ValidateOptionalValue("--sub-category", subCategory, validationResult);
        ValidateRecommendationTypeIds(recommendationTypeIds, validationResult);

        ServiceRetirementFilterValidator.Validate(
            validationResult,
            subCategory,
            trackingIds: null,
            retirementDate,
            serviceRetirementOnly);
    }

    internal static void ValidateMinimumSavings(decimal? minimumSavings, ValidationResult validationResult)
    {
        if (minimumSavings is < 0)
        {
            validationResult.Errors.Add("--minimum-savings must be non-negative.");
        }
    }

    internal static void ValidateCostOnlyFilters(
        string? category,
        string? subCategory,
        decimal? minimumSavings,
        DateTimeOffset? asOf,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ValidationResult validationResult)
    {
        ValidateMinimumSavings(minimumSavings, validationResult);

        if ((minimumSavings is not null || asOf is not null || from is not null || to is not null) &&
            !RecommendationQueryBuilder.IsIdleResourceCost(category, subCategory))
        {
            validationResult.Errors.Add(
                "--minimum-savings, --as-of, --from, and --to require --category Cost and --sub-category IdleResources.");
        }
    }

    internal static string? NormalizeRecommendationTypeId(string? recommendationTypeId) =>
        Guid.TryParseExact(recommendationTypeId?.Trim(), "D", out var parsed)
            ? parsed.ToString("D")
            : null;

    internal static IReadOnlyList<string> NormalizeRecommendationTypeIds(IEnumerable<string>? recommendationTypeIds) =>
        (recommendationTypeIds ?? [])
            .Select(NormalizeRecommendationTypeId)
            .Where(static recommendationTypeId => recommendationTypeId is not null)
            .Select(static recommendationTypeId => recommendationTypeId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    internal static IEnumerable<string> CombineValues(string? singular, IEnumerable<string>? plural) =>
        singular is null ? plural ?? [] : (plural ?? []).Prepend(singular);

    internal static string[] NormalizeFilterValues(IEnumerable<string>? values) =>
        (values ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    internal static string? NormalizeAllowedValue(
        string? value,
        IReadOnlyCollection<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return allowedValues.FirstOrDefault(
            candidate => candidate.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static void ValidateRecommendationTypeId(
        string? recommendationTypeId,
        ValidationResult validationResult)
    {
        if (recommendationTypeId is not null &&
            !Guid.TryParseExact(recommendationTypeId.Trim(), "D", out _))
        {
            validationResult.AddError(
                $"Invalid --recommendation-type-ids value '{recommendationTypeId}'. " +
                "Use a GUID in xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx format.",
                "Invalid Advisor recommendation type ID.");
        }
    }

    private static void ValidateRecommendationTypeIds(
        IEnumerable<string>? recommendationTypeIds,
        ValidationResult validationResult)
    {
        if (recommendationTypeIds is null)
        {
            return;
        }

        foreach (var recommendationTypeId in recommendationTypeIds)
        {
            if (recommendationTypeId is null)
            {
                validationResult.Errors.Add("--recommendation-type-ids cannot contain null values.");
                continue;
            }
            ValidateRecommendationTypeId(recommendationTypeId, validationResult);
        }
    }

    private static void ValidateAllowedValue(
        string optionName,
        string? value,
        IReadOnlyCollection<string> allowedValues,
        ValidationResult validationResult)
    {
        if (value is null)
        {
            return;
        }

        var normalized = value.Trim();
        if (normalized.Length == 0 ||
            !allowedValues.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"Invalid {optionName} value '{value}'. Allowed values: {string.Join(", ", allowedValues)}.",
                "Invalid Advisor filter value.");
        }
    }

    private static void ValidateOptionalValue(
        string optionName,
        string? value,
        ValidationResult validationResult)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
        {
            validationResult.Errors.Add($"{optionName} cannot be empty.");
        }
    }

    private static void ValidateOptionalValues(
        string optionName,
        IEnumerable<string>? values,
        ValidationResult validationResult)
    {
        if (values is null)
        {
            return;
        }

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                validationResult.Errors.Add($"{optionName} cannot contain empty values.");
            }
        }
    }
}
