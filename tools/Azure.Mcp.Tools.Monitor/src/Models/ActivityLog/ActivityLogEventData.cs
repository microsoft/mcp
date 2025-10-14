// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.ActivityLog;

/// <summary>
/// Activity log event data model.
/// https://learn.microsoft.com/en-us/rest/api/monitor/activity-logs/list#eventdata
/// </summary>
public sealed class ActivityLogEventData
{
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("resourceId")]
    public required string ResourceId { get; set; }

    [JsonPropertyName("operationName")]
    public required ActivityLogLocalizableString OperationName { get; set; }

    [JsonPropertyName("level")]
    public required ActivityLogEventLevel Level { get; set; }

    [JsonPropertyName("eventTimestamp")]
    public required string EventTimestamp { get; set; }

    [JsonPropertyName("properties")]
    public required Dictionary<string, object> Properties { get; set; }

    [JsonPropertyName("caller")]
    public string? Caller { get; set; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    [JsonPropertyName("category")]
    public ActivityLogLocalizableString? Category { get; set; }

    [JsonPropertyName("operationId")]
    public string? OperationId { get; set; }

    [JsonPropertyName("resourceGroupName")]
    public string? ResourceGroupName { get; set; }

    [JsonPropertyName("resourceProviderName")]
    public ActivityLogLocalizableString? ResourceProviderName { get; set; }

    [JsonPropertyName("status")]
    public ActivityLogLocalizableString? Status { get; set; }

    [JsonPropertyName("subStatus")]
    public ActivityLogLocalizableString? SubStatus { get; set; }

    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }
}
