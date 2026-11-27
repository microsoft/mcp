// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorServiceGroupIntelligenceService
{
    /// <summary>
    /// Gets one page of Service Group status, optionally filtered by criticality and status.
    /// </summary>
    Task<AdvisorQueryPage<AdvisorServiceGroupStatus>> GetStatusAsync(
        string? serviceGroup,
        string? criticality,
        string? status,
        int top,
        string? tenant,
        string? continuationToken,
        CancellationToken cancellationToken);
}
