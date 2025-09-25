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
}
