// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationFilterValidator
{
    private static readonly string[] AllowedCategories =
    [
        "Cost",
        "HighAvailability",
        "Security",
        "Performance",
        "OperationalExcellence",
    ];

    private static readonly string[] AllowedImpacts = ["High", "Medium", "Low"];

    internal static void Validate(RecommendationListOptions options, ValidationResult validationResult)
    {
        ValidateAllowedValue("--category", options.Category, AllowedCategories, validationResult);
        ValidateAllowedValue("--impact", options.Impact, AllowedImpacts, validationResult);
        ValidateOptionalValue("--resource-type", options.ResourceType, validationResult);
        ValidateOptionalValue("--resource", options.Resource, validationResult);
        ValidateOptionalValue("--search", options.Search, validationResult);
        ValidateOptionalValue("--sub-category", options.SubCategory, validationResult);

        if (options.RecommendationTypeId is not null &&
            !Guid.TryParseExact(options.RecommendationTypeId.Trim(), "D", out _))
        {
            validationResult.Errors.Add(
                $"Invalid --recommendation-type-id value '{options.RecommendationTypeId}'. " +
                "Use a GUID in xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx format.");
        }

        if (options.TrackingIds?.Any(string.IsNullOrWhiteSpace) == true)
        {
            validationResult.Errors.Add("--tracking-ids cannot contain empty values.");
        }

        ServiceRetirementFilterValidator.Validate(
            validationResult,
            options.SubCategory,
            options.TrackingIds,
            options.RetirementDate);

        if (string.Equals(options.Category?.Trim(), "Security", StringComparison.OrdinalIgnoreCase) &&
            (!string.IsNullOrWhiteSpace(options.SubCategory) ||
             options.TrackingIds?.Any(id => !string.IsNullOrWhiteSpace(id)) == true ||
             !string.IsNullOrWhiteSpace(options.RetirementDate)))
        {
            validationResult.Errors.Add(
                "Subcategory, tracking ID, and retirement-date filters are not applicable to Security recommendations.");
        }
    }

    internal static string? NormalizeRecommendationTypeId(string? recommendationTypeId) =>
        Guid.TryParseExact(recommendationTypeId?.Trim(), "D", out var parsed)
            ? parsed.ToString("D")
            : null;

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
        if (normalized.Length == 0 || !allowedValues.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                $"Invalid {optionName} value '{value}'. Allowed values: {string.Join(", ", allowedValues)}.");
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
}