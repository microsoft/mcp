// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ServiceFabric.Models;

/// <summary>
/// Request body for the restart nodes REST API.
/// </summary>
public class RestartNodeRequest
{
    /// <summary> The list of node names to restart. </summary>
    [JsonPropertyName("Nodes")]
    public List<string> Nodes { get; set; } = [];

    /// <summary> The update type for the restart operation. </summary>
    [JsonPropertyName("UpdateType")]
    public string UpdateType { get; set; } = "Default";
}
