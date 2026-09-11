// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public sealed record ReplicationOperationResult(
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("volumeResourceId")] string VolumeResourceId,
    [property: JsonPropertyName("message")] string Message);