// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public record Namespace(
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceName)] string Name,
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceId)] string Id,
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceGroup)] string ResourceGroup,
    [property: JsonPropertyName(EventHubsModelDefinitions.Location)] string? Location = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.Sku)] EventHubsNamespaceSku? Sku = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.Status)] string? Status = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.ProvisioningState)] string? ProvisioningState = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.CreationTime)] DateTimeOffset? CreationTime = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.UpdatedTime)] DateTimeOffset? UpdatedTime = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.ServiceBusEndpoint)] string? ServiceBusEndpoint = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.MetricId)] string? MetricId = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.IsAutoInflateEnabled)] bool? IsAutoInflateEnabled = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.MaximumThroughputUnits)] int? MaximumThroughputUnits = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.KafkaEnabled)] bool? KafkaEnabled = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.ZoneRedundant)] bool? ZoneRedundant = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.Tags)] Dictionary<string, string>? Tags = null);

public record EventHubsNamespaceSku(
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceName)] string? Name,
    [property: JsonPropertyName(EventHubsModelDefinitions.Tier)] string? Tier,
    [property: JsonPropertyName(EventHubsModelDefinitions.Capacity)] int? Capacity);
