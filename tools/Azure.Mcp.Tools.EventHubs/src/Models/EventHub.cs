// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public record EventHub(
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceName)] string Name,
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceId)] string Id,
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceGroup)] string ResourceGroup,
    [property: JsonPropertyName(EventHubsModelDefinitions.Location)] string? Location,
    [property: JsonPropertyName(EventHubsModelDefinitions.PartitionCount)] int? PartitionCount,
    [property: JsonPropertyName(EventHubsModelDefinitions.MessageRetentionInDays)] int? MessageRetentionInDays,
    [property: JsonPropertyName(EventHubsModelDefinitions.Status)] string? Status,
    [property: JsonPropertyName(EventHubsModelDefinitions.CreatedOn)] DateTimeOffset? CreatedOn,
    [property: JsonPropertyName(EventHubsModelDefinitions.UpdatedOn)] DateTimeOffset? UpdatedOn,
    [property: JsonPropertyName(EventHubsModelDefinitions.PartitionIds)] IReadOnlyList<string>? PartitionIds);
