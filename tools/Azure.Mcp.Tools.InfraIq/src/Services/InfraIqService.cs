// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Models.VmSku;

namespace Azure.Mcp.Tools.InfraIq.Services;

internal sealed class InfraIqService(IAzureService azureService, IInfraIqArmClient armClient)
    : BaseAzureService(azureService), IInfraIqService
{
    private readonly IInfraIqArmClient _armClient = armClient;

    public async Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscription,
        string location,
        InfraIqRecommendVmSkuRequestBody body,
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
