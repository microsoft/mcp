// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssInfo(
    string Name,
    string? Id,
    string? Location,
    VmssSkuInfo? Sku,
    long? Capacity,
    string? ProvisioningState,
    string? UpgradePolicy,
    bool? Overprovision,
    IReadOnlyList<string>? Zones,
    IReadOnlyDictionary<string, string>? Tags);

public sealed record VmssSkuInfo(string? Name, string? Tier, long? Capacity);
