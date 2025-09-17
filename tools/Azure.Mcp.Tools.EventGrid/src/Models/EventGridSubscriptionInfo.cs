// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventGrid.Models;

public record EventGridSubscriptionInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("endpointType")] string? EndpointType,
    [property: JsonPropertyName("endpointUrl")] string? EndpointUrl,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("deadLetterDestination")] string? DeadLetterDestination,
    [property: JsonPropertyName("filter")] string? Filter,
    [property: JsonPropertyName("maxDeliveryAttempts")] int? MaxDeliveryAttempts,
    [property: JsonPropertyName("eventTimeToLiveInMinutes")] int? EventTimeToLiveInMinutes,
    [property: JsonPropertyName("createdDateTime")] string? CreatedDateTime,
    [property: JsonPropertyName("updatedDateTime")] string? UpdatedDateTime
);
