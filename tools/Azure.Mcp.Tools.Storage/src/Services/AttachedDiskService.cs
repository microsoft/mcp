// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;

namespace Azure.Mcp.Tools.Storage.Services;

public sealed class AttachedDiskService(ISubscriptionService subscriptionService) : IAttachedDiskService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    public async Task<(string VmResourceId, string[]? DiskResourceIds)> ResolveFriendlySelectorAsync(
        string subscription,
        string resourceGroup,
        string vm,
        string[]? diskNames,
        CancellationToken cancellationToken)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(
            subscription,
            tenant: null,
            retryPolicy: null,
            cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var vmResource = await resourceGroupResource.Value
            .GetVirtualMachines()
            .GetAsync(vm, cancellationToken: cancellationToken);

        return (
            vmResource.Value.Id.ToString(),
            ResolveDiskResourceIds(vmResource.Value.Data, diskNames));
    }

    public async Task<string[]> ResolveDiskNamesAsync(
        string vmResourceId,
        string[] diskNames,
        CancellationToken cancellationToken)
    {
        var resourceId = new ResourceIdentifier(vmResourceId);
        var subscriptionId = resourceId.SubscriptionId
            ?? throw new InvalidOperationException("The virtual machine resource ID does not contain a subscription ID.");
        var resourceGroupName = resourceId.ResourceGroupName
            ?? throw new InvalidOperationException("The virtual machine resource ID does not contain a resource group name.");
        var subscriptionResource = await _subscriptionService.GetSubscription(
            subscriptionId,
            tenant: null,
            retryPolicy: null,
            cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroupName, cancellationToken);
        var vmResource = await resourceGroupResource.Value
            .GetVirtualMachines()
            .GetAsync(resourceId.Name, cancellationToken: cancellationToken);

        return ResolveDiskResourceIds(vmResource.Value.Data, diskNames) ?? [];
    }

    private static string[]? ResolveDiskResourceIds(VirtualMachineData vmData, string[]? diskNames)
    {
        if (diskNames is not { Length: > 0 })
        {
            return null;
        }

        var attachedDisks = new List<(string? Name, string? ResourceId)>();
        var osDisk = vmData.StorageProfile?.OSDisk;
        if (osDisk is not null)
        {
            attachedDisks.Add((osDisk.Name, osDisk.ManagedDisk?.Id?.ToString()));
        }
        if (vmData.StorageProfile?.DataDisks is not null)
        {
            attachedDisks.AddRange(vmData.StorageProfile.DataDisks.Select(dataDisk =>
                ((string?)dataDisk.Name, dataDisk.ManagedDisk?.Id?.ToString())));
        }

        return AttachedDiskResolver.ResolveResourceIds(attachedDisks, diskNames);
    }
}
