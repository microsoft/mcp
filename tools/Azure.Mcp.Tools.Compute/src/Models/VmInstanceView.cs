// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmInstanceView(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("powerState")] string? PowerState,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("vmAgent")] VmAgentInfo? VmAgent,
    [property: JsonPropertyName("disks")] IReadOnlyList<DiskInstanceView>? Disks,
    [property: JsonPropertyName("extensions")] IReadOnlyList<ExtensionInstanceView>? Extensions,
    [property: JsonPropertyName("statuses")] IReadOnlyList<StatusInfo>? Statuses);

public sealed record VmAgentInfo(
    [property: JsonPropertyName("vmAgentVersion")] string? VmAgentVersion,
    [property: JsonPropertyName("statuses")] IReadOnlyList<StatusInfo>? Statuses);

public sealed record DiskInstanceView(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("statuses")] IReadOnlyList<StatusInfo>? Statuses);

public sealed record ExtensionInstanceView(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("typeHandlerVersion")] string? TypeHandlerVersion,
    [property: JsonPropertyName("statuses")] IReadOnlyList<StatusInfo>? Statuses);

public sealed record StatusInfo(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("level")] string? Level,
    [property: JsonPropertyName("displayStatus")] string? DisplayStatus,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("time")] DateTimeOffset? Time);
