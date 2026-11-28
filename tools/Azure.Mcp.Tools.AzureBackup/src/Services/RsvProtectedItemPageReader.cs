// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager.RecoveryServicesBackup;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Reads RSV backupProtectedItems list pages one item at a time (NEW-8).
/// </summary>
/// <remarks>
/// The SDK pageable deserializes a whole page at once, so a single malformed item fails the
/// entire listing. Classic-VM items can carry empty resource-ID strings (for example
/// <c>"sourceResourceId": ""</c>), which the SDK passes to <see cref="ResourceIdentifier"/> and
/// which then throws <see cref="ArgumentException"/>. This reader deserializes each item on its own,
/// retries with empty strings removed, and finally falls back to a minimal item built from the raw
/// JSON so that no protected item is silently dropped.
/// </remarks>
internal static class RsvProtectedItemPageReader
{
    private static readonly ModelReaderWriterOptions s_wireOptions = new("J");

    // IJsonModel<T>.Create does not depend on instance state; the instance only provides the
    // AOT-safe, reflection-free entry point to the SDK's generated deserializer.
    private static readonly IJsonModel<BackupProtectedItemData> s_model = new BackupProtectedItemData(AzureLocation.EastUS);

    /// <summary>
    /// Reads a list page.
    /// </summary>
    /// <param name="page">The page JSON (an object with <c>value</c> and optional <c>nextLink</c>).</param>
    /// <param name="map">Maps a deserialized SDK item to the tool model.</param>
    internal static RsvProtectedItemPage ReadPage(JsonElement page, Func<BackupProtectedItemData, ProtectedItemInfo> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var items = new List<ProtectedItemInfo>();
        if (page.ValueKind != JsonValueKind.Object)
        {
            return new(items, null);
        }

        if (page.TryGetProperty("value", out var values) && values.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in values.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    items.Add(ReadItem(element, map));
                }
            }
        }

        var nextLink = GetString(page, "nextLink");
        return new(items, string.IsNullOrWhiteSpace(nextLink) ? null : nextLink);
    }

    /// <summary>
    /// Reads one protected item, tolerating SDK deserialization failures.
    /// </summary>
    internal static ProtectedItemInfo ReadItem(JsonElement element, Func<BackupProtectedItemData, ProtectedItemInfo> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var raw = element.GetRawText();
        if (TryDeserialize(raw, out var data))
        {
            return map(data);
        }

        if (JsonNode.Parse(raw) is JsonObject node)
        {
            RemoveEmptyStrings(node);
            if (TryDeserialize(node.ToJsonString(), out data))
            {
                return map(data);
            }
        }

        return CreateFallback(element);
    }

    private static bool TryDeserialize(string json, [NotNullWhen(true)] out BackupProtectedItemData? data)
    {
        try
        {
            data = s_model.Create(BinaryData.FromString(json), s_wireOptions);
            return data is not null;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException or JsonException or OverflowException)
        {
            data = null;
            return false;
        }
    }

    /// <summary>
    /// Removes string properties whose value is empty. The service uses empty strings for absent
    /// values, but the SDK treats any present string as a value to parse.
    /// </summary>
    internal static void RemoveEmptyStrings(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                List<string>? emptyKeys = null;
                foreach (var (key, value) in obj)
                {
                    if (value is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String &&
                        string.IsNullOrEmpty(jsonValue.GetValue<string>()))
                    {
                        (emptyKeys ??= []).Add(key);
                    }
                    else
                    {
                        RemoveEmptyStrings(value);
                    }
                }

                if (emptyKeys is not null)
                {
                    foreach (var key in emptyKeys)
                    {
                        obj.Remove(key);
                    }
                }

                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    RemoveEmptyStrings(item);
                }

                break;
        }
    }

    /// <summary>
    /// Builds a minimal protected item from raw JSON when the SDK cannot deserialize it at all.
    /// </summary>
    internal static ProtectedItemInfo CreateFallback(JsonElement element)
    {
        var properties = element.TryGetProperty("properties", out var p) && p.ValueKind == JsonValueKind.Object ? p : default;
        var policyId = NullIfEmpty(GetString(properties, "policyId"));
        DateTimeOffset? lastBackupTime = properties.ValueKind == JsonValueKind.Object &&
            properties.TryGetProperty("lastBackupTime", out var last) &&
            last.ValueKind == JsonValueKind.String &&
            last.TryGetDateTimeOffset(out var parsed)
                ? parsed
                : null;

        return new ProtectedItemInfo(
            Id: NullIfEmpty(GetString(element, "id")),
            Name: GetString(element, "name") ?? string.Empty,
            VaultType: VaultTypeResolver.Rsv,
            ProtectionStatus: NullIfEmpty(GetString(properties, "protectionStatus")) ?? NullIfEmpty(GetString(properties, "protectionState")),
            DatasourceType: NullIfEmpty(GetString(properties, "workloadType")),
            DatasourceId: NullIfEmpty(GetString(properties, "sourceResourceId")),
            PolicyName: policyId?.TrimEnd('/').Split('/')[^1],
            LastBackupTime: lastBackupTime,
            ContainerName: NullIfEmpty(GetString(properties, "containerName")));
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
