// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Models.VmSku;

namespace Azure.Mcp.Tools.InfraIq.Services;

internal interface IInfraIqArmClient
{
    /// <summary>
    /// Calls the Private.InfraIQ recommendVmSku action for a canonical subscription ID and normalized location.
    /// </summary>
    Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscriptionId,
        string location,
        InfraIqRecommendVmSkuRequestBody body,
        string? tenant,
        CancellationToken cancellationToken);
}
