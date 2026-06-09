// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmInstanceView(
    string Name,
    string? PowerState,
    string? ProvisioningState,
    VmAgentInfo? VmAgent,
    IReadOnlyList<DiskInstanceView>? Disks,
    IReadOnlyList<ExtensionInstanceView>? Extensions,
    IReadOnlyList<StatusInfo>? Statuses);

public sealed record VmAgentInfo(string? VmAgentVersion, IReadOnlyList<StatusInfo>? Statuses);

public sealed record DiskInstanceView(string? Name, IReadOnlyList<StatusInfo>? Statuses);

public sealed record ExtensionInstanceView(
    string? Name,
    string? Type,
    string? TypeHandlerVersion,
    IReadOnlyList<StatusInfo>? Statuses);

public sealed record StatusInfo(
    string? Code,
    string? Level,
    string? DisplayStatus,
    string? Message,
    DateTimeOffset? Time);
