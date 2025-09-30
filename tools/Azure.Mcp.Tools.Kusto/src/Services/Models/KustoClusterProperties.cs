// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Kusto.Services.Models;

/// <summary>
/// A class representing the Kusto Cluster properties model.
/// </summary>
internal sealed class KustoClusterProperties
{
    /// <summary> The state of the resource. </summary>
    public string? State { get; set; }
    /// <summary> The provisioned state of the resource. </summary>
    public string? ProvisioningState { get; set; }
    /// <summary> The cluster URI. </summary>
    [JsonPropertyName("uri")]
    public Uri? ClusterUri { get; set; }
    /// <summary> The cluster data ingestion URI. </summary>
    public Uri? DataIngestionUri { get; set; }
    /// <summary> The reason for the cluster's current state. </summary>
    public string? StateReason { get; set; }
    /// <summary> A boolean value that indicates if the streaming ingest is enabled. </summary>
    [JsonPropertyName("properties.enableStreamingIngest")]
    public bool? IsStreamingIngestEnabled { get; set; }
    /// <summary> The engine type. </summary>
    public string? EngineType { get; set; }
    /// <summary> A boolean value that indicates if the cluster could be automatically stopped (due to lack of data or no activity for many days). </summary>
    [JsonPropertyName("enableAutoStop")]
    public bool? IsAutoStopEnabled { get; set; }
}
