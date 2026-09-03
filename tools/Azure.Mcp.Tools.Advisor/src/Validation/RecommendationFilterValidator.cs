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
    }

    internal static string? NormalizeRecommendationTypeId(string? recommendationTypeId) =>
        Guid.TryParseExact(recommendationTypeId?.Trim(), "D", out var parsed)
            ? parsed.ToString("D")
            : null;

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
