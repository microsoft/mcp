// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorServiceGroupIntelligenceService
{
    /// <summary>
    /// Gets the Advisor status of one Service Group, or of all Service Groups when none is given,
    /// optionally filtered by criticality and status.
    /// </summary>
    Task<ResourceQueryResults<AdvisorServiceGroupStatus>> GetStatusAsync(
        string? serviceGroup,
        string? criticality,
        string? status,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
