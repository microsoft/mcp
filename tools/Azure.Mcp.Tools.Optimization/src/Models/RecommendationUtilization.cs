// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>Projected current-versus-target utilization time series for the explanation tool.</summary>
public sealed class RecommendationUtilization
{
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public int IntervalMinutes { get; init; }
    public string? MemoryUnavailableReason { get; init; }
    public List<RecommendationUtilizationPoint> Points { get; init; } = new();
}

public sealed class RecommendationUtilizationPoint
{
    public DateTimeOffset Timestamp { get; init; }
    public UtilizationValues Current { get; init; } = new();
    public UtilizationValues Projected { get; init; } = new();
}

public sealed class UtilizationValues
{
    public double? CpuPercent { get; init; }
    public double? UsedMemoryPercent { get; init; }
    public double? NetworkPercent { get; init; }
}
