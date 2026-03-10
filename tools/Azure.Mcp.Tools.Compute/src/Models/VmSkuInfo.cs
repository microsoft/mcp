// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Represents information about a VM SKU including capabilities and availability zone placement.
/// </summary>
public sealed record VmSkuInfo(
    string? Name,
    string? Tier,
    string? Size,
    string? Family,
    IReadOnlyList<string>? Locations,
    IReadOnlyList<string>? Zones,
    IReadOnlyList<VmSkuCapability>? Capabilities,
    IReadOnlyList<VmSkuRestriction>? Restrictions,
    VmSkuCapacity? Capacity);

/// <summary>
/// Represents a capability of a VM SKU.
/// </summary>
public sealed record VmSkuCapability(
    string? Name,
    string? Value);

/// <summary>
/// Represents a restriction on a VM SKU.
/// </summary>
public sealed record VmSkuRestriction(
    string? Type,
    IReadOnlyList<string>? Values,
    string? ReasonCode);

/// <summary>
/// Represents the capacity information of a VM SKU.
/// </summary>
public sealed record VmSkuCapacity(
    long? Minimum,
    long? Maximum,
    long? Default,
    string? ScaleType);
