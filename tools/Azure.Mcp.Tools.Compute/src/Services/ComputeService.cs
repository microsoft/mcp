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

public sealed class ComputeService(
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

    public async Task<List<VmSizeInfo>> ListVmSizesAsync(
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var sizes = new List<VmSizeInfo>();
        await foreach (var size in subscriptionResource.GetVirtualMachineSizesAsync(location, cancellationToken: cancellationToken))
        {
            sizes.Add(MapToVmSizeInfo(size));
        }

        return sizes;
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

    public async Task<VmssRollingUpgradeStatus> GetVmssRollingUpgradeStatusAsync(
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

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var vmssResource = await rgResource.Value
            .GetVirtualMachineScaleSets()
            .GetAsync(vmssName, cancellationToken: cancellationToken);

        var upgradeStatus = await vmssResource.Value
            .GetVirtualMachineScaleSetRollingUpgrade()
            .GetAsync(cancellationToken);

        return MapToVmssRollingUpgradeStatus(vmssName, upgradeStatus.Value.Data);
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

    private static VmSizeInfo MapToVmSizeInfo(VirtualMachineSize data)
    {
        return new VmSizeInfo(
            Name: data.Name,
            NumberOfCores: data.NumberOfCores,
            MemoryInMB: data.MemoryInMB,
            MaxDataDiskCount: data.MaxDataDiskCount,
            OsDiskSizeInMB: data.OSDiskSizeInMB,
            ResourceDiskSizeInMB: data.ResourceDiskSizeInMB
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

    private static VmssRollingUpgradeStatus MapToVmssRollingUpgradeStatus(string vmssName, VirtualMachineScaleSetRollingUpgradeData data)
    {
        return new VmssRollingUpgradeStatus(
            Name: vmssName,
            Policy: data.Policy != null ? new UpgradePolicyInfo(
                Mode: null,
                MaxBatchInstancePercent: data.Policy.MaxBatchInstancePercent,
                MaxUnhealthyInstancePercent: data.Policy.MaxUnhealthyInstancePercent,
                MaxUnhealthyUpgradedInstancePercent: data.Policy.MaxUnhealthyUpgradedInstancePercent,
                PauseTimeBetweenBatches: data.Policy.PauseTimeBetweenBatches?.ToString()
            ) : null,
            RunningStatus: data.RunningStatus != null ? new Models.RollingUpgradeRunningStatus(
                Code: data.RunningStatus.Code?.ToString(),
                StartTime: data.RunningStatus.StartOn,  // Changed from StartTime
                LastAction: data.RunningStatus.LastAction?.ToString(),
                LastActionTime: data.RunningStatus.LastActionOn  // Changed from LastActionTime
            ) : null,
            Progress: data.Progress != null ? new Models.RollingUpgradeProgressInfo(
                SuccessfulInstanceCount: data.Progress.SuccessfulInstanceCount,
                FailedInstanceCount: data.Progress.FailedInstanceCount,
                InProgressInstanceCount: data.Progress.InProgressInstanceCount,
                PendingInstanceCount: data.Progress.PendingInstanceCount
            ) : null,
            Error: data.Error?.Message
        );
    }
}
