// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Utilities;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
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

    private static readonly Dictionary<string, WorkloadConfiguration> s_workloadConfigurations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["development"] = new WorkloadConfiguration(
            WorkloadType: "development",
            SuggestedVmSize: "Standard_B2s",
            SuggestedOsDiskType: "StandardSSD_LRS",
            SuggestedOsDiskSizeGb: 64,
            Description: "Cost-effective burstable VM for development and testing workloads",
            Requirements: VmRequirements.WindowsComputerName),
        ["web"] = new WorkloadConfiguration(
            WorkloadType: "web",
            SuggestedVmSize: "Standard_D2s_v5",
            SuggestedOsDiskType: "Premium_LRS",
            SuggestedOsDiskSizeGb: 128,
            Description: "General purpose VM optimized for web servers and small to medium applications",
            Requirements: VmRequirements.WindowsComputerName),
        ["database"] = new WorkloadConfiguration(
            WorkloadType: "database",
            SuggestedVmSize: "Standard_E4s_v5",
            SuggestedOsDiskType: "Premium_LRS",
            SuggestedOsDiskSizeGb: 256,
            Description: "Memory-optimized VM for database workloads with high memory-to-CPU ratio",
            Requirements: VmRequirements.WindowsComputerName),
        ["compute"] = new WorkloadConfiguration(
            WorkloadType: "compute",
            SuggestedVmSize: "Standard_F4s_v2",
            SuggestedOsDiskType: "Premium_LRS",
            SuggestedOsDiskSizeGb: 128,
            Description: "Compute-optimized VM for CPU-intensive workloads like batch processing and analytics",
            Requirements: VmRequirements.WindowsComputerName),
        ["memory"] = new WorkloadConfiguration(
            WorkloadType: "memory",
            SuggestedVmSize: "Standard_E8s_v5",
            SuggestedOsDiskType: "Premium_LRS",
            SuggestedOsDiskSizeGb: 256,
            Description: "High-memory VM for in-memory databases, caching, and memory-intensive applications",
            Requirements: VmRequirements.WindowsComputerName),
        ["gpu"] = new WorkloadConfiguration(
            WorkloadType: "gpu",
            SuggestedVmSize: "Standard_NC6s_v3",
            SuggestedOsDiskType: "Premium_LRS",
            SuggestedOsDiskSizeGb: 256,
            Description: "GPU-enabled VM for machine learning, rendering, and GPU-accelerated workloads",
            Requirements: VmRequirements.WindowsComputerName),
        ["general"] = new WorkloadConfiguration(
            WorkloadType: "general",
            SuggestedVmSize: "Standard_D4s_v5",
            SuggestedOsDiskType: "StandardSSD_LRS",
            SuggestedOsDiskSizeGb: 128,
            Description: "General purpose VM balanced for compute, memory, and storage",
            Requirements: VmRequirements.WindowsComputerName)
    };

    private static readonly Dictionary<string, (string Publisher, string Offer, string Sku, string Version)> s_imageAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ubuntu2404"] = ("Canonical", "ubuntu-24_04-lts", "server", "latest"),
        ["Ubuntu2204"] = ("Canonical", "0001-com-ubuntu-server-jammy", "22_04-lts-gen2", "latest"),
        ["Ubuntu2004"] = ("Canonical", "0001-com-ubuntu-server-focal", "20_04-lts-gen2", "latest"),
        ["Debian11"] = ("Debian", "debian-11", "11-gen2", "latest"),
        ["Debian12"] = ("Debian", "debian-12", "12-gen2", "latest"),
        ["RHEL9"] = ("RedHat", "RHEL", "9_0", "latest"),
        ["CentOS8"] = ("OpenLogic", "CentOS", "8_5-gen2", "latest"),
        ["Win2022Datacenter"] = ("MicrosoftWindowsServer", "WindowsServer", "2022-datacenter-g2", "latest"),
        ["Win2019Datacenter"] = ("MicrosoftWindowsServer", "WindowsServer", "2019-datacenter-gensecond", "latest"),
        ["Win11Pro"] = ("MicrosoftWindowsDesktop", "windows-11", "win11-22h2-pro", "latest"),
        ["Win10Pro"] = ("MicrosoftWindowsDesktop", "Windows-10", "win10-22h2-pro-g2", "latest")
    };

    public WorkloadConfiguration GetWorkloadConfiguration(string? workload)
    {
        if (string.IsNullOrEmpty(workload) || !s_workloadConfigurations.TryGetValue(workload, out var config))
        {
            return s_workloadConfigurations["general"];
        }
        return config;
    }

    public async Task<VmCreateResult> CreateVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string location,
        string adminUsername,
        string? vmSize = null,
        string? image = null,
        string? adminPassword = null,
        string? sshPublicKey = null,
        string? workload = null,
        string? osType = null,
        string? virtualNetwork = null,
        string? subnet = null,
        string? publicIpAddress = null,
        string? networkSecurityGroup = null,
        bool? noPublicIp = null,
        string? zone = null,
        int? osDiskSizeGb = null,
        string? osDiskType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resourceGroupResource = rgResource.Value;

        // Get workload configuration
        var workloadConfig = GetWorkloadConfiguration(workload);

        // Determine OS type
        var effectiveOsType = ComputeUtilities.DetermineOsType(osType, image);

        // Determine VM size based on workload or explicit parameter
        var effectiveVmSize = vmSize ?? workloadConfig.SuggestedVmSize;

        // Determine disk settings
        var effectiveOsDiskType = osDiskType ?? workloadConfig.SuggestedOsDiskType;
        // Only use explicit disk size if provided; otherwise let Azure use image's default size
        var effectiveOsDiskSizeGb = osDiskSizeGb;

        // Parse image
        var (publisher, offer, sku, version) = ParseImage(image);

        // Create or get network resources
        var nicId = await CreateOrGetNetworkResourcesAsync(
            resourceGroupResource,
            vmName,
            location,
            virtualNetwork,
            subnet,
            publicIpAddress,
            networkSecurityGroup,
            noPublicIp ?? false,
            effectiveOsType,
            cancellationToken);

        // Build VM data
        var vmData = new VirtualMachineData(new AzureLocation(location))
        {
            HardwareProfile = new VirtualMachineHardwareProfile
            {
                VmSize = new VirtualMachineSizeType(effectiveVmSize)
            },
            StorageProfile = new VirtualMachineStorageProfile
            {
                OSDisk = new VirtualMachineOSDisk(DiskCreateOptionType.FromImage)
                {
                    Name = $"{vmName}-osdisk",
                    Caching = CachingType.ReadWrite,
                    ManagedDisk = new VirtualMachineManagedDisk
                    {
                        StorageAccountType = new StorageAccountType(effectiveOsDiskType)
                    },
                    DiskSizeGB = effectiveOsDiskSizeGb
                },
                ImageReference = new ImageReference
                {
                    Publisher = publisher,
                    Offer = offer,
                    Sku = sku,
                    Version = version
                }
            },
            OSProfile = new VirtualMachineOSProfile
            {
                ComputerName = vmName,
                AdminUsername = adminUsername
            },
            NetworkProfile = new VirtualMachineNetworkProfile
            {
                NetworkInterfaces =
                {
                    new VirtualMachineNetworkInterfaceReference
                    {
                        Id = nicId,
                        Primary = true
                    }
                }
            }
        };

        // Configure authentication based on OS type
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase))
        {
            vmData.OSProfile.AdminPassword = adminPassword;
            vmData.OSProfile.WindowsConfiguration = new WindowsConfiguration
            {
                ProvisionVmAgent = true,
                EnableAutomaticUpdates = true
            };
        }
        else
        {
            // For Linux VMs, configure SSH key if provided, otherwise allow Azure AD SSH login
            vmData.OSProfile.LinuxConfiguration = new LinuxConfiguration
            {
                DisablePasswordAuthentication = string.IsNullOrEmpty(adminPassword)
            };

            // Only add SSH key if explicitly provided
            if (!string.IsNullOrEmpty(sshPublicKey))
            {
                // Check if it's a file path
                var resolvedSshKey = File.Exists(sshPublicKey)
                    ? File.ReadAllText(sshPublicKey).Trim()
                    : sshPublicKey;

                vmData.OSProfile.LinuxConfiguration.SshPublicKeys.Add(new SshPublicKeyConfiguration
                {
                    Path = $"/home/{adminUsername}/.ssh/authorized_keys",
                    KeyData = resolvedSshKey
                });
            }

            if (!string.IsNullOrEmpty(adminPassword))
            {
                vmData.OSProfile.AdminPassword = adminPassword;
                vmData.OSProfile.LinuxConfiguration.DisablePasswordAuthentication = false;
            }

            // Note: If neither SSH key nor password is provided, the VM will be created
            // and can be accessed via Azure AD SSH login: az ssh vm --resource-group <rg> --vm-name <name>
        }

        // Add availability zone if specified
        if (!string.IsNullOrEmpty(zone))
        {
            vmData.Zones.Add(zone);
        }

        // Create the VM
        var vmCollection = resourceGroupResource.GetVirtualMachines();
        var vmOperation = await vmCollection.CreateOrUpdateAsync(
            Azure.WaitUntil.Completed,
            vmName,
            vmData,
            cancellationToken);

        var createdVm = vmOperation.Value;

        // Get IP addresses
        var (publicIp, privateIp) = await GetVmIpAddressesAsync(
            resourceGroupResource,
            nicId,
            cancellationToken);

        return new VmCreateResult(
            Name: createdVm.Data.Name,
            Id: createdVm.Data.Id?.ToString(),
            Location: createdVm.Data.Location.Name,
            VmSize: createdVm.Data.HardwareProfile?.VmSize?.ToString(),
            ProvisioningState: createdVm.Data.ProvisioningState,
            OsType: effectiveOsType,
            PublicIpAddress: publicIp,
            PrivateIpAddress: privateIp,
            Zones: createdVm.Data.Zones?.ToList(),
            Tags: createdVm.Data.Tags as IReadOnlyDictionary<string, string>,
            WorkloadConfiguration: workloadConfig);
    }

    private static (string Publisher, string Offer, string Sku, string Version) ParseImage(string? image)
    {
        // Default to Ubuntu 24.04 LTS
        if (string.IsNullOrEmpty(image))
        {
            return s_imageAliases["Ubuntu2404"];
        }

        // Check if it's an alias
        if (s_imageAliases.TryGetValue(image, out var aliasConfig))
        {
            return aliasConfig;
        }

        // Try to parse as URN (publisher:offer:sku:version)
        var parts = image.Split(':');
        if (parts.Length == 4)
        {
            return (parts[0], parts[1], parts[2], parts[3]);
        }

        // Default fallback
        return s_imageAliases["Ubuntu2404"];
    }

    private async Task<ResourceIdentifier> CreateOrGetNetworkResourcesAsync(
        ResourceGroupResource resourceGroup,
        string vmName,
        string location,
        string? virtualNetwork,
        string? subnet,
        string? publicIpAddress,
        string? networkSecurityGroup,
        bool noPublicIp,
        string osType,
        CancellationToken cancellationToken)
    {
        var vnetName = virtualNetwork ?? $"{vmName}-vnet";
        var subnetName = subnet ?? "default";
        var nsgName = networkSecurityGroup ?? $"{vmName}-nsg";
        var nicName = $"{vmName}-nic";

        // Create or get NSG
        var nsgCollection = resourceGroup.GetNetworkSecurityGroups();
        NetworkSecurityGroupResource nsgResource;

        try
        {
            var existingNsg = await nsgCollection.GetAsync(nsgName, cancellationToken: cancellationToken);
            nsgResource = existingNsg.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var nsgData = new NetworkSecurityGroupData
            {
                Location = new AzureLocation(location)
            };

            // Add appropriate security rule based on OS type
            // WARNING: These rules allow access from any source IP for quick-start scenarios.
            // For production use, restrict SourceAddressPrefix to specific IP ranges.
            var isWindows = osType.Equals("Windows", StringComparison.OrdinalIgnoreCase);

            if (isWindows)
            {
                _logger.LogWarning("Creating NSG with RDP (port 3389) open to all sources. For production, restrict the source IP range.");
                nsgData.SecurityRules.Add(new SecurityRuleData
                {
                    Name = "AllowRDP",
                    Priority = 1000,
                    Access = SecurityRuleAccess.Allow,
                    Direction = SecurityRuleDirection.Inbound,
                    Protocol = SecurityRuleProtocol.Tcp,
                    SourceAddressPrefix = "*",
                    SourcePortRange = "*",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "3389"
                });
            }
            else
            {
                _logger.LogWarning("Creating NSG with SSH (port 22) open to all sources. For production, restrict the source IP range.");
                nsgData.SecurityRules.Add(new SecurityRuleData
                {
                    Name = "AllowSSH",
                    Priority = 1000,
                    Access = SecurityRuleAccess.Allow,
                    Direction = SecurityRuleDirection.Inbound,
                    Protocol = SecurityRuleProtocol.Tcp,
                    SourceAddressPrefix = "*",
                    SourcePortRange = "*",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "22"
                });
            }

            var nsgOperation = await nsgCollection.CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                nsgName,
                nsgData,
                cancellationToken);
            nsgResource = nsgOperation.Value;
        }

        // Create or get VNet
        var vnetCollection = resourceGroup.GetVirtualNetworks();
        VirtualNetworkResource vnetResource;

        try
        {
            var existingVnet = await vnetCollection.GetAsync(vnetName, cancellationToken: cancellationToken);
            vnetResource = existingVnet.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var vnetData = new VirtualNetworkData
            {
                Location = new AzureLocation(location)
            };
            vnetData.AddressPrefixes.Add("10.0.0.0/16");
            vnetData.Subnets.Add(new SubnetData
            {
                Name = subnetName,
                AddressPrefix = "10.0.0.0/24",
                NetworkSecurityGroup = new NetworkSecurityGroupData { Id = nsgResource.Id }
            });

            var vnetOperation = await vnetCollection.CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                vnetName,
                vnetData,
                cancellationToken);
            vnetResource = vnetOperation.Value;
        }

        // Get subnet
        var subnetCollection = vnetResource.GetSubnets();
        var subnetResource = await subnetCollection.GetAsync(subnetName, cancellationToken: cancellationToken);

        // Create public IP if needed
        PublicIPAddressResource? publicIpResource = null;
        if (!noPublicIp)
        {
            var pipName = publicIpAddress ?? $"{vmName}-pip";
            var pipCollection = resourceGroup.GetPublicIPAddresses();

            try
            {
                var existingPip = await pipCollection.GetAsync(pipName, cancellationToken: cancellationToken);
                publicIpResource = existingPip.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var pipData = new PublicIPAddressData
                {
                    Location = new AzureLocation(location),
                    PublicIPAllocationMethod = NetworkIPAllocationMethod.Static,
                    Sku = new PublicIPAddressSku
                    {
                        Name = PublicIPAddressSkuName.Standard
                    }
                };

                var pipOperation = await pipCollection.CreateOrUpdateAsync(
                    Azure.WaitUntil.Completed,
                    pipName,
                    pipData,
                    cancellationToken);
                publicIpResource = pipOperation.Value;
            }
        }

        // Create NIC
        var nicCollection = resourceGroup.GetNetworkInterfaces();
        var nicData = new NetworkInterfaceData
        {
            Location = new AzureLocation(location)
        };

        var ipConfig = new NetworkInterfaceIPConfigurationData
        {
            Name = "ipconfig1",
            Primary = true,
            PrivateIPAllocationMethod = NetworkIPAllocationMethod.Dynamic,
            Subnet = new SubnetData { Id = subnetResource.Value.Id }
        };

        if (publicIpResource != null)
        {
            ipConfig.PublicIPAddress = new PublicIPAddressData { Id = publicIpResource.Id };
        }

        nicData.IPConfigurations.Add(ipConfig);

        var nicOperation = await nicCollection.CreateOrUpdateAsync(
            Azure.WaitUntil.Completed,
            nicName,
            nicData,
            cancellationToken);

        return nicOperation.Value.Id;
    }

    private static async Task<(string? PublicIp, string? PrivateIp)> GetVmIpAddressesAsync(
        ResourceGroupResource resourceGroup,
        ResourceIdentifier nicId,
        CancellationToken cancellationToken)
    {
        var nicName = nicId.Name;
        var nicCollection = resourceGroup.GetNetworkInterfaces();
        var nicResponse = await nicCollection.GetAsync(nicName, cancellationToken: cancellationToken);
        var nic = nicResponse.Value;

        string? privateIp = null;
        string? publicIp = null;

        foreach (var ipConfig in nic.Data.IPConfigurations)
        {
            privateIp ??= ipConfig.PrivateIPAddress;

            var publicIpId = ipConfig.PublicIPAddress?.Id;
            if (publicIpId is not null)
            {
                var pipName = publicIpId.Name;
                var pipCollection = resourceGroup.GetPublicIPAddresses();
                var pipResponse = await pipCollection.GetAsync(pipName, cancellationToken: cancellationToken);
                publicIp = pipResponse.Value.Data.IPAddress;
            }
        }

        return (publicIp, privateIp);
    }

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

    public async Task<VmssCreateResult> CreateVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string location,
        string adminUsername,
        string? vmSize = null,
        string? image = null,
        string? adminPassword = null,
        string? sshPublicKey = null,
        string? workload = null,
        string? osType = null,
        string? virtualNetwork = null,
        string? subnet = null,
        int? instanceCount = null,
        string? upgradePolicy = null,
        string? zone = null,
        int? osDiskSizeGb = null,
        string? osDiskType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resourceGroupResource = rgResource.Value;

        // Get workload configuration
        var workloadConfig = GetWorkloadConfiguration(workload);

        // Determine OS type
        var effectiveOsType = ComputeUtilities.DetermineOsType(osType, image);

        // Determine VM size based on workload or explicit parameter
        var effectiveVmSize = vmSize ?? workloadConfig.SuggestedVmSize;

        // Determine disk settings
        var effectiveOsDiskType = osDiskType ?? workloadConfig.SuggestedOsDiskType;
        var effectiveOsDiskSizeGb = osDiskSizeGb;
        var effectiveInstanceCount = instanceCount ?? 2;
        var effectiveUpgradePolicy = ParseUpgradePolicy(upgradePolicy);

        // Parse image
        var (publisher, offer, sku, version) = ParseImage(image);

        // Create or get network resources for VMSS
        var subnetId = await CreateOrGetVmssNetworkResourcesAsync(
            resourceGroupResource,
            vmssName,
            location,
            virtualNetwork,
            subnet,
            cancellationToken);

        // Build VMSS data using Flexible orchestration mode (default since Nov 2023)
        var vmssData = new VirtualMachineScaleSetData(new AzureLocation(location))
        {
            Sku = new ComputeSku
            {
                Name = effectiveVmSize,
                Tier = "Standard",
                Capacity = effectiveInstanceCount
            },
            UpgradePolicy = new VirtualMachineScaleSetUpgradePolicy
            {
                Mode = effectiveUpgradePolicy
            },
            Overprovision = false,
            VirtualMachineProfile = new VirtualMachineScaleSetVmProfile
            {
                StorageProfile = new VirtualMachineScaleSetStorageProfile
                {
                    OSDisk = new VirtualMachineScaleSetOSDisk(DiskCreateOptionType.FromImage)
                    {
                        Caching = CachingType.ReadWrite,
                        ManagedDisk = new VirtualMachineScaleSetManagedDisk
                        {
                            StorageAccountType = new StorageAccountType(effectiveOsDiskType)
                        },
                        DiskSizeGB = effectiveOsDiskSizeGb
                    },
                    ImageReference = new ImageReference
                    {
                        Publisher = publisher,
                        Offer = offer,
                        Sku = sku,
                        Version = version
                    }
                },
                OSProfile = new VirtualMachineScaleSetOSProfile
                {
                    // VMSS computer name prefix - Azure appends instance number
                    ComputerNamePrefix = vmssName.Length > 9 ? vmssName[..9] : vmssName,
                    AdminUsername = adminUsername
                },
                NetworkProfile = new VirtualMachineScaleSetNetworkProfile
                {
                    NetworkInterfaceConfigurations =
                    {
                        new VirtualMachineScaleSetNetworkConfiguration($"{vmssName}-nic")
                        {
                            Primary = true,
                            IPConfigurations =
                            {
                                new VirtualMachineScaleSetIPConfiguration($"{vmssName}-ipconfig")
                                {
                                    Primary = true,
                                    SubnetId = subnetId
                                }
                            }
                        }
                    }
                }
            }
        };

        // Configure authentication based on OS type
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase))
        {
            vmssData.VirtualMachineProfile.OSProfile.AdminPassword = adminPassword;
            vmssData.VirtualMachineProfile.OSProfile.WindowsConfiguration = new WindowsConfiguration
            {
                ProvisionVmAgent = true,
                EnableAutomaticUpdates = true
            };
        }
        else
        {
            vmssData.VirtualMachineProfile.OSProfile.LinuxConfiguration = new LinuxConfiguration
            {
                DisablePasswordAuthentication = string.IsNullOrEmpty(adminPassword)
            };

            if (!string.IsNullOrEmpty(sshPublicKey))
            {
                var resolvedSshKey = File.Exists(sshPublicKey)
                    ? File.ReadAllText(sshPublicKey).Trim()
                    : sshPublicKey;

                vmssData.VirtualMachineProfile.OSProfile.LinuxConfiguration.SshPublicKeys.Add(new SshPublicKeyConfiguration
                {
                    Path = $"/home/{adminUsername}/.ssh/authorized_keys",
                    KeyData = resolvedSshKey
                });
            }

            if (!string.IsNullOrEmpty(adminPassword))
            {
                vmssData.VirtualMachineProfile.OSProfile.AdminPassword = adminPassword;
                vmssData.VirtualMachineProfile.OSProfile.LinuxConfiguration.DisablePasswordAuthentication = false;
            }
        }

        // Add availability zone if specified
        if (!string.IsNullOrEmpty(zone))
        {
            vmssData.Zones.Add(zone);
        }

        // Create the VMSS
        var vmssCollection = resourceGroupResource.GetVirtualMachineScaleSets();
        var vmssOperation = await vmssCollection.CreateOrUpdateAsync(
            Azure.WaitUntil.Completed,
            vmssName,
            vmssData,
            cancellationToken);

        var createdVmss = vmssOperation.Value;

        return new VmssCreateResult(
            Name: createdVmss.Data.Name,
            Id: createdVmss.Data.Id?.ToString(),
            Location: createdVmss.Data.Location.Name,
            VmSize: createdVmss.Data.Sku?.Name,
            ProvisioningState: createdVmss.Data.ProvisioningState,
            OsType: effectiveOsType,
            Capacity: (int)(createdVmss.Data.Sku?.Capacity ?? effectiveInstanceCount),
            UpgradePolicy: createdVmss.Data.UpgradePolicy?.Mode?.ToString(),
            Zones: createdVmss.Data.Zones?.ToList(),
            Tags: createdVmss.Data.Tags as IReadOnlyDictionary<string, string>,
            WorkloadConfiguration: workloadConfig);
    }

    public async Task<VmssUpdateResult> UpdateVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string? vmSize = null,
        int? capacity = null,
        string? upgradePolicy = null,
        bool? overprovision = null,
        bool? enableAutoOsUpgrade = null,
        string? scaleInPolicy = null,
        string? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resourceGroupResource = rgResource.Value;

        // Get existing VMSS
        var vmssCollection = resourceGroupResource.GetVirtualMachineScaleSets();
        var vmssResponse = await vmssCollection.GetAsync(vmssName, cancellationToken: cancellationToken);
        var vmssResource = vmssResponse.Value;
        var vmssData = vmssResource.Data;

        // Apply updates using PATCH semantics - only update what's specified
        var needsUpdate = false;

        if (vmSize != null && vmssData.Sku != null)
        {
            vmssData.Sku.Name = vmSize;
            needsUpdate = true;
        }

        if (capacity.HasValue && vmssData.Sku != null)
        {
            vmssData.Sku.Capacity = capacity.Value;
            needsUpdate = true;
        }

        if (upgradePolicy != null)
        {
            vmssData.UpgradePolicy ??= new VirtualMachineScaleSetUpgradePolicy();
            vmssData.UpgradePolicy.Mode = ParseUpgradePolicy(upgradePolicy);
            needsUpdate = true;
        }

        if (overprovision.HasValue)
        {
            vmssData.Overprovision = overprovision.Value;
            needsUpdate = true;
        }

        if (enableAutoOsUpgrade.HasValue)
        {
            vmssData.UpgradePolicy ??= new VirtualMachineScaleSetUpgradePolicy();
            vmssData.UpgradePolicy.AutomaticOSUpgradePolicy ??= new AutomaticOSUpgradePolicy();
            vmssData.UpgradePolicy.AutomaticOSUpgradePolicy.EnableAutomaticOSUpgrade = enableAutoOsUpgrade.Value;
            needsUpdate = true;
        }

        if (scaleInPolicy != null)
        {
            vmssData.ScaleInPolicy ??= new ScaleInPolicy();
            vmssData.ScaleInPolicy.Rules.Clear();
            vmssData.ScaleInPolicy.Rules.Add(ParseScaleInPolicy(scaleInPolicy));
            needsUpdate = true;
        }

        if (tags != null)
        {
            // Parse tags in key=value,key2=value2 format
            var tagPairs = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in tagPairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    vmssData.Tags[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            var updateOperation = await vmssCollection.CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                vmssName,
                vmssData,
                cancellationToken);
            vmssResource = updateOperation.Value;
        }

        return new VmssUpdateResult(
            Name: vmssResource.Data.Name,
            Id: vmssResource.Data.Id?.ToString(),
            Location: vmssResource.Data.Location.Name,
            VmSize: vmssResource.Data.Sku?.Name,
            ProvisioningState: vmssResource.Data.ProvisioningState,
            Capacity: (int?)(vmssResource.Data.Sku?.Capacity),
            UpgradePolicy: vmssResource.Data.UpgradePolicy?.Mode?.ToString(),
            Zones: vmssResource.Data.Zones?.ToList(),
            Tags: vmssResource.Data.Tags as IReadOnlyDictionary<string, string>);
    }

    public async Task<VmUpdateResult> UpdateVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? vmSize = null,
        string? tags = null,
        string? licenseType = null,
        string? bootDiagnostics = null,
        string? userData = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resourceGroupResource = rgResource.Value;

        // Get existing VM
        var vmCollection = resourceGroupResource.GetVirtualMachines();
        var vmResponse = await vmCollection.GetAsync(vmName, cancellationToken: cancellationToken);
        var vmResource = vmResponse.Value;

        // Build patch object - only update what's specified
        var patch = new VirtualMachinePatch();
        var needsUpdate = false;

        if (vmSize != null)
        {
            patch.HardwareProfile = new VirtualMachineHardwareProfile { VmSize = new VirtualMachineSizeType(vmSize) };
            needsUpdate = true;
        }

        if (licenseType != null)
        {
            patch.LicenseType = licenseType.Equals("None", StringComparison.OrdinalIgnoreCase) ? null : licenseType;
            needsUpdate = true;
        }

        if (bootDiagnostics != null)
        {
            var enabled = bootDiagnostics.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                          bootDiagnostics.Equals("enable", StringComparison.OrdinalIgnoreCase);
            patch.BootDiagnostics = new BootDiagnostics { Enabled = enabled };
            needsUpdate = true;
        }

        if (userData != null)
        {
            patch.UserData = userData;
            needsUpdate = true;
        }

        if (tags != null)
        {
            // Parse tags in key=value,key2=value2 format
            var tagPairs = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in tagPairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    patch.Tags[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            var updateOperation = await vmResource.UpdateAsync(
                Azure.WaitUntil.Completed,
                patch,
                cancellationToken: cancellationToken);
            vmResource = updateOperation.Value;
        }

        // Extract power state from instance view if available
        string? powerState = null;
        try
        {
            var instanceViewResponse = await vmResource.InstanceViewAsync(cancellationToken);
            var instanceView = instanceViewResponse.Value;
            powerState = instanceView.Statuses?
                .FirstOrDefault(s => s.Code?.StartsWith("PowerState/", StringComparison.OrdinalIgnoreCase) == true)?
                .DisplayStatus;
        }
        catch
        {
            // Instance view not always available
        }

        return new VmUpdateResult(
            Name: vmResource.Data.Name,
            Id: vmResource.Data.Id?.ToString(),
            Location: vmResource.Data.Location.Name,
            VmSize: vmResource.Data.HardwareProfile?.VmSize?.ToString(),
            ProvisioningState: vmResource.Data.ProvisioningState,
            PowerState: powerState,
            OsType: vmResource.Data.StorageProfile?.OSDisk?.OSType?.ToString(),
            LicenseType: vmResource.Data.LicenseType,
            Zones: vmResource.Data.Zones?.ToList(),
            Tags: vmResource.Data.Tags as IReadOnlyDictionary<string, string>);
    }

    private static VirtualMachineScaleSetScaleInRule ParseScaleInPolicy(string scaleInPolicy)
    {
        return scaleInPolicy.ToLowerInvariant() switch
        {
            "default" => VirtualMachineScaleSetScaleInRule.Default,
            "oldestvm" => VirtualMachineScaleSetScaleInRule.OldestVm,
            "newestvm" => VirtualMachineScaleSetScaleInRule.NewestVm,
            _ => VirtualMachineScaleSetScaleInRule.Default
        };
    }

    private static VirtualMachineScaleSetUpgradeMode ParseUpgradePolicy(string? upgradePolicy)
    {
        if (string.IsNullOrEmpty(upgradePolicy))
        {
            return VirtualMachineScaleSetUpgradeMode.Manual;
        }

        return upgradePolicy.ToLowerInvariant() switch
        {
            "automatic" => VirtualMachineScaleSetUpgradeMode.Automatic,
            "rolling" => VirtualMachineScaleSetUpgradeMode.Rolling,
            _ => VirtualMachineScaleSetUpgradeMode.Manual
        };
    }

    private async Task<ResourceIdentifier> CreateOrGetVmssNetworkResourcesAsync(
        ResourceGroupResource resourceGroup,
        string vmssName,
        string location,
        string? virtualNetwork,
        string? subnet,
        CancellationToken cancellationToken)
    {
        var vnetName = virtualNetwork ?? $"{vmssName}-vnet";
        var subnetName = subnet ?? "default";

        // Create or get VNet
        var vnetCollection = resourceGroup.GetVirtualNetworks();
        VirtualNetworkResource vnetResource;

        try
        {
            var existingVnet = await vnetCollection.GetAsync(vnetName, cancellationToken: cancellationToken);
            vnetResource = existingVnet.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var vnetData = new VirtualNetworkData
            {
                Location = new AzureLocation(location),
                AddressPrefixes = { "10.0.0.0/16" },
                Subnets =
                {
                    new SubnetData
                    {
                        Name = subnetName,
                        AddressPrefix = "10.0.0.0/24"
                    }
                }
            };

            var vnetOperation = await vnetCollection.CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                vnetName,
                vnetData,
                cancellationToken);
            vnetResource = vnetOperation.Value;
        }

        // Get subnet
        var subnetCollection = vnetResource.GetSubnets();
        SubnetResource subnetResource;

        try
        {
            var existingSubnet = await subnetCollection.GetAsync(subnetName, cancellationToken: cancellationToken);
            subnetResource = existingSubnet.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var subnetData = new SubnetData
            {
                AddressPrefix = "10.0.1.0/24"
            };

            var subnetOperation = await subnetCollection.CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                subnetName,
                subnetData,
                cancellationToken);
            subnetResource = subnetOperation.Value;
        }

        return subnetResource.Id;
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
}
