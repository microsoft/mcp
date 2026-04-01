// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorService
{
    Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);
}
