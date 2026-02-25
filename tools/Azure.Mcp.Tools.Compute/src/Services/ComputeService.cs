// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Compute.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Compute.Services;

public class ComputeService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<ComputeService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IComputeService
{
    private readonly ILogger<ComputeService> _logger = logger;

    public async Task<VmInfo> GetVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmResource = await subscriptionResource
            .GetResourceGroup(resourceGroup, cancellationToken)
            .Value
            .GetVirtualMachines()
            .GetAsync(vmName, cancellationToken: cancellationToken);

        return MapToVmInfo(vmResource.Value.Data);
    }

    public async Task<List<VmInfo>> ListVmsAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vms = new List<VmInfo>();

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            await foreach (var vm in rgResource.Value.GetVirtualMachines().GetAllAsync(cancellationToken: cancellationToken))
            {
                vms.Add(MapToVmInfo(vm.Data));
            }
        }
        else
        {
            await foreach (var vm in subscriptionResource.GetVirtualMachinesAsync(cancellationToken: cancellationToken))
            {
                vms.Add(MapToVmInfo(vm.Data));
            }
        }

        return vms;
    }

    public async Task<VmInstanceView> GetVmInstanceViewAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmResource = await subscriptionResource
            .GetResourceGroup(resourceGroup, cancellationToken)
            .Value
            .GetVirtualMachines()
            .GetAsync(vmName, cancellationToken: cancellationToken);

        var instanceView = await vmResource.Value.InstanceViewAsync(cancellationToken);

        return MapToVmInstanceView(vmName, instanceView.Value);
    }

    public async Task<(VmInfo VmInfo, VmInstanceView InstanceView)> GetVmWithInstanceViewAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmResource = await subscriptionResource
            .GetResourceGroup(resourceGroup, cancellationToken)
            .Value
            .GetVirtualMachines()
            .GetAsync(vmName, cancellationToken: cancellationToken);

        var vmInfo = MapToVmInfo(vmResource.Value.Data);
        var instanceView = await vmResource.Value.InstanceViewAsync(cancellationToken);
        var vmInstanceView = MapToVmInstanceView(vmName, instanceView.Value);

        return (vmInfo, vmInstanceView);
    }

    public async Task<VmssInfo> GetVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmssResource = await subscriptionResource
            .GetResourceGroup(resourceGroup, cancellationToken)
            .Value
            .GetVirtualMachineScaleSets()
            .GetAsync(vmssName, cancellationToken: cancellationToken);

        return MapToVmssInfo(vmssResource.Value.Data);
    }

    public async Task<List<VmssInfo>> ListVmssAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmssList = new List<VmssInfo>();

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            await foreach (var vmss in rgResource.Value.GetVirtualMachineScaleSets().GetAllAsync(cancellationToken: cancellationToken))
            {
                vmssList.Add(MapToVmssInfo(vmss.Data));
            }
        }
        else
        {
            await foreach (var vmss in subscriptionResource.GetVirtualMachineScaleSetsAsync(cancellationToken: cancellationToken))
            {
                vmssList.Add(MapToVmssInfo(vmss.Data));
            }
        }

        return vmssList;
    }

    public async Task<List<VmssVmInfo>> ListVmssVmsAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmssResource = await subscriptionResource
            .GetResourceGroup(resourceGroup, cancellationToken)
            .Value
            .GetVirtualMachineScaleSets()
            .GetAsync(vmssName, cancellationToken: cancellationToken);

        var vms = new List<VmssVmInfo>();
        await foreach (var vm in vmssResource.Value.GetVirtualMachineScaleSetVms().GetAllAsync(cancellationToken: cancellationToken))
        {
            vms.Add(MapToVmssVmInfo(vm.Data));
        }

        return vms;
    }

    public async Task<VmssVmInfo> GetVmssVmAsync(
        string vmssName,
        string instanceId,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var vmssResource = await subscriptionResource
            .GetResourceGroup(resourceGroup, cancellationToken)
            .Value
            .GetVirtualMachineScaleSets()
            .GetAsync(vmssName, cancellationToken: cancellationToken);

        var vmResource = await vmssResource.Value
            .GetVirtualMachineScaleSetVms()
            .GetAsync(instanceId, cancellationToken: cancellationToken);

        return MapToVmssVmInfo(vmResource.Value.Data);
    }

    private static VmInfo MapToVmInfo(VirtualMachineData data)
    {
        return new VmInfo(
            Name: data.Name,
            Id: data.Id?.ToString(),
            Location: data.Location.Name,
            VmSize: data.HardwareProfile?.VmSize?.ToString(),
            ProvisioningState: data.ProvisioningState,
            OsType: data.StorageProfile?.OSDisk?.OSType?.ToString(),
            LicenseType: data.LicenseType,
            Zones: data.Zones?.ToList(),
            Tags: data.Tags as IReadOnlyDictionary<string, string>);
    }

    private static VmInstanceView MapToVmInstanceView(string vmName, VirtualMachineInstanceView instanceView)
    {
        var powerState = instanceView.Statuses?
            .FirstOrDefault(s => s.Code?.StartsWith("PowerState/", StringComparison.OrdinalIgnoreCase) == true)
            ?.Code?.Split('/')
            .LastOrDefault();

        var provisioningState = instanceView.Statuses?
            .FirstOrDefault(s => s.Code?.StartsWith("ProvisioningState/", StringComparison.OrdinalIgnoreCase) == true)
            ?.Code?.Split('/')
            .LastOrDefault();

        return new VmInstanceView(
            Name: vmName,
            PowerState: powerState,
            ProvisioningState: provisioningState,
            VmAgent: instanceView.VmAgent != null ? new VmAgentInfo(
                VmAgentVersion: instanceView.VmAgent.VmAgentVersion,
                Statuses: instanceView.VmAgent.Statuses?.Select(s => MapToStatusInfo(s)).ToList()
            ) : null,
            Disks: instanceView.Disks?.Select(d => new Models.DiskInstanceView(
                Name: d.Name,
                Statuses: d.Statuses?.Select(s => MapToStatusInfo(s)).ToList()
            )).ToList(),
            Extensions: instanceView.Extensions?.Select(e => new ExtensionInstanceView(
                Name: e.Name,
                Type: e.VirtualMachineExtensionInstanceViewType,
                TypeHandlerVersion: e.TypeHandlerVersion,
                Statuses: e.Statuses?.Select(s => MapToStatusInfo(s)).ToList()
            )).ToList(),
            Statuses: instanceView.Statuses?.Select(s => MapToStatusInfo(s)).ToList()
        );
    }

    private static StatusInfo MapToStatusInfo(InstanceViewStatus status)
    {
        return new StatusInfo(
            Code: status.Code,
            Level: status.Level?.ToString(),
            DisplayStatus: status.DisplayStatus,
            Message: status.Message,
            Time: status.Time
        );
    }

    private static VmssInfo MapToVmssInfo(VirtualMachineScaleSetData data)
    {
        return new VmssInfo(
            Name: data.Name,
            Id: data.Id?.ToString(),
            Location: data.Location.Name,
            Sku: data.Sku != null ? new VmssSkuInfo(
                Name: data.Sku.Name,
                Tier: data.Sku.Tier,
                Capacity: data.Sku.Capacity
            ) : null,
            Capacity: data.Sku?.Capacity,
            ProvisioningState: data.ProvisioningState,
            UpgradePolicy: data.UpgradePolicy?.Mode?.ToString(),
            Overprovision: data.Overprovision,
            Zones: data.Zones?.ToList(),
            Tags: data.Tags as IReadOnlyDictionary<string, string>);
    }

    private static VmssVmInfo MapToVmssVmInfo(VirtualMachineScaleSetVmData data)
    {
        return new VmssVmInfo(
            InstanceId: data.InstanceId,
            Name: data.Name,
            Id: data.Id?.ToString(),
            Location: data.Location.Name,
            VmSize: data.HardwareProfile?.VmSize?.ToString(),
            ProvisioningState: data.ProvisioningState,
            OsType: data.StorageProfile?.OSDisk?.OSType?.ToString(),
            Zones: data.Zones?.ToList(),
            Tags: data.Tags as IReadOnlyDictionary<string, string>
        );
    }

    public async Task<DiskInfo> GetDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var diskResource = await resourceGroupResource.Value.GetManagedDisks().GetAsync(diskName, cancellationToken);

        return ConvertToDiskModel(diskResource.Value, resourceGroup);
    }

    /// <inheritdoc/>
    public async Task<List<DiskInfo>> ListDisksAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                SubscriptionResource.CreateResourceIdentifier(subscription));
            var disks = new List<DiskInfo>();

            if (!string.IsNullOrEmpty(resourceGroup))
            {
                // List disks in specific resource group
                var rg = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                if (rg?.Value == null)
                {
                    throw new InvalidOperationException($"Resource group '{resourceGroup}' not found.");
                }

                await foreach (var diskResource in rg.Value.GetManagedDisks().GetAllAsync(cancellationToken))
                {
                    disks.Add(ConvertToDiskModel(diskResource, resourceGroup));
                }
            }
            else
            {
                // List all disks in subscription
                await foreach (var diskResource in subscriptionResource.GetManagedDisksAsync(cancellationToken))
                {
                    var rgName = ExtractResourceGroupFromId(diskResource.Id.ToString());
                    disks.Add(ConvertToDiskModel(diskResource, rgName));
                }
            }

            return disks;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to list disks: {ex.Message}", ex);
        }
    }

    private static DiskInfo ConvertToDiskModel(ManagedDiskResource diskResource, string resourceGroup)
    {
        var disk = diskResource.Data;
        return new DiskInfo
        {
            Name = disk.Name,
            Id = disk.Id?.ToString(),
            ResourceGroup = resourceGroup,
            Location = disk.Location.ToString(),
            SkuName = disk.Sku?.Name.ToString(),
            SkuTier = disk.Sku?.Tier,
            DiskSizeGB = disk.DiskSizeGB,
            DiskState = disk.DiskState?.ToString(),
            TimeCreated = disk.TimeCreated,
            OSType = disk.OSType?.ToString(),
            ProvisioningState = disk.ProvisioningState,
            Tags = disk.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private static string ExtractResourceGroupFromId(string resourceId)
    {
        // Resource ID format: /subscriptions/{guid}/resourceGroups/{rgName}/providers/...
        var parts = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rgIndex = Array.IndexOf(parts, "resourceGroups");
        return rgIndex >= 0 && rgIndex + 1 < parts.Length ? parts[rgIndex + 1] : string.Empty;
    }

    public async Task<DiskInfo> CreateDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? source = null,
        string? location = null,
        int? sizeGb = null,
        string? sku = null,
        string? osType = null,
        string? zone = null,
        string? hyperVGeneration = null,
        int? maxShares = null,
        string? networkAccessPolicy = null,
        string? enableBursting = null,
        string? tags = null,
        string? diskEncryptionSet = null,
        string? encryptionType = null,
        string? diskAccessId = null,
        string? tier = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));
        var rgResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);

        // Default to the resource group's location if not specified
        var resolvedLocation = location ?? rgResource.Value.Data.Location.Name;

        var creationData = CreateDiskCreationData(source);

        var diskData = new ManagedDiskData(new Azure.Core.AzureLocation(resolvedLocation))
        {
            CreationData = creationData
        };

        if (sizeGb.HasValue)
        {
            diskData.DiskSizeGB = sizeGb.Value;
        }

        if (!string.IsNullOrEmpty(sku))
        {
            diskData.Sku = new DiskSku { Name = new DiskStorageAccountType(sku) };
        }

        if (!string.IsNullOrEmpty(osType))
        {
            if (osType.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                diskData.OSType = SupportedOperatingSystemType.Windows;
            }
            else if (osType.Equals("Linux", StringComparison.OrdinalIgnoreCase))
            {
                diskData.OSType = SupportedOperatingSystemType.Linux;
            }
            else
            {
                throw new ArgumentException($"Invalid OS type: {osType}. Accepted values: Linux, Windows.");
            }
        }

        if (!string.IsNullOrEmpty(zone))
        {
            diskData.Zones.Add(zone);
        }

        if (!string.IsNullOrEmpty(hyperVGeneration))
        {
            diskData.HyperVGeneration = new HyperVGeneration(hyperVGeneration);
        }

        if (maxShares.HasValue)
        {
            diskData.MaxShares = maxShares.Value;
        }

        if (!string.IsNullOrEmpty(networkAccessPolicy))
        {
            diskData.NetworkAccessPolicy = new Azure.ResourceManager.Compute.Models.NetworkAccessPolicy(networkAccessPolicy);
        }

        if (!string.IsNullOrEmpty(enableBursting))
        {
            diskData.BurstingEnabled = enableBursting.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrEmpty(tags))
        {
            foreach (var pair in tags.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    diskData.Tags[parts[0]] = parts[1];
                }
            }
        }

        if (!string.IsNullOrEmpty(diskEncryptionSet) || !string.IsNullOrEmpty(encryptionType))
        {
            diskData.Encryption ??= new DiskEncryption();
            if (!string.IsNullOrEmpty(diskEncryptionSet))
            {
                diskData.Encryption.DiskEncryptionSetId = new Azure.Core.ResourceIdentifier(diskEncryptionSet);
            }

            if (!string.IsNullOrEmpty(encryptionType))
            {
                diskData.Encryption.EncryptionType = new Azure.ResourceManager.Compute.Models.ComputeEncryptionType(encryptionType);
            }
        }

        if (!string.IsNullOrEmpty(diskAccessId))
        {
            diskData.DiskAccessId = new Azure.Core.ResourceIdentifier(diskAccessId);
        }

        if (!string.IsNullOrEmpty(tier))
        {
            diskData.Tier = tier;
        }

        _logger.LogInformation("Creating disk {DiskName} in resource group {ResourceGroup}", diskName, resourceGroup);

        var result = await rgResource.Value.GetManagedDisks()
            .CreateOrUpdateAsync(Azure.WaitUntil.Completed, diskName, diskData, cancellationToken);

        return ConvertToDiskModel(result.Value, resourceGroup);
    }

    public async Task<DiskInfo> UpdateDiskAsync(
        string diskName,
        string? resourceGroup,
        string subscription,
        int? sizeGb = null,
        string? sku = null,
        long? diskIopsReadWrite = null,
        long? diskMbpsReadWrite = null,
        int? maxShares = null,
        string? networkAccessPolicy = null,
        string? enableBursting = null,
        string? tags = null,
        string? diskEncryptionSet = null,
        string? encryptionType = null,
        string? diskAccessId = null,
        string? tier = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));
        var rgResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var diskResource = await rgResource.Value.GetManagedDisks().GetAsync(diskName, cancellationToken);

        var diskPatch = new ManagedDiskPatch();

        if (sizeGb.HasValue)
        {
            diskPatch.DiskSizeGB = sizeGb.Value;
        }

        if (!string.IsNullOrEmpty(sku))
        {
            diskPatch.Sku = new DiskSku { Name = new DiskStorageAccountType(sku) };
        }

        if (diskIopsReadWrite.HasValue)
        {
            diskPatch.DiskIopsReadWrite = diskIopsReadWrite.Value;
        }

        if (diskMbpsReadWrite.HasValue)
        {
            diskPatch.DiskMBpsReadWrite = diskMbpsReadWrite.Value;
        }

        if (maxShares.HasValue)
        {
            diskPatch.MaxShares = maxShares.Value;
        }

        if (!string.IsNullOrEmpty(networkAccessPolicy))
        {
            diskPatch.NetworkAccessPolicy = new Azure.ResourceManager.Compute.Models.NetworkAccessPolicy(networkAccessPolicy);
        }

        if (!string.IsNullOrEmpty(enableBursting))
        {
            diskPatch.BurstingEnabled = enableBursting.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrEmpty(tags))
        {
            diskPatch.Tags.Clear();
            foreach (var pair in tags.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    diskPatch.Tags[parts[0]] = parts[1];
                }
            }
        }

        if (!string.IsNullOrEmpty(diskEncryptionSet) || !string.IsNullOrEmpty(encryptionType))
        {
            diskPatch.Encryption ??= new DiskEncryption();
            if (!string.IsNullOrEmpty(diskEncryptionSet))
            {
                diskPatch.Encryption.DiskEncryptionSetId = new Azure.Core.ResourceIdentifier(diskEncryptionSet);
            }

            if (!string.IsNullOrEmpty(encryptionType))
            {
                diskPatch.Encryption.EncryptionType = new Azure.ResourceManager.Compute.Models.ComputeEncryptionType(encryptionType);
            }
        }

        if (!string.IsNullOrEmpty(diskAccessId))
        {
            diskPatch.DiskAccessId = new Azure.Core.ResourceIdentifier(diskAccessId);
        }

        if (!string.IsNullOrEmpty(tier))
        {
            diskPatch.Tier = tier;
        }

        _logger.LogInformation("Updating disk {DiskName} in resource group {ResourceGroup}", diskName, resourceGroup);

        var result = await diskResource.Value.UpdateAsync(Azure.WaitUntil.Completed, diskPatch, cancellationToken);

        return ConvertToDiskModel(result.Value, resourceGroup!);
    }

    private static DiskCreationData CreateDiskCreationData(string? source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return new DiskCreationData(DiskCreateOption.Empty);
        }

        // Blob URIs start with http:// or https://
        if (source.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return new DiskCreationData(DiskCreateOption.Import)
            {
                SourceUri = new Uri(source)
            };
        }

        // Otherwise treat as a resource ID (snapshot or managed disk)
        return new DiskCreationData(DiskCreateOption.Copy)
        {
            SourceResourceId = new Azure.Core.ResourceIdentifier(source)
        };
    }
}
