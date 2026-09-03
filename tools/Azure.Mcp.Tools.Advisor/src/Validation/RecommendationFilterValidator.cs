// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationFilterValidator
{
    internal static readonly string[] AllowedCategories =
    [
        "Cost",
        "HighAvailability",
        "Security",
        "Performance",
        "OperationalExcellence",
    ];

    internal static readonly string[] AllowedImpacts = ["High", "Medium", "Low"];

    internal static void ValidateCommon(
        ValidationResult validationResult,
        string? category,
        string? impact,
        string? recommendationTypeId,
        string? resourceType,
        string? resource,
        string? search,
        string? subCategory,
        string? retirementDate,
        bool serviceRetirementOnly = false)
    {
        ValidateAllowedValue("--category", category, AllowedCategories, validationResult);
        ValidateAllowedValue("--impact", impact, AllowedImpacts, validationResult);
        ValidateOptionalValue("--resource-type", resourceType, validationResult);
        ValidateOptionalValue("--resource", resource, validationResult);
        ValidateOptionalValue("--search", search, validationResult);
        ValidateOptionalValue("--sub-category", subCategory, validationResult);

        if (recommendationTypeId is not null &&
            !Guid.TryParseExact(recommendationTypeId.Trim(), "D", out _))
        {
            validationResult.Errors.Add(
                $"Invalid --recommendation-type-id value '{recommendationTypeId}'. " +
                "Use a GUID in xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx format.");
        }

        ServiceRetirementFilterValidator.Validate(
            validationResult,
            subCategory,
            trackingIds: null,
            retirementDate,
            serviceRetirementOnly);
    }

    internal static string? NormalizeRecommendationTypeId(string? recommendationTypeId) =>
        Guid.TryParseExact(recommendationTypeId?.Trim(), "D", out var parsed)
            ? parsed.ToString("D")
            : null;

    internal static string? NormalizeAllowedValue(string? value, IReadOnlyCollection<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return allowedValues.FirstOrDefault(
            candidate => candidate.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
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
