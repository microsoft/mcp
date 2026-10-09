// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IServiceGroupIntelligenceService
{
    /// <summary>
    /// Returns Status, criticality and Insight KPIs per Service Group, read from Azure Resource Graph.
    /// </summary>
    Task<ServiceGroupStatusInsightsPage> GetStatusInsightsAsync(
        string[]? serviceGroups,
        string[]? criticalityTiers,
        string[]? statuses,
        string[]? insightNames,
        bool includeStatus,
        bool includeInsights,
        string? continuationToken,
        CancellationToken cancellationToken = default);
}
