// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationFilterValidator
{
    internal static void Validate(RecommendationListOptions options, ValidationResult validationResult)
    {
        if (options.Status is { } status && !Enum.IsDefined(status))
        {
            validationResult.Errors.Add(
                $"Invalid --status value '{status}'. Allowed values: {string.Join(", ", Enum.GetNames<RecommendationStatus>())}.");
        }

        ValidateOptionalValue("--resource-type", options.ResourceType, validationResult);
        ValidateOptionalValue("--resource", options.Resource, validationResult);
        ValidateOptionalValue("--search", options.Search, validationResult);
        ValidateOptionalValue("--sub-category", options.SubCategory, validationResult);
        ValidateRecommendationTypeId(options.RecommendationTypeId, validationResult);

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
        AdvisorRecommendationCategory? category,
        AdvisorRecommendationImpact? impact,
        string? recommendationTypeId,
        string? resourceType,
        string? resource,
        string? search,
        string? subCategory,
        string? retirementDate,
        bool serviceRetirementOnly = false)
    {
        ValidateAllowedValue("--category", category, validationResult);
        ValidateAllowedValue("--impact", impact, validationResult);
        ValidateOptionalValue("--resource-type", resourceType, validationResult);
        ValidateOptionalValue("--resource", resource, validationResult);
        ValidateOptionalValue("--search", search, validationResult);
        ValidateOptionalValue("--sub-category", subCategory, validationResult);
        ValidateRecommendationTypeId(recommendationTypeId, validationResult);

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

    private static void ValidateRecommendationTypeId(
        string? recommendationTypeId,
        ValidationResult validationResult)
    {
        if (recommendationTypeId is not null &&
            !Guid.TryParseExact(recommendationTypeId.Trim(), "D", out _))
        {
            validationResult.Errors.Add(
                $"Invalid --recommendation-type-id value '{recommendationTypeId}'. " +
                "Use a GUID in xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx format.");
        }
    }

    private static void ValidateAllowedValue<TEnum>(
        string optionName,
        TEnum? value,
        ValidationResult validationResult)
        where TEnum : struct, Enum
    {
        if (value is { } enumValue && !Enum.IsDefined(enumValue))
        {
            validationResult.Errors.Add(
                $"Invalid {optionName} value '{enumValue}'. Allowed values: {string.Join(", ", Enum.GetNames<TEnum>())}.");
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
