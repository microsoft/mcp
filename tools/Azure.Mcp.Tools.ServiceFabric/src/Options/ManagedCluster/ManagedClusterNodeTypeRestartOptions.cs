// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;

public class ManagedClusterNodeTypeRestartOptions : BaseServiceFabricOptions
{
    [JsonPropertyName(ServiceFabricOptionDefinitions.ClusterName)]
    public string? ClusterName { get; set; }

    [JsonPropertyName(ServiceFabricOptionDefinitions.NodeTypeName)]
    public string? NodeType { get; set; }

    [JsonPropertyName(ServiceFabricOptionDefinitions.NodesName)]
    public string[]? Nodes { get; set; }

    [JsonPropertyName(ServiceFabricOptionDefinitions.UpdateTypeName)]
    public string? UpdateType { get; set; }
}
