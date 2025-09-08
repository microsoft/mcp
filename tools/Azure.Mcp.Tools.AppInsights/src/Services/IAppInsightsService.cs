// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppInsights.Models;

namespace Azure.Mcp.Tools.AppInsights.Services;

public interface IAppInsightsService
{
    Task<List<ApplicationInsightsInfo>> ListApplicationInsights(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
