// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Aks.Options.Cluster;

public class ClusterGetOptions : BaseAksOptions
{
    [JsonPropertyName(AksOptionDefinitions.ClusterName)]
    public string? ClusterName { get; set; }
}
