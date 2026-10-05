// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Models.VmSku;

namespace Azure.Mcp.Tools.InfraIq.Services;

public interface IInfraIqService
{
    /// <summary>
    /// Resolves the subscription and requests VM SKU recommendations from InfraIQ.
    /// </summary>
    Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscription,
        string location,
        InfraIqRecommendVmSkuRequestBody body,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
