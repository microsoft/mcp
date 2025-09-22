// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public record EventHubsNamespaceDetails(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("resourceGroup")] string ResourceGroup,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("sku")] EventHubsNamespaceSku? Sku,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("creationTime")] DateTimeOffset? CreationTime,
    [property: JsonPropertyName("updatedTime")] DateTimeOffset? UpdatedTime,
    [property: JsonPropertyName("serviceBusEndpoint")] string? ServiceBusEndpoint,
    [property: JsonPropertyName("metricId")] string? MetricId,
    [property: JsonPropertyName("isAutoInflateEnabled")] bool? IsAutoInflateEnabled,
    [property: JsonPropertyName("maximumThroughputUnits")] int? MaximumThroughputUnits,
    [property: JsonPropertyName("kafkaEnabled")] bool? KafkaEnabled,
    [property: JsonPropertyName("zoneRedundant")] bool? ZoneRedundant,
    [property: JsonPropertyName("tags")] Dictionary<string, string>? Tags);

public record EventHubsNamespaceSku(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("capacity")] int? Capacity);
