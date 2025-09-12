// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Aks.Options.Nodepool;

public class NodepoolGetOptions : BaseAksOptions
{
    [JsonPropertyName(AksOptionDefinitions.ClusterName)]
    public string? ClusterName { get; set; }

    [JsonPropertyName(AksOptionDefinitions.NodepoolName)]
    public string? NodepoolName { get; set; }
}

