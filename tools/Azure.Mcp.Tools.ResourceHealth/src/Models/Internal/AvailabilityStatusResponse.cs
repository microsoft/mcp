// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResourceHealth.Models.Internal;

/// <summary>
/// Internal model representing the Azure REST API response structure for availability status
/// </summary>
internal class AvailabilityStatusResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public AvailabilityStatusProperties? Properties { get; set; }
}

internal class AvailabilityStatusProperties
{
    [JsonPropertyName("availabilityState")]
    public string? AvailabilityState { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("detailedStatus")]
    public string? DetailedStatus { get; set; }

    [JsonPropertyName("reasonType")]
    public string? ReasonType { get; set; }

    [JsonPropertyName("occuredTime")]
    public DateTimeOffset? OccurredTime { get; set; }

    [JsonPropertyName("reportedTime")]
    public DateTimeOffset? ReportedTime { get; set; }

    [JsonPropertyName("reasonChronicity")]
    public string? ReasonChronicity { get; set; }

    [JsonPropertyName("rootCauseAttributionTime")]
    public string? RootCauseAttributionTime { get; set; }

    [JsonPropertyName("healthEventCategory")]
    public string? HealthEventCategory { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

/// <summary>
/// Internal model representing the Azure REST API list response for availability statuses
/// </summary>
internal class AvailabilityStatusListResponse
{
    [JsonPropertyName("value")]
    public List<AvailabilityStatusResponse>? Value { get; set; }

    [JsonPropertyName("nextLink")]
    public string? NextLink { get; set; }
}
