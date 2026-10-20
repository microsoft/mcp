// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AIInfraOptimization.Models.Request;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;

namespace Azure.Mcp.Tools.AIInfraOptimization.Services;

internal interface IAIInfraOptimizationArmClient
{
    /// <summary>
    /// Calls the Microsoft.AIInfraOptimization recommendVmSku action for a canonical subscription ID and normalized location.
    /// </summary>
    Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscriptionId,
        string location,
        AIInfraOptimizationRecommendVmSkuRequestBody body,
        string? tenant,
        CancellationToken cancellationToken);
}
