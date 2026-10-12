// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Request;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;

namespace Azure.Mcp.Tools.AIInfraOptimization.Services;

internal sealed class AIInfraOptimizationService(IAzureService azureService, IAIInfraOptimizationArmClient armClient)
    : BaseAzureService(azureService), IAIInfraOptimizationService
{
    private readonly IAIInfraOptimizationArmClient _armClient = armClient;

    public async Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscription,
        string location,
        AIInfraOptimizationRecommendVmSkuRequestBody body,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(location), location));
        ArgumentNullException.ThrowIfNull(body);

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");

        return await _armClient.RecommendVmSkuAsync(subscriptionId, location, body, tenant, cancellationToken);
    }
}
