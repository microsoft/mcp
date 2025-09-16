// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResourceHealth.Models.Internal;

/// <summary>
/// Internal model representing the Azure REST API response structure for service health events
/// </summary>
internal class ServiceHealthEventResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("properties")]
    public ServiceHealthEventProperties? Properties { get; set; }
}

internal class ServiceHealthEventProperties
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("eventLevel")]
    public string? EventLevel { get; set; }

    [JsonPropertyName("trackingId")]
    public string? TrackingId { get; set; }

    [JsonPropertyName("impactStartTime")]
    public DateTimeOffset? ImpactStartTime { get; set; }

    [JsonPropertyName("impactMitigationTime")]
    public DateTimeOffset? ImpactMitigationTime { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTimeOffset? LastUpdateTime { get; set; }

    [JsonPropertyName("communicationId")]
    public string? CommunicationId { get; set; }

    [JsonPropertyName("impact")]
    public List<ServiceHealthEventImpact>? Impact { get; set; }
}

internal class ServiceHealthEventImpact
{
    [JsonPropertyName("impactedService")]
    public string? ImpactedService { get; set; }

    [JsonPropertyName("impactedRegions")]
    public List<ServiceHealthEventRegion>? ImpactedRegions { get; set; }
}

internal class ServiceHealthEventRegion
{
    [JsonPropertyName("impactedRegion")]
    public string? ImpactedRegion { get; set; }
}

/// <summary>
/// Internal model representing the Azure REST API list response for service health events
/// </summary>
internal class ServiceHealthEventListResponse
{
    [JsonPropertyName("value")]
    public List<ServiceHealthEventResponse>? Value { get; set; }

    [JsonPropertyName("nextLink")]
    public string? NextLink { get; set; }
}
