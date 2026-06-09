// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssVmInfo(
    string InstanceId,
    string? Name,
    string? Id,
    string? Location,
    string? VmSize,
    string? ProvisioningState,
    string? OsType,
    IReadOnlyList<string>? Zones,
    IReadOnlyDictionary<string, string>? Tags);
