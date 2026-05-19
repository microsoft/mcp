// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmPowerStateResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("resourceGroup")] string ResourceGroup,
    [property: JsonPropertyName("powerAction")] string PowerAction,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("completed")] bool Completed,
    [property: JsonPropertyName("statusUri")] string? StatusUri = null);
