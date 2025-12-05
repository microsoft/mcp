// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Azure.ResourceManager.DeviceRegistry;
using Azure.ResourceManager.DeviceRegistry.Models;
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

    public async Task<DeviceRegistryNamespaceResource> CreateNamespaceAsync(
        string subscriptionId,
        string resourceGroupName,
        string namespaceName,
        string location,
        Dictionary<string, string>? tags = null,
        bool enableSystemAssignedIdentity = false,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(resourceGroupName), resourceGroupName),
            (nameof(namespaceName), namespaceName),
            (nameof(location), location));

        var credential = await GetCredential(tenantId, cancellationToken);
        var armClient = new ArmClient(credential);
        var subscription = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

        var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName, cancellationToken);
        var namespacesCollection = resourceGroup.Value.GetDeviceRegistryNamespaces();

        var namespaceData = new DeviceRegistryNamespaceData(new AzureLocation(location));

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                namespaceData.Tags.Add(tag.Key, tag.Value);
            }
        }

        if (enableSystemAssignedIdentity)
        {
            namespaceData.Identity = new SystemAssignedServiceIdentity(SystemAssignedServiceIdentityType.SystemAssigned);
        }

        var operation = await namespacesCollection.CreateOrUpdateAsync(
            WaitUntil.Completed,
            namespaceName,
            namespaceData,
            cancellationToken);

        return operation.Value;
    }

    public async Task<DeviceRegistryNamespaceResource> GetNamespaceAsync(
        string subscriptionId,
        string resourceGroupName,
        string namespaceName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(resourceGroupName), resourceGroupName),
            (nameof(namespaceName), namespaceName));

        var credential = await GetCredential(tenantId, cancellationToken);
        var armClient = new ArmClient(credential);
        var subscription = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

        var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName, cancellationToken);
        var namespacesCollection = resourceGroup.Value.GetDeviceRegistryNamespaces();

        var namespaceResource = await namespacesCollection.GetAsync(namespaceName, cancellationToken);

        return namespaceResource.Value;
    }
}
