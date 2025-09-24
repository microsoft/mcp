// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Options;

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
    Task<IEnumerable<JsonNode>> GetAppTracesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        DateTime? startDateTimeUtc = null,
        DateTime? endDateTimeUtc = null);
}
