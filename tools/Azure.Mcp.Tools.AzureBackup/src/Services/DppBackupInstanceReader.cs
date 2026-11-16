// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.DataProtectionBackup;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Reads DPP backup instances over the ArmClient pipeline and repairs payload shapes that
/// Azure.ResourceManager.DataProtectionBackup cannot deserialize before handing them to the SDK model.
/// </summary>
/// <remarks>
/// The service can return <c>dataSourceInfo.resourceProperties</c> or <c>dataSourceSetInfo.resourceProperties</c>
/// with a null <c>objectType</c>; the SDK then throws <see cref="ArgumentNullException"/> while deserializing the
/// instance (and, when listing, the whole page that contains it). Those optional, type-specific properties are dropped
/// so the rest of the instance can be returned, and each listed instance is deserialized independently.
/// </remarks>
internal static class DppBackupInstanceReader
{
    // Matches the api-version used by Azure.ResourceManager.DataProtectionBackup for backup instances.
    internal const string ApiVersion = "2026-03-01";

    private static readonly ModelReaderWriterOptions s_wireOptions = new("J");

    internal static async Task<DataProtectionBackupInstanceData> GetAsync(
        ArmClient armClient, ResourceIdentifier instanceId, CancellationToken cancellationToken)
    {
        var resource = new ArmPipelineResource(armClient, instanceId);
        var uri = CreateBackupInstancesUri(resource.BaseUri, instanceId.SubscriptionId!, instanceId.ResourceGroupName!, instanceId.Parent!.Name);
        uri.AppendPath("/", escape: false);
        uri.AppendPath(instanceId.Name, escape: true);
        uri.AppendQuery("api-version", ApiVersion, escapeValue: true);

        var instance = await SendAsync(resource, uri.ToUri(), cancellationToken);
        return Deserialize(instance);
    }

    /// <summary>
    /// Lists every backup instance in the vault, following nextLink pages through the validated ARM pipeline.
    /// An instance that still fails to deserialize after repair is skipped instead of failing the listing.
    /// </summary>
    internal static async Task<List<TResult>> ListAsync<TResult>(
        ArmClient armClient,
        ResourceIdentifier vaultId,
        Func<DataProtectionBackupInstanceData, TResult> map,
        CancellationToken cancellationToken)
    {
        var resource = new ArmPipelineResource(armClient, vaultId);
        var uri = CreateBackupInstancesUri(resource.BaseUri, vaultId.SubscriptionId!, vaultId.ResourceGroupName!, vaultId.Name);
        uri.AppendQuery("api-version", ApiVersion, escapeValue: true);

        var results = new List<TResult>();
        Uri? pageUri = uri.ToUri();
        while (pageUri is not null)
        {
            var page = await SendAsync(resource, pageUri, cancellationToken);
            if (page?["value"] is JsonArray items)
            {
                foreach (var item in items)
                {
                    try
                    {
                        results.Add(map(Deserialize(item)));
                    }
                    catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException or JsonException)
                    {
                        // Skip only the instance that cannot be represented; the rest of the page is still returned.
                    }
                }
            }

            // ARM endpoint validation for nextLink happens in the ArmClient pipeline before each request is sent.
            var nextLink = page?["nextLink"] is JsonValue next && next.TryGetValue<string>(out var link) ? link : null;
            Uri? nextUri = string.IsNullOrEmpty(nextLink) ? null : new Uri(nextLink, UriKind.Absolute);
            pageUri = nextUri is not null && nextUri != pageUri ? nextUri : null;
        }

        return results;
    }

    /// <summary>
    /// Drops type-specific datasource <c>resourceProperties</c> without a usable <c>objectType</c>, then deserializes
    /// the instance with the SDK model.
    /// </summary>
    internal static DataProtectionBackupInstanceData Deserialize(JsonNode? instance)
    {
        if (instance is not JsonObject instanceObject)
        {
            throw new FormatException("The backup instance response is not a JSON object.");
        }

        if (instanceObject["properties"] is JsonObject properties)
        {
            RemoveUntypedResourceProperties(properties["dataSourceInfo"]);
            RemoveUntypedResourceProperties(properties["dataSourceSetInfo"]);
        }

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            instanceObject.WriteTo(writer);
        }

        var model = (IPersistableModel<DataProtectionBackupInstanceData>)new DataProtectionBackupInstanceData();
        return model.Create(new BinaryData(buffer.WrittenMemory), s_wireOptions)
            ?? throw new FormatException("The backup instance response could not be deserialized.");
    }

    private static void RemoveUntypedResourceProperties(JsonNode? datasourceInfo)
    {
        if (datasourceInfo is JsonObject info &&
            info["resourceProperties"] is JsonNode resourceProperties &&
            !(resourceProperties["objectType"] is JsonValue objectType &&
              objectType.TryGetValue<string>(out var value) &&
              !string.IsNullOrEmpty(value)))
        {
            info.Remove("resourceProperties");
        }
    }

    private static RequestUriBuilder CreateBackupInstancesUri(Uri endpoint, string subscription, string resourceGroup, string vaultName)
    {
        var uri = new RequestUriBuilder();
        uri.Reset(endpoint);
        uri.AppendPath("/subscriptions/", escape: false);
        uri.AppendPath(subscription, escape: true);
        uri.AppendPath("/resourceGroups/", escape: false);
        uri.AppendPath(resourceGroup, escape: true);
        uri.AppendPath("/providers/Microsoft.DataProtection/backupVaults/", escape: false);
        uri.AppendPath(vaultName, escape: true);
        uri.AppendPath("/backupInstances", escape: false);
        return uri;
    }

    private static async Task<JsonNode?> SendAsync(ArmPipelineResource resource, Uri uri, CancellationToken cancellationToken)
    {
        using var message = resource.HttpPipeline.CreateMessage();
        message.Request.Method = RequestMethod.Get;
        message.Request.Uri.Reset(uri);
        message.Request.Headers.Add("Accept", "application/json");

        await resource.HttpPipeline.SendAsync(message, cancellationToken);
        if (message.Response.Status != 200)
        {
            throw new RequestFailedException(message.Response);
        }

        return JsonNode.Parse(message.Response.Content.ToMemory().Span);
    }
}
