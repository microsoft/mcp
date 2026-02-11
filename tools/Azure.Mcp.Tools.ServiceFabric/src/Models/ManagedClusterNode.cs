// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ServiceFabric.Models;

/// <summary>
/// Represents a node in a Service Fabric managed cluster as returned by the REST API.
/// </summary>
public class ManagedClusterNode
{
    /// <summary> The Azure resource ID of the node. </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary> The node properties. </summary>
    [JsonPropertyName("properties")]
    public ManagedClusterNodeProperties? Properties { get; set; }
}

/// <summary>
/// Properties of a node in a Service Fabric managed cluster.
/// </summary>
public class ManagedClusterNodeProperties
{
    /// <summary> The name of the node. </summary>
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    /// <summary> The IP address or FQDN of the node. </summary>
    [JsonPropertyName("IpAddressOrFQDN")]
    public string? IpAddressOrFQDN { get; set; }

    /// <summary> The node type this node belongs to. </summary>
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    /// <summary> The Service Fabric runtime code version on the node. </summary>
    [JsonPropertyName("CodeVersion")]
    public string? CodeVersion { get; set; }

    /// <summary> The configuration version of the node. </summary>
    [JsonPropertyName("ConfigVersion")]
    public string? ConfigVersion { get; set; }

    /// <summary> The status of the node (numeric). </summary>
    [JsonPropertyName("NodeStatus")]
    public int? NodeStatus { get; set; }

    /// <summary> The time in seconds the node has been up. </summary>
    [JsonPropertyName("NodeUpTimeInSeconds")]
    public string? NodeUpTimeInSeconds { get; set; }

    /// <summary> The health state of the node (numeric). </summary>
    [JsonPropertyName("HealthState")]
    public int? HealthState { get; set; }

    /// <summary> Whether this node is a seed node. </summary>
    [JsonPropertyName("IsSeedNode")]
    public bool? IsSeedNode { get; set; }

    /// <summary> The upgrade domain of the node. </summary>
    [JsonPropertyName("UpgradeDomain")]
    public string? UpgradeDomain { get; set; }

    /// <summary> The fault domain of the node. </summary>
    [JsonPropertyName("FaultDomain")]
    public string? FaultDomain { get; set; }

    /// <summary> The internal node identifier. </summary>
    [JsonPropertyName("Id")]
    public NodeIdentifier? NodeId { get; set; }

    /// <summary> The instance ID of the node. </summary>
    [JsonPropertyName("InstanceId")]
    public string? InstanceId { get; set; }

    /// <summary> The time in seconds the node has been down. </summary>
    [JsonPropertyName("NodeDownTimeInSeconds")]
    public string? NodeDownTimeInSeconds { get; set; }

    /// <summary> The timestamp when the node came up. </summary>
    [JsonPropertyName("NodeUpAt")]
    public string? NodeUpAt { get; set; }

    /// <summary> The timestamp when the node went down. </summary>
    [JsonPropertyName("NodeDownAt")]
    public string? NodeDownAt { get; set; }

    /// <summary> The infrastructure placement ID. </summary>
    [JsonPropertyName("InfrastructurePlacementID")]
    public string? InfrastructurePlacementID { get; set; }

    /// <summary> Additional properties not explicitly modeled. </summary>
    [JsonExtensionData]
    public JsonObject? AdditionalProperties { get; set; }
}

/// <summary>
/// Represents the internal node identifier.
/// </summary>
public class NodeIdentifier
{
    /// <summary> The node ID string. </summary>
    [JsonPropertyName("Id")]
    public string? Id { get; set; }
}

/// <summary>
/// Represents the response from the list nodes REST API.
/// </summary>
internal class ListNodesResponse
{
    [JsonPropertyName("value")]
    public List<ManagedClusterNode>? Value { get; set; }

    [JsonPropertyName("nextLink")]
    public string? NextLink { get; set; }
}
