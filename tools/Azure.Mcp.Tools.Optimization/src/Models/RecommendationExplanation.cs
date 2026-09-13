// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>
/// Recommendation explanation projection describing the current and target SKU/instance
/// configuration and the utilization thresholds used to project load.
/// </summary>
public sealed class RecommendationExplanation
{
    public string ResourceId { get; set; } = string.Empty;
    public string? SubscriptionId { get; set; }
    public string? ResourceGroupName { get; set; }
    public string? ResourceName { get; set; }
    public string? RecommendationSubType { get; set; }
    public string? RecommendationMessage { get; set; }

    public string? SKU { get; set; }
    public string? NewSKU { get; set; }
    public int? SkuCores { get; set; }
    public int? NewSkuCores { get; set; }
    public double? MemoryGB { get; set; }
    public double? NewMemoryGB { get; set; }
    public double? NetworkMbps { get; set; }
    public double? NewNetworkMbps { get; set; }
    public int? CurrentInstanceCount { get; set; }
    public int? NewInstanceCount { get; set; }

    public double? MaxCpuThreshold { get; set; }
    public double? MaxMemoryThreshold { get; set; }
    public double? MaxNetworkThreshold { get; set; }
}
