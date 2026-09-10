// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureMigrate.Constants;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Talks to the Platform Landing Zone ARM resource and the Artifact Store that holds its output.
/// </summary>
/// <remarks>
/// The resource is manipulated as raw JSON rather than through generated models. Its properties tree is
/// a set of discriminated unions whose shapes the service selects, and the service also defaults every
/// block the caller omits. Keeping the payload as <see cref="JsonNode"/> lets the caller send only what
/// it wants to change, echo back the full effective configuration the service computed, and stay free of
/// reflection-based polymorphic serialization.
/// </remarks>
public sealed class PlatformLandingZoneService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IPlatformLandingZoneService
{
    private static readonly JsonSerializerOptions s_indented = new() { WriteIndented = true };

    private readonly AzureHttpHelper _httpHelper = new(azureService);

    /// <inheritdoc/>
    public async Task<PlatformLandingZoneView> CreateOrUpdateAsync(
        PlatformLandingZoneContext context,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetPropertiesAsync(context, cancellationToken);
        var properties = PlatformLandingZoneRequestBuilder.BuildProperties(options, existing);
        var body = new JsonObject { ["properties"] = properties }.ToJsonString();

        var result = await _httpHelper.SendJsonAsync(HttpMethod.Put, BuildResourceUrl(context), body, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(DescribeUpsertFailure(context, result));
        }

        return ParseView(result.Body);
    }

