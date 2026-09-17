// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Constants;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service for platform landing zone operations.
/// </summary>
public sealed class PlatformLandingZoneService(IAzureService azureService, AzureHttpHelper httpHelper, ILogger<PlatformLandingZoneService> logger)
    : BaseAzureResourceService(azureService), IPlatformLandingZoneService
{
    private static readonly ConcurrentDictionary<string, PlatformLandingZoneParameters> s_parameterCache = new();

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

        s_parameterCache[key] = parameters;
        return Task.FromResult(parameters);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckExistingAsync(PlatformLandingZoneContext context, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(context, "CheckPlatformLandingZone");

        try
        {
            var response = await httpHelper.GetAsync(url, cancellationToken);

            if (string.IsNullOrEmpty(response))
                return false;

            using var doc = JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("exists", out var existsProperty))
            {
                return existsProperty.GetBoolean();
            }

            return false;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GenerateAsync(PlatformLandingZoneContext context, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(context);
        if (!s_parameterCache.TryGetValue(key, out var parameters))
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

        return TryParseDownloadUrl(response);
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
        if (!s_parameterCache.TryGetValue(key, out var p))
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
        $"{AzureService.CloudConfiguration.ArmEnvironment.Endpoint}subscriptions/{ctx.SubscriptionId}/resourceGroups/{ctx.ResourceGroupName}/providers/Microsoft.Migrate/MigrateProjects/{ctx.MigrateProjectName}/{action}?api-version={PlatformLandingZoneConstants.ApiVersion}";

    private static string GetCacheKey(PlatformLandingZoneContext ctx) =>
        $"{ctx.SubscriptionId}:{ctx.ResourceGroupName}:{ctx.MigrateProjectName}";

    private static void ThrowIfFailed(string response)
    {
        if (response.Contains("creation failed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Platform landing zone creation failed. Details: {response}");
    }

    private static string? TryParseDownloadUrl(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return null;

        if (Uri.TryCreate(response.Trim(), UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            return uri.AbsoluteUri;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(response);
        }
        catch (JsonException) when (response.TrimStart()[0] is not ('{' or '[' or '"'))
        {
            // The API also returns plain-text generation acknowledgements.
            return null;
        }

        using (doc)
        {
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.String)
            {
                var text = root.GetString();
                return Uri.TryCreate(text, UriKind.Absolute, out var downloadUri) &&
                    (downloadUri.Scheme == Uri.UriSchemeHttps || downloadUri.Scheme == Uri.UriSchemeHttp)
                    ? downloadUri.AbsoluteUri
                    : null;
            }

            if (root.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Unexpected platform landing zone response. Expected an object or a download URL.");

            ThrowIfFailed(root);
            if (root.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
                ThrowIfFailed(properties);

            if (root.TryGetProperty("downloadUrl", out var url))
                return ValidateDownloadUrl(url);
            if (properties.ValueKind == JsonValueKind.Object && properties.TryGetProperty("downloadUrl", out var propertiesUrl))
                return ValidateDownloadUrl(propertiesUrl);
        }
        return null;
    }

    private static void ThrowIfFailed(JsonElement response)
    {
        if (response.TryGetProperty("error", out var error) && error.ValueKind is not JsonValueKind.Null)
            throw new InvalidOperationException($"Platform landing zone creation failed. Details: {error}");

        foreach (var property in new[] { "status", "provisioningState" })
        {
            if (response.TryGetProperty(property, out var status) && status.ValueKind == JsonValueKind.String &&
                (string.Equals(status.GetString(), "Failed", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(status.GetString(), "Canceled", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Platform landing zone generation {status.GetString()}. Details: {response}");
        }
    }

    private static string? ValidateDownloadUrl(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null)
            return null;

        if (value.ValueKind == JsonValueKind.String)
        {
            var url = value.GetString();
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
                return uri.AbsoluteUri;
        }

        throw new InvalidOperationException("The platform landing zone response contains an invalid download URL.");
    }
}
