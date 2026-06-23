// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>
/// Describes the current state of a Fabric long running operation.
/// Returned by GET /v1/operations/{operationId}.
/// </summary>
public class OperationState
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("percentComplete")]
    public int? PercentComplete { get; set; }

    [JsonPropertyName("createdTimeUtc")]
    public string? CreatedTimeUtc { get; set; }

    [JsonPropertyName("lastUpdatedTimeUtc")]
    public string? LastUpdatedTimeUtc { get; set; }

    [JsonPropertyName("error")]
    public OperationError? Error { get; set; }
}

/// <summary>
/// Error details returned when a long running operation fails.
/// </summary>
public class OperationError
{
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
