// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Options.Metadata;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Metadata;

[CommandMetadata(
    Id = "16c9c57e-8f14-43bd-91da-d1548b6af72e",
    Name = "list",
    Title = "List Advisor Recommendation Metadata",
    Description = "List the global Azure Advisor recommendation metadata catalog (formerly recommendation types) from Azure Resource Graph. " +
                  "Use this catalog even when an environment has no generated recommendations: discover available types for greenfield environments, " +
                  "or filter by supported resource type during brownfield onboarding to identify applicable recommendation types. Returns localized type IDs, " +
                  "names, categories, subcategories, impact, priority, descriptions, benefits, actions, scope, source query, and service-retirement details. " +
                  "Supports optional language, resource type, impact, category, and tenant filters. " +
                  "Results are ordered by impact from High to Medium to Low, then by display name.",
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

            var metadata = await _advisorService.ListRecommendationMetadataAsync(
                language,
                NormalizeOptionalFilter(options.ResourceType),
                impact,
                NormalizeOptionalFilter(options.Category),
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(metadata.Results, metadata.AreResultsTruncated),
                AdvisorJsonContext.Default.RecommendationMetadataListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor recommendation metadata. Language: {Language}, ResourceType: {ResourceType}, Impact: {Impact}, Category: {Category}.",
                options.Language, options.ResourceType, options.Impact, options.Category);
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
    {
        if (string.IsNullOrWhiteSpace(impact))
        {
            return null;
        }

        return AllowedImpacts.FirstOrDefault(
            candidate => candidate.Equals(impact.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeOptionalFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed record RecommendationMetadataListResult(
        List<Models.RecommendationMetadata> Metadata,
        bool AreResultsTruncated);
}
