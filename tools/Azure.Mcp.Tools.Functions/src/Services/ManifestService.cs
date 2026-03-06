// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Service for fetching and caching the Azure Functions template manifest from CDN.
/// </summary>
public sealed class ManifestService(
    IHttpClientFactory httpClientFactory,
    ICacheService cacheService,
    ILogger<ManifestService> logger) : IManifestService
{
    private const string ManifestUrl = "https://cdn-test.functions.azure.com/public/templates/manifest.json";
    private const long MaxManifestSizeBytes = 10_485_760; // 10 MB
    private const string CacheGroup = "functions";
    private const string ManifestCacheKey = "manifest";
    private static readonly TimeSpan s_manifestCacheDuration = TimeSpan.FromHours(12);

    /// <inheritdoc />
    public async Task<TemplateManifest> FetchManifestAsync(CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<TemplateManifest>(CacheGroup, ManifestCacheKey, s_manifestCacheDuration, cancellationToken);
        if (cached?.Templates?.Count > 0)
        {
            return cached;
        }

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-MCP-Server/1.0");
            using var response = await client.GetAsync(new Uri(ManifestUrl), cancellationToken);
            response.EnsureSuccessStatusCode();

            // Use size-limited reading to prevent DoS (protects against missing Content-Length header)
            var json = await GitHubUrlValidator.ReadSizeLimitedStringAsync(response.Content, MaxManifestSizeBytes, cancellationToken);
            var manifest = JsonSerializer.Deserialize(json, FunctionTemplatesManifestJsonContext.Default.TemplateManifest)
                ?? throw new InvalidOperationException("Failed to deserialize the CDN manifest.");

            await cacheService.SetAsync(CacheGroup, ManifestCacheKey, manifest, s_manifestCacheDuration, cancellationToken);
            return manifest;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch CDN manifest from {Url}", ManifestUrl);
            throw new InvalidOperationException(
                $"Could not fetch the Azure Functions templates manifest. Details: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse CDN manifest JSON from {Url}", ManifestUrl);
            throw new InvalidOperationException(
                $"Could not parse the Azure Functions templates manifest. Details: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public TemplateManifestEntry? SelectCandidateTemplate(TemplateManifest manifest, string language)
    {
        return manifest.Templates
            .Where(t => t.Language.Equals(language, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.RepositoryUrl)
                && !string.IsNullOrWhiteSpace(t.FolderPath))
            .OrderBy(t => t.Priority)
            .FirstOrDefault();
    }
}
