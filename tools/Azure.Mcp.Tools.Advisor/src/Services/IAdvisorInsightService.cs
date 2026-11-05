// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorInsightService
{
    /// <summary>
    /// Lists Advisor insights for a Service Group or a subscription (mutually exclusive). When neither is given,
    /// insights across all accessible Service Groups and subscriptions in the tenant are returned.
    /// </summary>
    Task<ResourceQueryResults<AdvisorInsight>> ListInsightsAsync(
        string? serviceGroup,
        string? subscription,
        string? insightName,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
