// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Constants;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service for platform landing zone operations.
/// </summary>
public sealed class PlatformLandingZoneService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    AzureHttpHelper httpHelper,
    ILogger<PlatformLandingZoneService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IPlatformLandingZoneService
{
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private static readonly ConcurrentDictionary<string, PlatformLandingZoneParameters> ParameterCache = new();

    /// <inheritdoc/>
    public Task<PlatformLandingZoneParameters> UpdateParametersAsync(
        PlatformLandingZoneContext context,
        string? regionType,
        string? fireWallType,
        string? networkArchitecture,
        string? identitySubscriptionId,
        string? managementSubscriptionId,
        string? connectivitySubscriptionId,
        string? regions,
        string? environmentName,
        string? versionControlSystem,
        string? organizationName,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(context);

        var parameters = new PlatformLandingZoneParameters
        {
            RegionType = regionType ?? "single",
            FireWallType = fireWallType ?? "azurefirewall",
            NetworkArchitecture = networkArchitecture ?? "hubspoke",
            IdentitySubscriptionId = identitySubscriptionId ?? context.SubscriptionId,
            ManagementSubscriptionId = managementSubscriptionId ?? context.SubscriptionId,
            ConnectivitySubscriptionId = connectivitySubscriptionId ?? context.SubscriptionId,
            Regions = regions ?? "eastus",
            EnvironmentName = environmentName ?? "prod",
            VersionControlSystem = versionControlSystem ?? "local",
            OrganizationName = organizationName ?? "contoso",
            CachedAt = DateTime.UtcNow
        };

        ParameterCache[key] = parameters;
        return Task.FromResult(parameters);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckExistingAsync(PlatformLandingZoneContext context, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(context, "CheckPlatformLandingZone");

        try
        {
            var response = await httpHelper.GetAsync(url, cancellationToken);
            return !string.IsNullOrEmpty(response);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GenerateAsync(PlatformLandingZoneContext context, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(context);
        if (!ParameterCache.TryGetValue(key, out var parameters))
            throw new InvalidOperationException("No parameters cached. Use 'update' action first.");

        var url = BuildUrl(context, "GeneratePlatformLandingZone");

        var payload = new PlatformLandingZoneGenerationPayload
        {
            RegionType = parameters.RegionType,
            FireWallType = parameters.FireWallType,
            NetworkArchitecture = parameters.NetworkArchitecture,
            IdentitySubscriptionId = parameters.IdentitySubscriptionId,
            ManagementSubscriptionId = parameters.ManagementSubscriptionId,
            ConnectivitySubscriptionId = parameters.ConnectivitySubscriptionId,
            VersionControlSystem = parameters.VersionControlSystem,
            Regions = parameters.Regions?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? ["eastus"],
            ServiceName = parameters.EnvironmentName,
            OrganizationName = parameters.OrganizationName
        };

        logger.LogInformation("Generating landing zone: {Payload}",
            JsonSerializer.Serialize(payload, AzureMigrateJsonContext.Default.PlatformLandingZoneGenerationPayload));

        var response = await httpHelper.PostAsync(url, payload, AzureMigrateJsonContext.Default, cancellationToken);
        ThrowIfFailed(response);

        return "Generation initiated. Use 'download' action in 1-2 minutes to retrieve files.";
    }

    /// <inheritdoc/>
    public async Task<string> DownloadAsync(PlatformLandingZoneContext context, string outputPath, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(context, "DownloadPlatformLandingZone");
        var response = await httpHelper.PostAsync(url, cancellationToken);
        ThrowIfFailed(response);

        var downloadUrl = TryParseDownloadUrl(response);
        if (string.IsNullOrEmpty(downloadUrl))
            throw new InvalidOperationException("Download URL not yet available. The landing zone may still be generating. Please try again in 1-2 minutes.");

        var bytes = await httpHelper.DownloadBytesAsync(downloadUrl, cancellationToken);
        var fileName = Path.Combine(outputPath, $"landing-zone-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        await File.WriteAllBytesAsync(fileName, bytes, cancellationToken);

        return fileName;
    }

    /// <inheritdoc/>
    public string GetParameterStatus(PlatformLandingZoneContext context)
    {
        var key = GetCacheKey(context);
        if (!ParameterCache.TryGetValue(key, out var p))
            return "No parameters cached. Use 'update' action to set parameters.";

        return $"""
            Cached parameters (updated {p.CachedAt:u}):
              regionType: {p.RegionType}
              firewallType: {p.FireWallType}
              networkArchitecture: {p.NetworkArchitecture}
              regions: {p.Regions}
              environmentName: {p.EnvironmentName}
              organizationName: {p.OrganizationName}
              versionControlSystem: {p.VersionControlSystem}
              identitySubscriptionId: {p.IdentitySubscriptionId}
              managementSubscriptionId: {p.ManagementSubscriptionId}
              connectivitySubscriptionId: {p.ConnectivitySubscriptionId}
            Ready to generate: Yes
            """;
    }

    /// <inheritdoc/>
    public List<string> GetMissingParameters(PlatformLandingZoneContext context) => [];

    private string BuildUrl(PlatformLandingZoneContext ctx, string action) =>
        $"{_tenantService.CloudConfiguration.ArmEnvironment.Endpoint}subscriptions/{ctx.SubscriptionId}/resourceGroups/{ctx.ResourceGroupName}/providers/Microsoft.Migrate/MigrateProjects/{ctx.MigrateProjectName}/{action}?api-version={PlatformLandingZoneConstants.ApiVersion}";

    private static string GetCacheKey(PlatformLandingZoneContext ctx) =>
        $"{ctx.SubscriptionId}:{ctx.ResourceGroupName}:{ctx.MigrateProjectName}";

    private static void ThrowIfFailed(string response)
    {
        if (response.Contains("creation failed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Platform landing zone creation failed.");
    }

    private static string? TryParseDownloadUrl(string response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("downloadUrl", out var url))
                return url.GetString();
            if (doc.RootElement.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("downloadUrl", out var propsUrl))
                return propsUrl.GetString();
        }
        catch (JsonException)
        {
            var trimmed = response.Trim().Trim('"');
            if (trimmed.StartsWith("http://") || trimmed.StartsWith("https://"))
                return trimmed;
        }
        return null;
    }
}
