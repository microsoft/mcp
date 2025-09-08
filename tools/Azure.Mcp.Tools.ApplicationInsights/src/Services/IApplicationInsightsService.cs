// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ApplicationInsights.Models;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public interface IApplicationInsightsService
{
    Task<List<ApplicationInsightsInfo>> ListApplicationInsights(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
