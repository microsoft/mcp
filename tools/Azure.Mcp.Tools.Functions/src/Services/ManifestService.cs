// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Service for fetching and caching the Azure Functions template manifest from CDN.
/// </summary>
public sealed class ManifestService(
    IHttpClientFactory httpClientFactory,
    ICacheService cacheService,
    IOptions<FunctionsOptions> options,
    ILogger<ManifestService> logger) : IManifestService
{
    private const long MaxManifestSizeBytes = 10_485_760; // 10 MB
    private const string CacheGroup = "functions";
    private const string ManifestCacheKey = "manifest";

    private readonly string _manifestUrl = options.Value.ManifestUrl;
    private readonly string _fallbackManifestUrl = options.Value.FallbackManifestUrl;

    /// <summary>
    /// Result of a manifest fetch operation.
    /// </summary>
    private sealed record ManifestFetchResult
    {
        public TemplateManifest? Manifest { get; init; }
        public string? Error { get; init; }
        public bool IsSuccess => Manifest is not null;

        public static ManifestFetchResult Success(TemplateManifest manifest) => new() { Manifest = manifest };
        public static ManifestFetchResult Failure(string error) => new() { Error = error };
    }

    /// <inheritdoc />
    public async Task<TemplateManifest> FetchManifestAsync(CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<TemplateManifest>(CacheGroup, ManifestCacheKey, FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);
        if (cached?.Templates?.Count > 0)
        {
            return cached;
        }

        // Try primary URL first, then fallback
        var primaryResult = await TryFetchManifestAsync(_manifestUrl, cancellationToken);
        if (primaryResult.IsSuccess)
        {
            await cacheService.SetAsync(CacheGroup, ManifestCacheKey, primaryResult.Manifest!, FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);
            return primaryResult.Manifest!;
        }

        logger.LogWarning("Primary manifest URL failed, trying fallback. Error: {Error}", primaryResult.Error);

        var fallbackResult = await TryFetchManifestAsync(_fallbackManifestUrl, cancellationToken);
        if (fallbackResult.IsSuccess)
        {
            await cacheService.SetAsync(CacheGroup, ManifestCacheKey, fallbackResult.Manifest!, FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);
            return fallbackResult.Manifest!;
        }

        throw new InvalidOperationException(
            $"Could not fetch the Azure Functions templates manifest from primary or fallback URLs. " +
            $"Primary error: {primaryResult.Error}. Fallback error: {fallbackResult.Error}");
    }

    private async Task<ManifestFetchResult> TryFetchManifestAsync(string url, CancellationToken cancellationToken)
    {
        var uri = new Uri(url);

        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await GitHubUrlValidator.ReadSizeLimitedStringAsync(response.Content, MaxManifestSizeBytes, cancellationToken);
            var manifest = JsonSerializer.Deserialize(json, FunctionsJsonContext.Default.TemplateManifest);

            if (manifest is null)
            {
                return ManifestFetchResult.Failure("Failed to deserialize manifest JSON");
            }

            return ManifestFetchResult.Success(manifest);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch manifest from {Url}", url);
            return ManifestFetchResult.Failure(ex.Message);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse manifest JSON from {Url}", url);
            return ManifestFetchResult.Failure(ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Request timed out fetching manifest from {Url}", url);
            return ManifestFetchResult.Failure("Request timed out");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Manifest response from {Url} exceeded size limit", url);
            return ManifestFetchResult.Failure(ex.Message);
        }
    }
}