    /// <inheritdoc/>
    public async Task<PlatformLandingZoneView?> GetAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpHelper.SendJsonAsync(HttpMethod.Get, BuildResourceUrl(context), null, cancellationToken);
        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to read Platform Landing Zone '{context.LandingZoneName}' ({(int)result.StatusCode} {result.StatusCode}): {result.Body}");
        }

        return ParseView(result.Body);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlatformLandingZoneView>> ListAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpHelper.SendJsonAsync(HttpMethod.Get, BuildCollectionUrl(context), null, cancellationToken);
        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Migrate project '{context.MigrateProjectName}' was not found in resource group '{context.ResourceGroupName}' under subscription '{context.SubscriptionId}'. " +
                "Verify the project name, resource group and subscription, or use the 'createmigrateproject' action to create the project first.");
        }

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to list Platform Landing Zones under migrate project '{context.MigrateProjectName}' ({(int)result.StatusCode} {result.StatusCode}): {result.Body}");
        }

        var views = new List<PlatformLandingZoneView>();
        if (JsonNode.Parse(result.Body) is JsonObject page && page["value"] is JsonArray items)
        {
            foreach (var item in items)
            {
                if (item is JsonObject resource)
                {
                    views.Add(ParseView(resource));
                }
            }
        }

        return views;
    }

    /// <inheritdoc/>
    public async Task<PlatformLandingZoneView> WaitForTerminalAsync(
        PlatformLandingZoneContext context,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (true)
        {
            var view = await GetAsync(context, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Platform Landing Zone '{context.LandingZoneName}' was not found under migrate project '{context.MigrateProjectName}'.");

            if (!string.Equals(view.Status, PlatformLandingZoneConstants.StatusRunning, StringComparison.OrdinalIgnoreCase))
            {
                return view;
            }

            if (DateTimeOffset.UtcNow + PlatformLandingZoneConstants.WaitPollInterval >= deadline)
            {
                throw new TimeoutException(
                    $"Platform Landing Zone '{context.LandingZoneName}' was still generating after {timeout.TotalMinutes:0} minutes. " +
                    "Generation continues in the background; run the 'get' or 'wait' action again to check on it.");
            }

            await Task.Delay(PlatformLandingZoneConstants.WaitPollInterval, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> DownloadAsync(
        PlatformLandingZoneContext context,
        string outputDirectory,
        bool includeDesignDocument,
        CancellationToken cancellationToken = default)
    {
        var view = await GetAsync(context, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Platform Landing Zone '{context.LandingZoneName}' was not found under migrate project '{context.MigrateProjectName}'.");

        if (string.Equals(view.Status, PlatformLandingZoneConstants.StatusRunning, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Platform Landing Zone '{context.LandingZoneName}' is still generating. Use the 'wait' action, then download.");
        }

        if (string.Equals(view.Status, PlatformLandingZoneConstants.StatusFailed, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Platform Landing Zone '{context.LandingZoneName}' failed to generate, so there is no output to download. " +
                "Use the 'get' action to inspect the effective configuration and correct it, then run 'create' again.");
        }

        var artifactName = ResolveArtifactName(context, view.ArtifactId);
        await EnsureArtifactHasContentAsync(context, artifactName, cancellationToken);

        Directory.CreateDirectory(outputDirectory);

        var downloaded = new List<string>
        {
            await DownloadArtifactFileAsync(
                context,
                artifactName,
                PlatformLandingZoneConstants.OutputZipFileName,
                Path.Combine(outputDirectory, BuildLocalFileName(context, PlatformLandingZoneConstants.OutputZipFileName)),
                cancellationToken)
        };

        if (includeDesignDocument)
        {
            downloaded.Add(await DownloadArtifactFileAsync(
                context,
                artifactName,
                PlatformLandingZoneConstants.DesignDocumentFileName,
                Path.Combine(outputDirectory, BuildLocalFileName(context, PlatformLandingZoneConstants.DesignDocumentFileName)),
                cancellationToken));
        }

        return downloaded;
    }

    private async Task<JsonObject?> GetPropertiesAsync(PlatformLandingZoneContext context, CancellationToken cancellationToken)
    {
        var result = await _httpHelper.SendJsonAsync(HttpMethod.Get, BuildResourceUrl(context), null, cancellationToken);
        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to read the current state of Platform Landing Zone '{context.LandingZoneName}' before updating it " +
                $"({(int)result.StatusCode} {result.StatusCode}): {result.Body}");
        }

        return JsonNode.Parse(result.Body) is JsonObject resource && resource["properties"] is JsonObject properties
            ? properties
            : null;
    }

    private async Task EnsureArtifactHasContentAsync(
        PlatformLandingZoneContext context,
        string artifactName,
        CancellationToken cancellationToken)
    {
        var result = await _httpHelper.SendJsonAsync(
            HttpMethod.Get,
            BuildArtifactUrl(context, artifactName),
            null,
            cancellationToken);

        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"No artifact named '{artifactName}' exists yet under migrate project '{context.MigrateProjectName}'. " +
                "The landing zone reports success but its output has not been published; retry shortly.");
        }

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to read artifact '{artifactName}' ({(int)result.StatusCode} {result.StatusCode}): {result.Body}");
        }

        var latestVersion = ReadInt(result.Body, "latestVersion");
        if (latestVersion is null or < 1)
        {
            throw new InvalidOperationException(
                $"Artifact '{artifactName}' has no committed version yet, so there is nothing to download. Retry shortly.");
        }
    }

    private async Task<string> DownloadArtifactFileAsync(
        PlatformLandingZoneContext context,
        string artifactName,
        string artifactFilePath,
        string localPath,
        CancellationToken cancellationToken)
    {
        var request = new JsonObject
        {
            ["mode"] = "file",
            ["path"] = artifactFilePath
        }.ToJsonString();

        var result = await _httpHelper.SendJsonAsync(
            HttpMethod.Post,
            BuildArtifactUrl(context, artifactName, "/generateDownloadUrl"),
            request,
            cancellationToken);

        if (result.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                $"Access was denied generating a download URL for '{artifactFilePath}'. Downloading requires the " +
                "'Microsoft.Migrate/migrateProjects/artifacts/generateDownloadUrl/action' permission on the migrate " +
                "project, which is separate from read access to the landing zone itself.");
        }

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to generate a download URL for '{artifactFilePath}' ({(int)result.StatusCode} {result.StatusCode}): {result.Body}");
        }

        var sasUrl = ReadSasUrl(result.Body)
            ?? throw new InvalidOperationException(
                $"The download URL response for '{artifactFilePath}' did not contain a SAS URL.");

        // The SAS URL carries its own credential; sending an ARM bearer token alongside it is rejected.
        var bytes = await _httpHelper.DownloadBytesAsync(sasUrl, cancellationToken);
        await File.WriteAllBytesAsync(localPath, bytes, cancellationToken);
        return localPath;
    }

    private static string ResolveArtifactName(PlatformLandingZoneContext context, string? artifactId)
    {
        const string Separator = "/artifacts/";
        if (!string.IsNullOrWhiteSpace(artifactId))
        {
            var index = artifactId.LastIndexOf(Separator, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var name = artifactId[(index + Separator.Length)..].Trim('/');
                if (name.Length > 0)
                {
                    return name;
                }
            }
        }

        return PlatformLandingZoneConstants.ArtifactNamePrefix + context.LandingZoneName;
    }

    private static string BuildLocalFileName(PlatformLandingZoneContext context, string artifactFileName) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{context.MigrateProjectName}-{context.LandingZoneName}-{artifactFileName}");

    private string BuildResourceUrl(PlatformLandingZoneContext context) =>
        $"{BuildMigrateProjectUrl(context)}/platformLandingZones/{Uri.EscapeDataString(context.LandingZoneName)}" +
        $"?api-version={PlatformLandingZoneConstants.PlatformLandingZoneApiVersion}";

    private string BuildCollectionUrl(PlatformLandingZoneContext context) =>
        $"{BuildMigrateProjectUrl(context)}/platformLandingZones" +
        $"?api-version={PlatformLandingZoneConstants.PlatformLandingZoneApiVersion}";

    private string BuildArtifactUrl(PlatformLandingZoneContext context, string artifactName, string action = "") =>
        $"{BuildMigrateProjectUrl(context)}/artifacts/{Uri.EscapeDataString(artifactName)}{action}" +
        $"?api-version={PlatformLandingZoneConstants.ArtifactApiVersion}";

    private string BuildMigrateProjectUrl(PlatformLandingZoneContext context)
    {
        var endpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        return $"{endpoint}/subscriptions/{Uri.EscapeDataString(context.SubscriptionId)}" +
               $"/resourceGroups/{Uri.EscapeDataString(context.ResourceGroupName)}" +
               $"/providers/Microsoft.Migrate/migrateProjects/{Uri.EscapeDataString(context.MigrateProjectName)}";
    }

    private static string DescribeUpsertFailure(PlatformLandingZoneContext context, HttpResult result) =>
        result.StatusCode switch
        {
            // The service rejects overlapping writes rather than queueing them, so a conflict almost always
            // means an earlier generation run for this landing zone has not finished yet.
            HttpStatusCode.Conflict =>
                $"Platform Landing Zone '{context.LandingZoneName}' cannot be updated right now, most likely because a " +
                $"generation run is still in progress. Use the 'wait' action, then retry. Service response: {result.Body}",
            HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized =>
                $"Access was denied creating or updating Platform Landing Zone '{context.LandingZoneName}'. " +
                $"Write access on migrate project '{context.MigrateProjectName}' is required. Service response: {result.Body}",
            HttpStatusCode.NotFound =>
                $"Migrate project '{context.MigrateProjectName}' was not found in resource group '{context.ResourceGroupName}'. " +
                "Create it first with the 'createmigrateproject' action.",
            _ =>
                $"Failed to create or update Platform Landing Zone '{context.LandingZoneName}' " +
                $"({(int)result.StatusCode} {result.StatusCode}): {result.Body}"
        };

    private static PlatformLandingZoneView ParseView(string json) =>
        JsonNode.Parse(json) is JsonObject resource
            ? ParseView(resource)
            : new PlatformLandingZoneView(null, null, null, null, null);

    private static PlatformLandingZoneView ParseView(JsonObject resource)
    {
        var properties = resource["properties"] as JsonObject;

        return new PlatformLandingZoneView(
            Name: resource["name"]?.GetValue<string>(),
            ProvisioningState: ReadString(properties, "provisioningState"),
            Status: ReadString(properties, "status"),
            ArtifactId: ReadString(properties, "artifactId"),
            EffectiveProperties: properties?.ToJsonString(s_indented));
    }

    private static string? ReadString(JsonObject? node, string propertyName) =>
        node?[propertyName] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static int? ReadInt(string json, string propertyName)
    {
        if (JsonNode.Parse(json) is not JsonObject root)
        {
            return null;
        }

        // The Artifact Store places its payload under 'properties', but tolerate a flattened response too.
        var candidate = (root["properties"] as JsonObject)?[propertyName] ?? root[propertyName];
        return candidate is JsonValue value && value.TryGetValue<int>(out var number) ? number : null;
    }

    private static string? ReadSasUrl(string json)
    {
        if (JsonNode.Parse(json) is not JsonObject root)
        {
            return null;
        }

        return ReadString(root, "sasUrl") ?? ReadString(root["properties"] as JsonObject, "sasUrl");
    }
}
