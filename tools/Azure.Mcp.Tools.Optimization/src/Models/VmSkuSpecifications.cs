// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>Current versus target VM SKU comparison used by the recommendation explanation.</summary>
public sealed record ResourceSkuComparison(
    string ResourceId,
    string Location,
    string ResourceKind,
    int CurrentInstanceCount,
    VmSkuSpecifications Current,
    VmSkuSpecifications Target);

/// <summary>Minimal VM SKU specifications required to project utilization.</summary>
public sealed record VmSkuSpecifications(
    string Name,
    int AvailableVcpus,
    double MemoryGB);
