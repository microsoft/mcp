// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Net;
using Azure.Mcp.Tools.Advisor.Options.Metadata;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Metadata;

[CommandMetadata(
    Id = "3c9f1e6a-8b2d-4e77-9c14-5a6d0f2b7e83",
    Name = "get",
    Title = "Get Advisor Recommendation Metadata",
    Description = "Get the Azure Advisor metadata catalog entry and details for a specific recommendation type by its " +
        "recommendation type id. Explains what an Advisor recommendation type means, including its display name, " +
        "category, sub-category, impact, supported resource type, description, potential benefits, and remediation actions.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MetadataGetCommand(ILogger<MetadataGetCommand> logger, IAdvisorService advisorService)
    : AuthenticatedCommand<MetadataGetOptions, MetadataGetCommand.MetadataGetResult>()
{
    private readonly IAdvisorService _advisorService = advisorService;
    private readonly ILogger<MetadataGetCommand> _logger = logger;

    // The set of locales present in the Advisor recommendation-metadata catalog (properties.language).
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "cs", "de", "es", "fr", "hu", "id", "it", "ja", "ko",
        "nl", "pl", "pt-br", "pt-pt", "ru", "sv", "tr", "zh-hans", "zh-hant",
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MetadataGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!TryNormalizeLanguage(options.Language, out var language))
            {
                throw new ArgumentException(
                    $"Unsupported language '{options.Language}'. Supported values: {string.Join(", ", SupportedLanguages.OrderBy(l => l, StringComparer.Ordinal))}. " +
                    "Region-qualified tags such as 'en-US' are accepted and mapped to the base language.");
            }

            var metadata = await _advisorService.GetRecommendationMetadataAsync(
                options.RecommendationTypeId,
                language,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(metadata is null ? [] : [metadata]),
                AdvisorJsonContext.Default.MetadataGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Advisor recommendation metadata. RecommendationTypeId: {RecommendationTypeId}, Language: {Language}.",
                options.RecommendationTypeId, options.Language);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendation metadata not found. Verify the recommendation type id and language.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Advisor metadata. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    // Normalizes a user-supplied locale to a value present in the catalog. Matches are case-insensitive.
    // Region-qualified tags (e.g. 'en-US') fall back to their primary subtag ('en') when that base language
    // is supported; full script/region tags such as 'pt-br' and 'zh-hans' are matched exactly.
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

    public sealed record MetadataGetResult(List<Models.RecommendationMetadata> Metadata);
}
