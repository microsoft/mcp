// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public interface IApplicationInsightsService
{
    Task<IEnumerable<JsonNode>> GetProfilerInsightsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
