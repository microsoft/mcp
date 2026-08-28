// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>Current or target SKU/instance configuration surfaced by the explanation tool.</summary>
public sealed record SkuConfiguration(
    string Sku,
    int InstanceCount,
    int AvailableVcpus,
    double MemoryGB,
    double? NetworkMbps);

/// <summary>Maximum utilization thresholds used to evaluate the projection.</summary>
public sealed record UtilizationThresholds(
    double CpuPercent,
    double UsedMemoryPercent,
    double NetworkPercent);

/// <summary>
/// Result of the recommendation explanation tool: the matching Advisor recommendation count, the
/// current-versus-target configuration, and the projected utilization time-series (current versus
/// target) as structured data for the agent to render as an inline chart.
/// </summary>
public sealed record RecommendationExplanationResult(
    string RenderingInstructions,
    int RecommendationCount,
    string ResourceId,
    string? Location,
    string? ResourceKind,
    SkuConfiguration? Current,
    SkuConfiguration? Target,
    UtilizationThresholds? Thresholds,
    RecommendationUtilization? RecentUtilization,
    RecommendationUtilization? LongTermUtilization);
