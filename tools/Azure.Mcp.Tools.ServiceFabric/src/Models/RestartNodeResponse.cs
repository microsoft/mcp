// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ServiceFabric.Models;

/// <summary>
/// Response from the restart nodes REST API.
/// The restart operation is a long-running operation (LRO) that returns 202 Accepted.
/// </summary>
public class RestartNodeResponse
{
    /// <summary> The HTTP status code of the response. </summary>
    public int StatusCode { get; set; }

    /// <summary> The Azure-AsyncOperation URL for polling the operation status. </summary>
    public string? AsyncOperationUrl { get; set; }

    /// <summary> The Location header URL for polling the operation result. </summary>
    public string? Location { get; set; }
}
