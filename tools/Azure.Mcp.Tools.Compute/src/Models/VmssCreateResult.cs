// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssCreateResult(
    string Name,
    string? Id,
    string? Location,
    string? VmSize,
    string? ProvisioningState,
    string? OsType,
    int Capacity,
    string? UpgradePolicy,
    IReadOnlyList<string>? Zones,
    IReadOnlyDictionary<string, string>? Tags);
