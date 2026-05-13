// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssUpdateResult(
    string Name,
    string? Id,
    string? Location,
    string? VmSize,
    string? ProvisioningState,
    int? Capacity,
    string? UpgradePolicy,
    IReadOnlyList<string>? Zones,
    IReadOnlyDictionary<string, string>? Tags);
