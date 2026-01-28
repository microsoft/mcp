// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssVmInfo(
    [property: JsonPropertyName("instanceId")] string InstanceId,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("vmSize")] string? VmSize,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("osType")] string? OsType,
    [property: JsonPropertyName("zones")] IReadOnlyList<string>? Zones,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags);
