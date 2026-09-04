// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Metadata;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Metadata;

[CommandMetadata(
    Id = "16c9c57e-8f14-43bd-91da-d1548b6af72e",
    Name = "list",
    Title = "List Advisor Recommendation Metadata",
    Description = "List Azure Advisor recommendation metadata, also known as recommendation types. " +
                  "Use this tool before deploying resources such as virtual machines to discover what recommendations Advisor could produce, even when there are no active recommendations. " +
                  "Show Advisor service retirements on, before, or after a specified retirement date, or find service-retirement metadata by Service Health tracking ID. " +
                  "The global Azure Resource Graph catalog supports greenfield discovery and resource-type filtering for brownfield onboarding. " +
                  "Optional filters include language, resource type, impact, category, subcategory, tracking ID, and retirement date. " +
                  "Returns localized type IDs, names, categories, subcategories, impact, priority, descriptions, benefits, actions, scope, source query, and service-retirement details, " +
                  "ordered by impact from High to Medium to Low and then by display name.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationMetadataListCommand(
    ILogger<RecommendationMetadataListCommand> logger,
    IAdvisorService advisorService)
    : AuthenticatedCommand<
        RecommendationMetadataListOptions,
        RecommendationMetadataListCommand.RecommendationMetadataListResult>()
{
    private static readonly string[] AllowedImpacts = ["High", "Medium", "Low"];
    private static readonly string[] AllowedCategories =
    [
        "Cost",
        "HighAvailability",
        "Security",
        "Performance",
        "OperationalExcellence",
    ];
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "cs", "de", "es", "fr", "hu", "id", "it", "ja", "ko",
        "nl", "pl", "pt-br", "pt-pt", "ru", "sv", "tr", "zh-hans", "zh-hant",
    };

    private readonly ILogger<RecommendationMetadataListCommand> _logger = logger;
    private readonly IAdvisorService _advisorService = advisorService;

    public override void ValidateOptions(
        RecommendationMetadataListOptions options,
        ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!TryNormalizeLanguage(options.Language, out _))
        {
            validationResult.Errors.Add(
                $"Unsupported --language value '{options.Language}'. Supported values: " +
                $"{string.Join(", ", SupportedLanguages.OrderBy(l => l, StringComparer.Ordinal))}.");
        }

        var normalizedImpact = options.Impact?.Trim();
        if (!string.IsNullOrEmpty(normalizedImpact) &&
            !AllowedImpacts.Contains(normalizedImpact, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                $"Invalid --impact value '{options.Impact}'. Allowed values: {string.Join(", ", AllowedImpacts)}.");
        }

        var normalizedCategory = options.Category?.Trim();
        if (!string.IsNullOrEmpty(normalizedCategory) &&
            !AllowedCategories.Contains(normalizedCategory, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                $"Invalid --category value '{options.Category}'. Allowed values: {string.Join(", ", AllowedCategories)}.");
        }

        ServiceRetirementFilterValidator.Validate(
            validationResult,
            options.SubCategory,
            NormalizeTrackingIds(options.TrackingId),
            options.RetirementDate);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RecommendationMetadataListOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _ = TryNormalizeLanguage(options.Language, out var language);
            var impact = NormalizeImpact(options.Impact);
            _ = ServiceRetirementFilterValidator.TryParseRetirementDate(
                options.RetirementDate,
                out var retirementDateOperator,
                out var retirementDate,
                out _);

            var filters = new RecommendationMetadataFilters(
                ResourceType: NormalizeOptionalFilter(options.ResourceType),
                Impact: impact,
                Category: NormalizeAllowedValue(options.Category, AllowedCategories),
                SubCategory: NormalizeOptionalFilter(options.SubCategory),
                TrackingIds: NormalizeTrackingIds(options.TrackingId),
                RetirementDateOperator: retirementDateOperator,
                RetirementDate: retirementDate);

            var metadata = await _advisorService.ListRecommendationMetadataAsync(
                language,
                filters,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(metadata.Results, metadata.AreResultsTruncated),
                AdvisorJsonContext.Default.RecommendationMetadataListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor recommendation metadata. Language: {Language}, ResourceType: {ResourceType}, Impact: {Impact}, Category: {Category}, SubCategory: {SubCategory}.",
                options.Language, options.ResourceType, options.Impact, options.Category, options.SubCategory);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed accessing Advisor metadata in Azure Resource Graph. Verify the signed-in identity has Reader access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendation metadata was not found in Azure Resource Graph.",
        RequestFailedException =>
            "Failed to query Advisor recommendation metadata in Azure Resource Graph.",
        _ => base.GetErrorMessage(ex)
    };

    private static bool TryNormalizeLanguage(string? language, out string normalized)
    {
        normalized = (language ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            normalized = "en";
            return true;
        }

        if (SupportedLanguages.TryGetValue(normalized, out var exact))
        {
            normalized = exact;
            return true;
        }

        var dash = normalized.IndexOf('-');
        if (dash > 0 && SupportedLanguages.TryGetValue(normalized[..dash], out var baseMatch))
        {
            normalized = baseMatch;
            return true;
        }

        return false;
    }

    private static string? NormalizeImpact(string? impact)
        => NormalizeAllowedValue(impact, AllowedImpacts);

    private static string? NormalizeAllowedValue(string? value, IReadOnlyList<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return allowedValues.FirstOrDefault(
            candidate => candidate.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeOptionalFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[]? NormalizeTrackingIds(string? trackingId) =>
        NormalizeOptionalFilter(trackingId) is { } normalized ? [normalized] : null;

    public sealed record RecommendationMetadataListResult(
        List<Models.RecommendationMetadata> Metadata,
        bool AreResultsTruncated);
}
