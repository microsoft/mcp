// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

/// <summary>
/// A class representing the EventHubs Namespace properties model.
/// </summary>
internal sealed class EventHubsNamespaceProperties
{
    /// <summary> Provisioning state of the Namespace. </summary>
    public string? ProvisioningState { get; set; }
    /// <summary> Status of the Namespace. </summary>
    public string? Status { get; set; }
    /// <summary> The time the Namespace was created. </summary>
    [JsonPropertyName(EventHubsModelDefinitions.CreatedAt)]
    public DateTimeOffset? CreatedOn { get; set; }
    /// <summary> The time the Namespace was updated. </summary>
    [JsonPropertyName(EventHubsModelDefinitions.UpdatedAt)]
    public DateTimeOffset? UpdatedOn { get; set; }
    /// <summary> Endpoint you can use to perform Service Bus operations. </summary>
    public string? ServiceBusEndpoint { get; set; }
    /// <summary> Identifier for Azure Insights metrics. </summary>
    public string? MetricId { get; set; }
    /// <summary> Value that indicates whether AutoInflate is enabled for eventhub namespace. </summary>
    public bool? IsAutoInflateEnabled { get; set; }
    /// <summary> Upper limit of throughput units when AutoInflate is enabled, value should be within 0 to 20 throughput units. ( '0' if AutoInflateEnabled = true). </summary>
    public int? MaximumThroughputUnits { get; set; }
    /// <summary> Enabling this property creates a Standard Event Hubs Namespace in regions supported availability zones. </summary>
    public bool? ZoneRedundant { get; set; }
    /// <summary> Value that indicates whether Kafka is enabled for eventhub namespace. </summary>
    public bool? KafkaEnabled { get; set; }
}
