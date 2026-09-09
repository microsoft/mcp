// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>Azure Monitor utilization series used to project current versus target-SKU load.</summary>
public sealed class AzureMonitorUtilizationData
{
    public Dictionary<DateTimeOffset, double> CpuMaximumPercent { get; } = new();
    public Dictionary<DateTimeOffset, double> NetworkInTotalBytes { get; } = new();
    public Dictionary<DateTimeOffset, double> NetworkOutTotalBytes { get; } = new();
    public Dictionary<DateTimeOffset, double> UsedMemoryMaximumPercent { get; } = new();
    public string? MemoryUnavailableReason { get; set; }
}
