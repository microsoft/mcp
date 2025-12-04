// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Azure.ResourceManager.DeviceRegistry;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.DeviceRegistry.Services;

public sealed class DeviceRegistryService(ITenantService tenantService) : BaseAzureService(tenantService), IDeviceRegistryService
{
    public async Task<List<DeviceRegistryNamespaceResource>> GetNamespacesAsync(
        string subscriptionId,
        string? resourceGroupName = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var armClient = new ArmClient(credential);
        var subscription = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

        var namespaces = new List<DeviceRegistryNamespaceResource>();

        if (!string.IsNullOrEmpty(resourceGroupName))
        {
            // List namespaces in a specific resource group
            var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName, cancellationToken);
            var namespacesCollection = resourceGroup.Value.GetDeviceRegistryNamespaces();
            
            await foreach (var ns in namespacesCollection.GetAllAsync(cancellationToken: cancellationToken))
            {
                namespaces.Add(ns);
            }
        }
        else
        {
            // List namespaces across all resource groups in the subscription
            await foreach (var ns in subscription.GetDeviceRegistryNamespacesAsync(cancellationToken: cancellationToken))
            {
                namespaces.Add(ns);
            }
        }

        return namespaces;
    }
}
