// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models.Chaos;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorChaosReviewService
{
    Task<ChaosRemediationStatus> ReviewChaosRemediationAsync(
        string subscription,
        Guid recommendationTypeId,
        string resource,
        string? workspace = null,
        string? scenario = null,
        string? configuration = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
