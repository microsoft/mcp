// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;

namespace Azure.Mcp.Tools.Storage.Services;

public sealed class AttachedDiskService(IAzureService azureService) : IAttachedDiskService
{
    private static readonly ResourceType s_virtualMachineResourceType = new("Microsoft.Compute/virtualMachines");
    private static readonly ResourceType s_virtualMachineScaleSetResourceType = new("Microsoft.Compute/virtualMachineScaleSets");
    private static readonly ResourceType s_virtualMachineScaleSetVmResourceType = new("Microsoft.Compute/virtualMachineScaleSets/virtualMachines");
    private readonly IAzureService _azureService = azureService;

    public async Task<(string VmResourceId, string[]? DiskResourceIds)> ResolveFriendlySelectorAsync(
        string subscription,
        string resourceGroup,
        string vm,
        string[]? diskNames,
        CancellationToken cancellationToken)
    {
        var subscriptionResource = await _azureService.GetSubscription(
            subscription,
            tenant: null,
            cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var vmResource = await resourceGroupResource.Value
            .GetVirtualMachines()
            .GetAsync(vm, cancellationToken: cancellationToken);

        return (
            vmResource.Value.Id.ToString(),
            ResolveDiskResourceIds(vmResource.Value.Data.StorageProfile, diskNames));
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
        var subscriptionResource = await _azureService.GetSubscription(
            subscriptionId,
            tenant: null,
            cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroupName, cancellationToken);
        VirtualMachineStorageProfile? storageProfile;
        if (resourceId.ResourceType == s_virtualMachineResourceType)
        {
            var vmResource = await resourceGroupResource.Value
                .GetVirtualMachines()
                .GetAsync(resourceId.Name, cancellationToken: cancellationToken);
            storageProfile = vmResource.Value.Data.StorageProfile;
        }
        else if (resourceId.ResourceType == s_virtualMachineScaleSetVmResourceType
            && resourceId.Parent is { } parent
            && parent.ResourceType == s_virtualMachineScaleSetResourceType)
        {
            var vmssResource = await resourceGroupResource.Value
                .GetVirtualMachineScaleSets()
                .GetAsync(parent.Name, cancellationToken: cancellationToken);
            var vmssVmResource = await vmssResource.Value
                .GetVirtualMachineScaleSetVms()
                .GetAsync(resourceId.Name, cancellationToken: cancellationToken);
            storageProfile = vmssVmResource.Value.Data.StorageProfile;
        }
        else
        {
            throw new ArgumentException(
                "The compute instance resource ID must identify a standalone virtual machine or virtual machine scale set instance.",
                nameof(vmResourceId));
        }

        return ResolveDiskResourceIds(storageProfile, diskNames) ?? [];
    }

    private static string[]? ResolveDiskResourceIds(
        VirtualMachineStorageProfile? storageProfile,
        string[]? diskNames)
    {
        if (diskNames is not { Length: > 0 })
        {
            return null;
        }

        var attachedDisks = new List<(string? Name, string? ResourceId)>();
        var osDisk = storageProfile?.OSDisk;
        if (osDisk is not null)
        {
            attachedDisks.Add((osDisk.Name, osDisk.ManagedDisk?.Id?.ToString()));
        }
        if (storageProfile?.DataDisks is not null)
        {
            attachedDisks.AddRange(storageProfile.DataDisks.Select(dataDisk =>
                ((string?)dataDisk.Name, dataDisk.ManagedDisk?.Id?.ToString())));
        }

        return AttachedDiskResolver.ResolveResourceIds(attachedDisks, diskNames);
    }
}
