// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public record ConsumerGroup(
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceName)] string Name,
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceId)] string Id,
    [property: JsonPropertyName(EventHubsModelDefinitions.ResourceGroup)] string ResourceGroup,
    [property: JsonPropertyName(EventHubsModelDefinitions.Namespace)] string Namespace,
    [property: JsonPropertyName(EventHubsModelDefinitions.EventHub)] string EventHub,
    [property: JsonPropertyName(EventHubsModelDefinitions.Location)] string? Location = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.UserMetadata)] string? UserMetadata = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.CreationTime)] DateTimeOffset? CreationTime = null,
    [property: JsonPropertyName(EventHubsModelDefinitions.UpdatedTime)] DateTimeOffset? UpdatedTime = null);
