// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Utilities;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Compute.Services;

public class ComputeService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<ComputeService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IComputeService
{
    private readonly ILogger<ComputeService> _logger = logger;

    // Default VM size matching Azure CLI (az vm create --size default)
    private const string DefaultVmSize = "Standard_DS1_v2";

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
        string? osType = null,
        string? virtualNetwork = null,
        string? subnet = null,
        string? publicIpAddress = null,
        string? networkSecurityGroup = null,
        bool? noPublicIp = null,
        string? sourceAddressPrefix = null,
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

        // Determine OS type
        var effectiveOsType = ComputeUtilities.DetermineOsType(osType, image);

        // Use Azure CLI default VM size (Standard_DS1_v2) when not specified
        var effectiveVmSize = vmSize ?? DefaultVmSize;

        // Determine disk settings - let Azure choose disk type based on VM size when not specified
        var effectiveOsDiskType = osDiskType;
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
            sourceAddressPrefix,
            cancellationToken);

        // Build VM data
        var vmData = new VirtualMachineData(new(location))
        {
            HardwareProfile = new()
            {
                VmSize = new(effectiveVmSize)
            },
            StorageProfile = new()
            {
                OSDisk = new(DiskCreateOptionType.FromImage)
                {
                    Name = $"{vmName}-osdisk",
                    Caching = CachingType.ReadWrite,
                    ManagedDisk = new(),
                    DiskSizeGB = effectiveOsDiskSizeGb
                },
                ImageReference = new()
                {
                    Publisher = publisher,
                    Offer = offer,
                    Sku = sku,
                    Version = version
                }
            },
            OSProfile = new()
            {
                ComputerName = vmName,
                AdminUsername = adminUsername
            },
            NetworkProfile = new()
            {
                NetworkInterfaces =
                {
                    new()
                    {
                        Id = nicId,
                        Primary = true
                    }
                }
            }
        };

        // Set disk type only if explicitly specified; otherwise let Azure choose based on VM size
        if (effectiveOsDiskType != null)
        {
            vmData.StorageProfile.OSDisk.ManagedDisk.StorageAccountType = new(effectiveOsDiskType);
        }

        // Configure authentication based on OS type
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase))
        {
            vmData.OSProfile.AdminPassword = adminPassword;
            vmData.OSProfile.WindowsConfiguration = new()
            {
                ProvisionVmAgent = true,
                EnableAutomaticUpdates = true
            };
        }
        else
        {
            // For Linux VMs, configure SSH key if provided, otherwise allow Azure AD SSH login
            vmData.OSProfile.LinuxConfiguration = new()
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

                vmData.OSProfile.LinuxConfiguration.SshPublicKeys.Add(new()
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
            WaitUntil.Completed,
            vmName,
            vmData,
            cancellationToken);

        var createdVm = vmOperation.Value;

        // Get IP addresses
        var (publicIp, privateIp) = await GetVmIpAddressesAsync(
            resourceGroupResource,
            nicId,
            cancellationToken);

        return new(
            Name: createdVm.Data.Name,
            Id: createdVm.Data.Id?.ToString(),
            Location: createdVm.Data.Location.Name,
            VmSize: createdVm.Data.HardwareProfile?.VmSize?.ToString(),
            ProvisioningState: createdVm.Data.ProvisioningState,
            OsType: effectiveOsType,
            PublicIpAddress: publicIp,
            PrivateIpAddress: privateIp,
            Zones: createdVm.Data.Zones?.ToList(),
            Tags: createdVm.Data.Tags as IReadOnlyDictionary<string, string>);
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
        string? sourceAddressPrefix,
        CancellationToken cancellationToken)
    {
        var effectiveSourceAddressPrefix = sourceAddressPrefix ?? "*";
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
                Location = new(location)
            };

            // Add appropriate security rule based on OS type
            // WARNING: These rules allow access from any source IP for quick-start scenarios.
            // For production use, restrict SourceAddressPrefix to specific IP ranges.
            var isWindows = osType.Equals("Windows", StringComparison.OrdinalIgnoreCase);

            if (isWindows)
            {
                if (effectiveSourceAddressPrefix == "*")
                {
                    _logger.LogWarning("Creating NSG with RDP (port 3389) open to all sources. For production, restrict the source IP range using --source-address-prefix.");
                }

                nsgData.SecurityRules.Add(new()
                {
                    Name = "AllowRDP",
                    Priority = 1000,
                    Access = SecurityRuleAccess.Allow,
                    Direction = SecurityRuleDirection.Inbound,
                    Protocol = SecurityRuleProtocol.Tcp,
                    SourceAddressPrefix = effectiveSourceAddressPrefix,
                    SourcePortRange = "*",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "3389"
                });
            }
            else
            {
                if (effectiveSourceAddressPrefix == "*")
                {
                    _logger.LogWarning("Creating NSG with SSH (port 22) open to all sources. For production, restrict the source IP range using --source-address-prefix.");
                }

                nsgData.SecurityRules.Add(new()
                {
                    Name = "AllowSSH",
                    Priority = 1000,
                    Access = SecurityRuleAccess.Allow,
                    Direction = SecurityRuleDirection.Inbound,
                    Protocol = SecurityRuleProtocol.Tcp,
                    SourceAddressPrefix = effectiveSourceAddressPrefix,
                    SourcePortRange = "*",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "22"
                });
            }

            var nsgOperation = await nsgCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
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
                Location = new(location)
            };
            vnetData.AddressPrefixes.Add("10.0.0.0/16");
            vnetData.Subnets.Add(new()
            {
                Name = subnetName,
                AddressPrefix = "10.0.0.0/24",
                NetworkSecurityGroup = new() { Id = nsgResource.Id }
            });

            var vnetOperation = await vnetCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
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
                    Location = new(location),
                    PublicIPAllocationMethod = NetworkIPAllocationMethod.Static,
                    Sku = new()
                    {
                        Name = PublicIPAddressSkuName.Standard
                    }
                };

                var pipOperation = await pipCollection.CreateOrUpdateAsync(
                    WaitUntil.Completed,
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
            Location = new(location)
        };

        var ipConfig = new NetworkInterfaceIPConfigurationData
        {
            Name = "ipconfig1",
            Primary = true,
            PrivateIPAllocationMethod = NetworkIPAllocationMethod.Dynamic,
            Subnet = new() { Id = subnetResource.Value.Id }
        };

        if (publicIpResource != null)
        {
            ipConfig.PublicIPAddress = new() { Id = publicIpResource.Id };
        }

        nicData.IPConfigurations.Add(ipConfig);

        var nicOperation = await nicCollection.CreateOrUpdateAsync(
            WaitUntil.Completed,
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

        // Determine OS type
        var effectiveOsType = ComputeUtilities.DetermineOsType(osType, image);

        // Use Azure CLI default VM size (Standard_DS1_v2) when not specified
        var effectiveVmSize = vmSize ?? DefaultVmSize;

        // Determine disk settings - let Azure choose disk type based on VM size when not specified
        var effectiveOsDiskType = osDiskType;
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
        var vmssData = new VirtualMachineScaleSetData(new(location))
        {
            Sku = new()
            {
                Name = effectiveVmSize,
                Tier = "Standard",
                Capacity = effectiveInstanceCount
            },
            UpgradePolicy = new()
            {
                Mode = effectiveUpgradePolicy
            },
            Overprovision = false,
            VirtualMachineProfile = new()
            {
                StorageProfile = new()
                {
                    OSDisk = new(DiskCreateOptionType.FromImage)
                    {
                        Caching = CachingType.ReadWrite,
                        ManagedDisk = new(),
                        DiskSizeGB = effectiveOsDiskSizeGb
                    },
                    ImageReference = new()
                    {
                        Publisher = publisher,
                        Offer = offer,
                        Sku = sku,
                        Version = version
                    }
                },
                OSProfile = new()
                {
                    // VMSS computer name prefix - Azure appends instance number
                    ComputerNamePrefix = vmssName.Length > 9 ? vmssName[..9] : vmssName,
                    AdminUsername = adminUsername
                },
                NetworkProfile = new()
                {
                    NetworkInterfaceConfigurations =
                    {
                        new($"{vmssName}-nic")
                        {
                            Primary = true,
                            IPConfigurations =
                            {
                                new($"{vmssName}-ipconfig")
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

        // Set disk type only if explicitly specified; otherwise let Azure choose based on VM size
        if (effectiveOsDiskType != null)
        {
            vmssData.VirtualMachineProfile.StorageProfile.OSDisk.ManagedDisk.StorageAccountType = new(effectiveOsDiskType);
        }

        // Configure authentication based on OS type
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase))
        {
            vmssData.VirtualMachineProfile.OSProfile.AdminPassword = adminPassword;
            vmssData.VirtualMachineProfile.OSProfile.WindowsConfiguration = new()
            {
                ProvisionVmAgent = true,
                EnableAutomaticUpdates = true
            };
        }
        else
        {
            vmssData.VirtualMachineProfile.OSProfile.LinuxConfiguration = new()
            {
                DisablePasswordAuthentication = string.IsNullOrEmpty(adminPassword)
            };

            if (!string.IsNullOrEmpty(sshPublicKey))
            {
                var resolvedSshKey = File.Exists(sshPublicKey)
                    ? File.ReadAllText(sshPublicKey).Trim()
                    : sshPublicKey;

                vmssData.VirtualMachineProfile.OSProfile.LinuxConfiguration.SshPublicKeys.Add(new()
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
            WaitUntil.Completed,
            vmssName,
            vmssData,
            cancellationToken);

        var createdVmss = vmssOperation.Value;

        return new(
            Name: createdVmss.Data.Name,
            Id: createdVmss.Data.Id?.ToString(),
            Location: createdVmss.Data.Location.Name,
            VmSize: createdVmss.Data.Sku?.Name,
            ProvisioningState: createdVmss.Data.ProvisioningState,
            OsType: effectiveOsType,
            Capacity: (int)(createdVmss.Data.Sku?.Capacity ?? effectiveInstanceCount),
            UpgradePolicy: createdVmss.Data.UpgradePolicy?.Mode?.ToString(),
            Zones: createdVmss.Data.Zones?.ToList(),
            Tags: createdVmss.Data.Tags as IReadOnlyDictionary<string, string>);
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

        // Build PATCH payload - only include what's specified
        var patch = new VirtualMachineScaleSetPatch();
        var needsUpdate = false;

        if (vmSize != null || capacity.HasValue)
        {
            patch.Sku = new()
            {
                Name = vmSize ?? vmssResource.Data.Sku?.Name,
                Tier = vmssResource.Data.Sku?.Tier,
                Capacity = capacity ?? vmssResource.Data.Sku?.Capacity
            };
            needsUpdate = true;
        }

        if (upgradePolicy != null || enableAutoOsUpgrade.HasValue)
        {
            patch.UpgradePolicy = new()
            {
                Mode = upgradePolicy != null ? ParseUpgradePolicy(upgradePolicy) : vmssResource.Data.UpgradePolicy?.Mode
            };

            if (enableAutoOsUpgrade.HasValue)
            {
                patch.UpgradePolicy.AutomaticOSUpgradePolicy = new()
                {
                    EnableAutomaticOSUpgrade = enableAutoOsUpgrade.Value
                };
            }

            needsUpdate = true;
        }

        if (overprovision.HasValue)
        {
            patch.Overprovision = overprovision.Value;
            needsUpdate = true;
        }

        if (scaleInPolicy != null)
        {
            patch.ScaleInPolicy = new();
            patch.ScaleInPolicy.Rules.Add(ParseScaleInPolicy(scaleInPolicy));
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
            var updateOperation = await vmssResource.UpdateAsync(
                WaitUntil.Completed,
                patch,
                cancellationToken: cancellationToken);
            vmssResource = updateOperation.Value;
        }

        return new(
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
            patch.HardwareProfile = new() { VmSize = new(vmSize) };
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
            patch.BootDiagnostics = new() { Enabled = enabled };
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
                WaitUntil.Completed,
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
        catch (RequestFailedException ex)
        {
            // Instance view may not be available due to permissions or VM state
            _logger.LogDebug(ex, "Could not retrieve instance view for VM {VmName}", vmResource.Data.Name);
        }

        return new(
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

    public async Task<bool> DeleteVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        bool? forceDeletion = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resourceGroupResource = rgResource.Value;

        var vmCollection = resourceGroupResource.GetVirtualMachines();

        try
        {
            var vmResponse = await vmCollection.GetAsync(vmName, cancellationToken: cancellationToken);
            var vmResource = vmResponse.Value;
            await vmResource.DeleteAsync(WaitUntil.Completed, forceDeletion, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug(ex, "VM {VmName} not found in resource group {ResourceGroup}", vmName, resourceGroup);
            return false;
        }
    }

    public async Task<bool> DeleteVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        bool? forceDeletion = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resourceGroupResource = rgResource.Value;

        var vmssCollection = resourceGroupResource.GetVirtualMachineScaleSets();

        try
        {
            var vmssResponse = await vmssCollection.GetAsync(vmssName, cancellationToken: cancellationToken);
            var vmssResource = vmssResponse.Value;
            await vmssResource.DeleteAsync(WaitUntil.Completed, forceDeletion, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug(ex, "VMSS {VmssName} not found in resource group {ResourceGroup}", vmssName, resourceGroup);
            return false;
        }
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
                Location = new(location),
                AddressPrefixes = { "10.0.0.0/16" },
                Subnets =
                {
                    new()
                    {
                        Name = subnetName,
                        AddressPrefix = "10.0.0.0/24"
                    }
                }
            };

            var vnetOperation = await vnetCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
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
                WaitUntil.Completed,
                subnetName,
                subnetData,
                cancellationToken);
            subnetResource = subnetOperation.Value;
        }

        return subnetResource.Id;
    }

    private static VmInfo MapToVmInfo(VirtualMachineData data)
    {
        return new(
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

        return new(
            Name: vmName,
            PowerState: powerState,
            ProvisioningState: provisioningState,
            VmAgent: instanceView.VmAgent != null ? new(
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
        return new(
            Code: status.Code,
            Level: status.Level?.ToString(),
            DisplayStatus: status.DisplayStatus,
            Message: status.Message,
            Time: status.Time
        );
    }

    private static VmssInfo MapToVmssInfo(VirtualMachineScaleSetData data)
    {
        return new(
            Name: data.Name,
            Id: data.Id?.ToString(),
            Location: data.Location.Name,
            Sku: data.Sku != null ? new(
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
        return new(
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
            _logger.LogError(ex, "Failed to list disks.");
            throw;
        }
    }

    private static DiskInfo ConvertToDiskModel(ManagedDiskResource diskResource, string resourceGroup)
    {
        var disk = diskResource.Data;
        return new()
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
        string? galleryImageReference = null,
        int? galleryImageReferenceLun = null,
        long? diskIopsReadWrite = null,
        long? diskMbpsReadWrite = null,
        string? uploadType = null,
        long? uploadSizeBytes = null,
        string? securityType = null,
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

        var creationData = CreateDiskCreationData(source, galleryImageReference, galleryImageReferenceLun, uploadType, uploadSizeBytes);

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

        if (tags is not null)
        {
            diskData.Tags.Clear();
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

        if (diskIopsReadWrite.HasValue)
        {
            diskData.DiskIopsReadWrite = diskIopsReadWrite.Value;
        }

        if (diskMbpsReadWrite.HasValue)
        {
            diskData.DiskMBpsReadWrite = diskMbpsReadWrite.Value;
        }

        if (!string.IsNullOrEmpty(securityType))
        {
            diskData.SecurityProfile = new DiskSecurityProfile
            {
                SecurityType = new DiskSecurityType(securityType)
            };
        }

        _logger.LogInformation("Creating disk {DiskName} in resource group {ResourceGroup}", diskName, resourceGroup);

        var result = await rgResource.Value.GetManagedDisks()
            .CreateOrUpdateAsync(Azure.WaitUntil.Completed, diskName, diskData, cancellationToken);

        return ConvertToDiskModel(result.Value, resourceGroup);
    }

    public async Task<DiskInfo> UpdateDiskAsync(
        string diskName,
        string resourceGroup,
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

        if (tags is not null)
        {
            diskPatch.Tags.Clear();
            if (!string.IsNullOrEmpty(tags))
            {
                foreach (var pair in tags.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        diskPatch.Tags[parts[0]] = parts[1];
                    }
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

        return ConvertToDiskModel(result.Value, resourceGroup);
    }

    private static DiskCreationData CreateDiskCreationData(string? source, string? galleryImageReference = null, int? galleryImageReferenceLun = null, string? uploadType = null, long? uploadSizeBytes = null)
    {
        if (!string.IsNullOrEmpty(uploadType))
        {
            var createOption = uploadType.Equals("UploadWithSecurityData", StringComparison.OrdinalIgnoreCase)
                ? DiskCreateOption.UploadPreparedSecure
                : DiskCreateOption.Upload;

            return new DiskCreationData(createOption)
            {
                UploadSizeBytes = uploadSizeBytes
            };
        }

        if (!string.IsNullOrEmpty(galleryImageReference))
        {
            var creationData = new DiskCreationData(DiskCreateOption.FromImage)
            {
                GalleryImageReference = new ImageDiskReference
                {
                    Id = new Azure.Core.ResourceIdentifier(galleryImageReference)
                }
            };

            if (galleryImageReferenceLun.HasValue)
            {
                creationData.GalleryImageReference.Lun = galleryImageReferenceLun.Value;
            }

            return creationData;
        }

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

    public async Task<bool> DeleteDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
            var diskResource = await resourceGroupResource.Value.GetManagedDisks().GetAsync(diskName, cancellationToken);

            await diskResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}",
                diskName, resourceGroup);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning(
                "Disk not found during delete operation. Disk: {Disk}, ResourceGroup: {ResourceGroup}",
                diskName, resourceGroup);

            // Return false to indicate the disk was not found (idempotent delete)
            return false;
        }
    }
}
