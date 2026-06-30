// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.CloudHealth;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorHealthModelService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IMonitorHealthModelService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    private static readonly ModelReaderWriterContext s_context = AzureResourceManagerCloudHealthContext.Default;

    private static JsonNode ToJson(object model) =>
        JsonNode.Parse(ModelReaderWriter.Write(model, ModelReaderWriterOptions.Json, s_context).ToString())
            ?? throw new InvalidOperationException("Failed to serialize the health model response.");

    public async Task<List<JsonNode>> ListHealthModels(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var results = new List<JsonNode>();

        if (string.IsNullOrEmpty(resourceGroup))
        {
            await foreach (var model in subscriptionResource.GetHealthModelsAsync(cancellationToken))
            {
                results.Add(ToJson(model.Data));
            }
        }
        else
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            await foreach (var model in resourceGroupResource.Value.GetHealthModels().GetAllAsync(cancellationToken: cancellationToken))
            {
                results.Add(ToJson(model.Data));
            }
        }

        return results;
    }

    public async Task<JsonNode> GetHealthModel(
        string subscription,
        string resourceGroup,
        string healthModelName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(healthModelName), healthModelName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var model = await resourceGroupResource.Value.GetHealthModels().GetAsync(healthModelName, cancellationToken);
        return ToJson(model.Value.Data);
    }
}
