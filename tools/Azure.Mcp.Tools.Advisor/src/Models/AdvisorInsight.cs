// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A single Azure Advisor insight (a 'microsoft.advisor/insights' resource) for a Service Group or subscription.
/// </summary>
public sealed record AdvisorInsight(
    string Name,
    string? Domain,
    string ScopeType,
    string ScopeId,
    DateTimeOffset? LastUpdatedTime,
    List<AdvisorInsightKpi> Kpis);

