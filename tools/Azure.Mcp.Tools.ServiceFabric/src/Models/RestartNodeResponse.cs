// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ServiceFabric.Models;

/// <summary>
/// Response from the restart nodes REST API.
/// The restart operation is a long-running operation (LRO) that returns 202 Accepted.
/// </summary>
public class RestartNodeResponse
{
    /// <summary> The HTTP status code of the response. </summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    /// <summary> The Azure-AsyncOperation URL for polling the operation status. </summary>
    [JsonPropertyName("asyncOperationUrl")]
    public string? AsyncOperationUrl { get; set; }

    /// <summary> The Location header URL for polling the operation result. </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary> Additional properties from the response. </summary>
    [JsonExtensionData]
    public JsonObject? AdditionalProperties { get; set; }
}
