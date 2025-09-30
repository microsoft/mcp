// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ApplicationInsights.Models;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public interface IApplicationInsightsService
{
    Task<IEnumerable<JsonNode>> GetProfilerInsightsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// List Application Insights trace metadata (placeholder until full trace retrieval is implemented).
    /// Currently returns basic component information that can be used to scope future trace queries.
    /// </summary>
    Task<AppListTraceResult> ListDistributedTracesAsync(
        string subscription,
        string? resourceGroup,
        string? resourceName,
        string? resourceId,
        string[] filters,
        string table,
        DateTime startTime,
        DateTime endTime,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
