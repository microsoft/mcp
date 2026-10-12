// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AIInfraOptimization.Models.Request;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;

namespace Azure.Mcp.Tools.AIInfraOptimization.Services;

public interface IAIInfraOptimizationService
{
    /// <summary>
    /// Resolves the subscription and requests VM SKU recommendations from AI Infra Optimization.
    /// </summary>
    Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscription,
        string location,
        AIInfraOptimizationRecommendVmSkuRequestBody body,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
