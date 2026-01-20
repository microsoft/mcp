// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service for platform landing zone operations.
/// </summary>
public sealed class PlatformLandingZoneService(
    IHttpClientFactory httpClientFactory, ISubscriptionService subscriptionService, ITenantService tenantService,
    ILogger<PlatformLandingZoneService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IPlatformLandingZoneService
{
    private const string ArmBaseUrl = "https://management.azure.com";
    private const string ApiVersion = "2020-06-01-preview";
    private const int MaxPollingAttempts = 24;
    private const int PollingIntervalSeconds = 5;
    private static readonly ConcurrentDictionary<string, PlatformLandingZoneParameters> s_parameterCache = new();
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PlatformLandingZoneService> _logger = logger;

    /// <inheritdoc/>
    public Task<PlatformLandingZoneParameters> UpdateParametersAsync(
        PlatformLandingZoneContext context,
        string? regionType = null,
        string? fireWallType = null,
        string? networkArchitecture = null,
        string? identitySubscriptionId = null,
        string? managementSubscriptionId = null,
        string? connectivitySubscriptionId = null,
        string? regions = null,
        string? environmentName = null,
        string? versionControlSystem = null,
        string? organizationName = null,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(context);
        var existing = s_parameterCache.GetOrAdd(key, _ => new PlatformLandingZoneParameters
        {
            RegionType = null,
            FireWallType = null,
            NetworkArchitecture = null,
            IdentitySubscriptionId = null,
            ManagementSubscriptionId = null,
            ConnectivitySubscriptionId = null,
            Regions = null,
            EnvironmentName = null,
            VersionControlSystem = null,
            OrganizationName = null,
            CachedAt = DateTime.UtcNow
        });

        if (regionType != null && regionType != "single" && regionType != "multi")
        {
            throw new ArgumentException("regionType must be 'single' or 'multi'", nameof(regionType));
        }

        if (fireWallType != null && fireWallType != "azurefirewall" && fireWallType != "nva" && fireWallType != "none")
        {
            throw new ArgumentException("fireWallType must be 'azurefirewall', 'nva', or 'none'", nameof(fireWallType));
        }

        if (networkArchitecture != null && networkArchitecture != "hubspoke" && networkArchitecture != "vwan")
        {
            throw new ArgumentException("networkArchitecture must be 'hubspoke' or 'vwan'", nameof(networkArchitecture));
        }

        var updated = existing with
        {
            RegionType = regionType ?? existing.RegionType,
            FireWallType = fireWallType ?? existing.FireWallType,
            NetworkArchitecture = networkArchitecture ?? existing.NetworkArchitecture,
            IdentitySubscriptionId = identitySubscriptionId ?? existing.IdentitySubscriptionId,
            ManagementSubscriptionId = managementSubscriptionId ?? existing.ManagementSubscriptionId,
            ConnectivitySubscriptionId = connectivitySubscriptionId ?? existing.ConnectivitySubscriptionId,
            Regions = regions ?? existing.Regions,
            EnvironmentName = environmentName ?? existing.EnvironmentName,
            VersionControlSystem = versionControlSystem ?? existing.VersionControlSystem,
            OrganizationName = organizationName ?? existing.OrganizationName,
            CachedAt = DateTime.UtcNow
        };

        s_parameterCache[key] = updated;
        _logger.LogInformation("Updated parameters for {Key}. Complete: {IsComplete}", key, updated.IsComplete);

        return Task.FromResult(updated);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckExistingLandingZoneAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default)
    {
        string? tenant = null;
        var url = $"{ArmBaseUrl}/subscriptions/{context.SubscriptionId}/resourceGroups/{context.ResourceGroupName}/providers/Microsoft.Migrate/MigrateProjects/{context.MigrateProjectName}?api-version={ApiVersion}";

        _logger.LogInformation("Checking existing landing zone at {Url}", url);

        try
        {
            var response = await MakeHttpGetRequestAsync(url, tenant, cancellationToken);
            _logger.LogInformation("Check response: {Response}", response);

            return !string.IsNullOrEmpty(response);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Migrate project not found");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GenerateLandingZoneAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default)
    {
        string? tenant = null;
        var key = GetCacheKey(context);
        if (!s_parameterCache.TryGetValue(key, out var parameters))
        {
            throw new InvalidOperationException($"No cached parameters found for {key}. Use 'update' action first to set parameters.");
        }

        if (!parameters.IsComplete)
        {
            var missing = GetMissingParameters(context);
            throw new InvalidOperationException($"Cannot generate landing zone. Missing required parameters: {string.Join(", ", missing)}");
        }

        var generateUrl = $"{ArmBaseUrl}/subscriptions/{context.SubscriptionId}/resourceGroups/{context.ResourceGroupName}/providers/Microsoft.Migrate/MigrateProjects/{context.MigrateProjectName}/GeneratePlatformLandingZone?api-version={ApiVersion}";
        
        var regionsArray = parameters.Regions?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        
        var payload = new PlatformLandingZoneGenerationPayload
        {
            RegionType = parameters.RegionType,
            FireWallType = parameters.FireWallType,
            NetworkArchitecture = parameters.NetworkArchitecture,
            IdentitySubscriptionId = parameters.IdentitySubscriptionId,
            ManagementSubscriptionId = parameters.ManagementSubscriptionId,
            ConnectivitySubscriptionId = parameters.ConnectivitySubscriptionId,
            VersionControlSystem = parameters.VersionControlSystem,
            Regions = regionsArray,
            ServiceName = parameters.EnvironmentName,
            OrganizationName = parameters.OrganizationName
        };

        _logger.LogInformation("Starting landing zone generation for {Key}", key);
        _logger.LogInformation("Generation payload: {Payload}", JsonSerializer.Serialize(payload, AzureMigrateJsonContext.Default.PlatformLandingZoneGenerationPayload));

        var responseContent = await MakeHttpPostRequestAsync(generateUrl, payload, AzureMigrateJsonContext.Default, tenant, cancellationToken);
        ThrowIfCreationFailed(responseContent);
        _logger.LogInformation("Landing zone generation initiated. Response: {Response}", responseContent);

        return "Generation initiated successfully. This might take some time, around 1-2 minutes. Use 'download' action to poll for completion and download the landing zone once ready.";
    }

    /// <inheritdoc/>
    public async Task<string> DownloadLandingZoneAsync(
        PlatformLandingZoneContext context,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        string? tenant = null;
        var downloadStatusUrl = $"{ArmBaseUrl}/subscriptions/{context.SubscriptionId}/resourceGroups/{context.ResourceGroupName}/providers/Microsoft.Migrate/MigrateProjects/{context.MigrateProjectName}/DownloadPlatformLandingZone?api-version={ApiVersion}";
        
        _logger.LogInformation("Polling for landing zone completion and download URL");

        var client = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        string? downloadUrl = null;

        for (int attempt = 1; attempt <= MaxPollingAttempts; attempt++)
        {
            _logger.LogInformation("Polling attempt {Attempt}/{Max}", attempt, MaxPollingAttempts);

            var pollResponse = await client.PostAsync(downloadStatusUrl, null, cancellationToken);
            pollResponse.EnsureSuccessStatusCode();
            var statusResponse = await pollResponse.Content.ReadAsStringAsync(cancellationToken);
            ThrowIfCreationFailed(statusResponse);
            _logger.LogInformation("Download status response: {Response}", statusResponse);

            downloadUrl = TryParseDownloadUrl(statusResponse);
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                _logger.LogInformation("Landing zone ready. Download URL: {Url}", downloadUrl);
                break;
            }

            if (attempt < MaxPollingAttempts)
            {
                _logger.LogInformation("Landing zone not ready yet, waiting {Seconds} seconds before next poll", PollingIntervalSeconds);
                await Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), cancellationToken);
            }
        }

        if (string.IsNullOrEmpty(downloadUrl))
        {
            throw new InvalidOperationException($"Landing zone generation timed out after {MaxPollingAttempts * PollingIntervalSeconds} seconds. No download URL available.");
        }

        _logger.LogInformation("Downloading landing zone from {Url} to {Path}", downloadUrl, outputPath);

        var downloadClient = _httpClientFactory.CreateClient();
        var downloadResponse = await downloadClient.GetAsync(downloadUrl, cancellationToken);
        downloadResponse.EnsureSuccessStatusCode();

        var content = await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = Path.Combine(outputPath, $"landing-zone-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");

        await File.WriteAllBytesAsync(fileName, content, cancellationToken);
        _logger.LogInformation("Downloaded landing zone to {FileName}", fileName);

        return fileName;
    }

    /// <inheritdoc/>
    public string GetParameterStatus(PlatformLandingZoneContext context)
    {
        var key = GetCacheKey(context);
        if (!s_parameterCache.TryGetValue(key, out var parameters))
        {
            return "No parameters cached. Use 'update' action to set parameters.";
        }

        var status = new StringBuilder();
        status.AppendLine($"Parameters for {key}:");
        status.AppendLine($"  Cached at: {parameters.CachedAt:u}");
        status.AppendLine($"  Complete: {parameters.IsComplete}");
        status.AppendLine($"  RegionType: {parameters.RegionType ?? "(not set)"}");
        status.AppendLine($"  FireWallType: {parameters.FireWallType ?? "(not set)"}");
        status.AppendLine($"  NetworkArchitecture: {parameters.NetworkArchitecture ?? "(not set)"}");
        status.AppendLine($"  IdentitySubscriptionId: {parameters.IdentitySubscriptionId ?? "(not set)"}");
        status.AppendLine($"  ManagementSubscriptionId: {parameters.ManagementSubscriptionId ?? "(not set)"}");
        status.AppendLine($"  ConnectivitySubscriptionId: {parameters.ConnectivitySubscriptionId ?? "(not set)"}");
        status.AppendLine($"  Regions: {parameters.Regions ?? "(not set)"}");
        status.AppendLine($"  EnvironmentName: {parameters.EnvironmentName ?? "(not set)"}");
        status.AppendLine($"  VersionControlSystem: {parameters.VersionControlSystem ?? "(not set)"}");
        status.AppendLine($"  OrganizationName: {parameters.OrganizationName ?? "(not set)"}");

        if (!parameters.IsComplete)
        {
            var missing = GetMissingParameters(context);
            status.AppendLine($"  Missing: {string.Join(", ", missing)}");
        }

        return status.ToString();
    }

    /// <inheritdoc/>
    public List<string> GetMissingParameters(PlatformLandingZoneContext context)
    {
        var key = GetCacheKey(context);
        if (!s_parameterCache.TryGetValue(key, out var parameters))
        {
            return
            [
                "regionType",
                "fireWallType",
                "networkArchitecture",
                "identitySubscriptionId",
                "managementSubscriptionId",
                "connectivitySubscriptionId",
                "regions",
                "environmentName",
                "versionControlSystem",
                "organizationName"
            ];
        }

        var missing = new List<string>();
        if (string.IsNullOrEmpty(parameters.RegionType)) missing.Add("regionType");
        if (string.IsNullOrEmpty(parameters.FireWallType)) missing.Add("fireWallType");
        if (string.IsNullOrEmpty(parameters.NetworkArchitecture)) missing.Add("networkArchitecture");
        if (string.IsNullOrEmpty(parameters.IdentitySubscriptionId)) missing.Add("identitySubscriptionId");
        if (string.IsNullOrEmpty(parameters.ManagementSubscriptionId)) missing.Add("managementSubscriptionId");
        if (string.IsNullOrEmpty(parameters.ConnectivitySubscriptionId)) missing.Add("connectivitySubscriptionId");
        if (string.IsNullOrEmpty(parameters.Regions)) missing.Add("regions");
        if (string.IsNullOrEmpty(parameters.EnvironmentName)) missing.Add("environmentName");
        if (string.IsNullOrEmpty(parameters.VersionControlSystem)) missing.Add("versionControlSystem");
        if (string.IsNullOrEmpty(parameters.OrganizationName)) missing.Add("organizationName");

        return missing;
    }

    private static void ThrowIfCreationFailed(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return;
        }

        var trimmed = responseContent.Trim().Trim('"');
        if (IsFailureMessage(trimmed))
        {
            throw new InvalidOperationException("PlatformLandingZone creation failed");
        }

        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("message", out var messageElement))
            {
                var message = messageElement.GetString();
                if (IsFailureMessage(message))
                {
                    throw new InvalidOperationException(message ?? "PlatformLandingZone creation failed");
                }
            }
        }
        catch (JsonException)
        {
        }
    }

    private static bool IsFailureMessage(string? message) =>
        !string.IsNullOrEmpty(message) &&
        string.Equals(message, "PlatformLandingZone creation failed", StringComparison.OrdinalIgnoreCase);

    private static string GetCacheKey(PlatformLandingZoneContext context) =>
        $"{context.SubscriptionId}:{context.ResourceGroupName}:{context.MigrateProjectName}";

    private async Task<HttpClient> GetAuthenticatedHttpClientAsync(string? tenant, CancellationToken cancellationToken)
    {
        var httpClient = TenantService.GetClient();
        var credential = await TenantService.GetTokenCredentialAsync(tenant, cancellationToken);
        var token = await credential.GetTokenAsync(
            new TokenRequestContext(["https://management.azure.com/.default"]), cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return httpClient;
    }

    private static string? TryParseDownloadUrl(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            
            if (doc.RootElement.TryGetProperty("downloadUrl", out var downloadUrlElement))
            {
                return downloadUrlElement.GetString();
            }
            
            if (doc.RootElement.TryGetProperty("properties", out var properties) &&
                properties.TryGetProperty("downloadUrl", out var propsDownloadUrl))
            {
                return propsDownloadUrl.GetString();
            }
        }
        catch (JsonException)
        {
            var trimmed = responseContent.Trim().Trim('"');
            if (!string.IsNullOrEmpty(trimmed) && (trimmed.StartsWith("http://") || trimmed.StartsWith("https://")))
            {
                return trimmed;
            }
        }
        
        return null;
    }

    private async Task<string> MakeHttpGetRequestAsync(string url, string? tenant, CancellationToken cancellationToken)
    {
        var httpClient = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<string> MakeHttpPostRequestAsync<TPayload>(string url, TPayload payload, JsonSerializerContext context, string? tenant, CancellationToken cancellationToken)
    {
        var httpClient = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        var json = JsonSerializer.Serialize(payload, typeof(TPayload), context);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    } 
}