// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorService
{
    Task<List<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);
}
