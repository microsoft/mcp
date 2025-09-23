// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Kusto.Models;

public record KustoClusterModel(
    [property: JsonPropertyName("name")] string ClusterName,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroupName,
    [property: JsonPropertyName("subscription")] string? SubscriptionId,
    [property: JsonPropertyName("sku")] string? Sku,
    [property: JsonPropertyName("zones")] string? Zones,
    [property: JsonPropertyName("identity")] string? Identity,
    [property: JsonPropertyName("etag")] string? ETag,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("clusterUri")] string? ClusterUri,
    [property: JsonPropertyName("dataIngestionUri")] string? DataIngestionUri,
    [property: JsonPropertyName("stateReason")] string? StateReason,
    [property: JsonPropertyName("isStreamingIngestEnabled")] bool? IsStreamingIngestEnabled,
    [property: JsonPropertyName("engineType")] string? EngineType,
    [property: JsonPropertyName("isAutoStopEnabled")] bool? IsAutoStopEnabled
);
