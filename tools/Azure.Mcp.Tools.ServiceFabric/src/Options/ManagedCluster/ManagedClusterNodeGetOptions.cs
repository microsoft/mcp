// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;

public class ManagedClusterNodeGetOptions : BaseServiceFabricOptions
{
    [JsonPropertyName(ServiceFabricOptionDefinitions.ClusterName)]
    public string? ClusterName { get; set; }

    [JsonPropertyName(ServiceFabricOptionDefinitions.NodeName)]
    public string? NodeName { get; set; }
}
