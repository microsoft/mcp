// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
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
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Services;

public class ComputeService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory,
    ILogger<ComputeService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IComputeService
{
    private readonly ILogger<ComputeService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    // Named HttpClient registered in ComputeSetup.cs for the unauthenticated Azure Retail Prices API.
    internal const string RetailPricesClientName = "AzureRetailPrices";

    // Default VM size (D-series v5, approximately 2 vCPU and 8 GB RAM)
    private const string DefaultVmSize = "Standard_D2s_v5";

    private sealed record ImageSource
    {
        public string? Publisher { get; init; }
        public string? Offer { get; init; }
        public string? Sku { get; init; }
        public string? Version { get; init; }
        public string? SharedGalleryImageUniqueId { get; init; }

        public bool IsSharedGallery => SharedGalleryImageUniqueId != null;

        public static ImageSource FromMarketplace(string publisher, string offer, string sku, string version) =>
            new() { Publisher = publisher, Offer = offer, Sku = sku, Version = version };

        public static ImageSource FromSharedGallery(string sharedGalleryImageUniqueId) =>
            new() { SharedGalleryImageUniqueId = sharedGalleryImageUniqueId };
    }

    private static readonly Dictionary<string, ImageSource> s_imageAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ubuntu2604"] = ImageSource.FromMarketplace("Canonical", "ubuntu-26_04-lts", "server", "latest"),
        ["Ubuntu2404"] = ImageSource.FromMarketplace("Canonical", "ubuntu-24_04-lts", "server", "latest"),
        ["Ubuntu2204"] = ImageSource.FromMarketplace("Canonical", "0001-com-ubuntu-server-jammy", "22_04-lts-gen2", "latest"),
        ["Debian11"] = ImageSource.FromMarketplace("Debian", "debian-11", "11-gen2", "latest"),
        ["Debian12"] = ImageSource.FromMarketplace("Debian", "debian-12", "12-gen2", "latest"),
        ["RHEL9"] = ImageSource.FromMarketplace("RedHat", "RHEL", "9_0", "latest"),
        ["CentOS8"] = ImageSource.FromMarketplace("OpenLogic", "CentOS", "8_5-gen2", "latest"),
        ["Win2022Datacenter"] = ImageSource.FromMarketplace("MicrosoftWindowsServer", "WindowsServer2022", "2022-datacenter-azure-edition", "latest"),
        ["Win2022Datacenter1P"] = ImageSource.FromSharedGallery("/sharedGalleries/WINDOWSSERVER.1P/images/2022-DATACENTER-AZURE-EDITION/versions/latest"),
        ["Win11Pro"] = ImageSource.FromMarketplace("MicrosoftWindowsDesktop", "windows-11", "win11-22h2-pro", "latest"),
        ["Win10Pro"] = ImageSource.FromMarketplace("MicrosoftWindowsDesktop", "Windows-10", "win10-22h2-pro-g2", "latest")
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

        // Use default VM size (Standard_D2s_v5) when not specified
        var effectiveVmSize = vmSize ?? DefaultVmSize;

        // Determine disk settings - let Azure choose disk type based on VM size when not specified
        var effectiveOsDiskType = osDiskType;
        // Only use explicit disk size if provided; otherwise let Azure use image's default size
        var effectiveOsDiskSizeGb = osDiskSizeGb;

        // Parse image (required for VM create)
        var imageSource = ParseImage(image!);

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
                ImageReference = CreateImageReference(imageSource)
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
            WaitUntil.Started,
            vmName,
            vmData,
            cancellationToken);
        await WaitForLroCompletionAsync(vmOperation, cancellationToken);

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

    private static ImageSource ParseImage(string image)
    {
        if (string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("An image must be specified. Provide an alias (e.g., 'Ubuntu2404', 'Win2022Datacenter'), a Marketplace URN ('publisher:offer:sku:version'), or a shared gallery image ID (starting with '/sharedGalleries/').", nameof(image));
        }

        // Check if it's an alias
        if (s_imageAliases.TryGetValue(image, out var aliasConfig))
        {
            return aliasConfig;
        }

        // Check if it's a shared gallery image URI
        if (image.StartsWith("/sharedGalleries/", StringComparison.OrdinalIgnoreCase))
        {
            return ImageSource.FromSharedGallery(image);
        }

        // Try to parse as URN (publisher:offer:sku:version)
        var parts = image.Split(':');
        if (parts.Length == 4)
        {
            return ImageSource.FromMarketplace(parts[0], parts[1], parts[2], parts[3]);
        }

        throw new ArgumentException($"Unrecognized image '{image}'. Provide a known alias, a Marketplace URN ('publisher:offer:sku:version'), or a shared gallery image ID (starting with '/sharedGalleries/').", nameof(image));
    }

    private static ImageReference CreateImageReference(ImageSource source)
    {
        if (source.IsSharedGallery)
        {
            return new() { SharedGalleryImageUniqueId = source.SharedGalleryImageUniqueId };
        }

        return new()
        {
            Publisher = source.Publisher,
            Offer = source.Offer,
            Sku = source.Sku,
            Version = source.Version
        };
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
                WaitUntil.Started,
                nsgName,
                nsgData,
                cancellationToken);
            await WaitForLroCompletionAsync(nsgOperation, cancellationToken);
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
                WaitUntil.Started,
                vnetName,
                vnetData,
                cancellationToken);
            await WaitForLroCompletionAsync(vnetOperation, cancellationToken);
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
                    WaitUntil.Started,
                    pipName,
                    pipData,
                    cancellationToken);
                await WaitForLroCompletionAsync(pipOperation, cancellationToken);
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
            WaitUntil.Started,
            nicName,
            nicData,
            cancellationToken);
        await WaitForLroCompletionAsync(nicOperation, cancellationToken);

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
        string? publicIpAddress = null,
        string? networkSecurityGroup = null,
        bool? noPublicIp = null,
        string? sourceAddressPrefix = null,
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

        // Use default VM size (Standard_D2s_v5) when not specified
        var effectiveVmSize = vmSize ?? DefaultVmSize;

        // Determine disk settings - let Azure choose disk type based on VM size when not specified
        var effectiveOsDiskType = osDiskType;
        var effectiveOsDiskSizeGb = osDiskSizeGb;
        var effectiveInstanceCount = instanceCount ?? 2;
        var effectiveUpgradePolicy = ParseUpgradePolicy(upgradePolicy);
        var effectiveNoPublicIp = noPublicIp ?? false;

        // Parse image - required, no default
        var imageSource = ParseImage(image!);

        // Create or get network resources for VMSS (subnet + optional NSG)
        var (subnetId, nsgId) = await CreateOrGetVmssNetworkResourcesAsync(
            resourceGroupResource,
            vmssName,
            location,
            virtualNetwork,
            subnet,
            networkSecurityGroup,
            effectiveNoPublicIp,
            effectiveOsType,
            sourceAddressPrefix,
            cancellationToken);

        // Build the primary IP configuration; attach public IP config unless suppressed.
        var ipConfig = new VirtualMachineScaleSetIPConfiguration($"{vmssName}-ipconfig")
        {
            Primary = true,
            SubnetId = subnetId
        };

        if (!effectiveNoPublicIp)
        {
            ipConfig.PublicIPAddressConfiguration = new(publicIpAddress ?? $"{vmssName}-pip")
            {
                IdleTimeoutInMinutes = 15,
                PublicIPAddressVersion = IPVersion.IPv4
            };
        }

        var nicConfig = new VirtualMachineScaleSetNetworkConfiguration($"{vmssName}-nic")
        {
            Primary = true,
            IPConfigurations = { ipConfig }
        };

        if (nsgId is not null)
        {
            nicConfig.NetworkSecurityGroupId = nsgId;
        }

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
                    ImageReference = CreateImageReference(imageSource)
                },
                OSProfile = new()
                {
                    // VMSS computer name prefix - Azure appends instance number
                    ComputerNamePrefix = vmssName.Length > 9 ? vmssName[..9] : vmssName,
                    AdminUsername = adminUsername
                },
                NetworkProfile = new()
                {
                    NetworkInterfaceConfigurations = { nicConfig }
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
            WaitUntil.Started,
            vmssName,
            vmssData,
            cancellationToken);
        await WaitForLroCompletionAsync(vmssOperation, cancellationToken);

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
                WaitUntil.Started,
                patch,
                cancellationToken: cancellationToken);
            await WaitForLroCompletionAsync(updateOperation, cancellationToken);
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
                WaitUntil.Started,
                patch,
                cancellationToken: cancellationToken);
            await WaitForLroCompletionAsync(updateOperation, cancellationToken);
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
            var deleteOperation = await vmResource.DeleteAsync(WaitUntil.Started, forceDeletion, cancellationToken);
            await WaitForLroCompletionAsync(deleteOperation, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug(ex, "VM {VmName} not found in resource group {ResourceGroup}", vmName, resourceGroup);
            return false;
        }
    }

    public async Task<VmPowerStateResult> ChangeVmPowerStateAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string powerAction,
        bool noWait = false,
        bool skipShutdown = false,
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
        var vmResponse = await vmCollection.GetAsync(vmName, cancellationToken: cancellationToken);
        var vmResource = vmResponse.Value;

        ArmOperation operation = powerAction.ToLowerInvariant() switch
        {
            "start" => await vmResource.PowerOnAsync(WaitUntil.Started, cancellationToken),
            "stop" => await vmResource.PowerOffAsync(WaitUntil.Started, skipShutdown, cancellationToken),
            "deallocate" => await vmResource.DeallocateAsync(WaitUntil.Started, cancellationToken: cancellationToken),
            "restart" => await vmResource.RestartAsync(WaitUntil.Started, cancellationToken),
            _ => throw new ArgumentException($"Invalid power action '{powerAction}'. Accepted values: start, stop, deallocate, restart.", nameof(powerAction))
        };

        if (!noWait)
        {
            await WaitForLroCompletionAsync(operation, cancellationToken);
        }

        var completed = !noWait;

        // When --no-wait is used, surface the ARM long-running-operation tracking URL so callers
        // can poll the status of the specific power-state request. Prefer Azure-AsyncOperation
        // (returns a status document with InProgress/Succeeded/Failed) and fall back to Location.
        string? statusUri = null;
        if (noWait)
        {
            var rawResponse = operation.GetRawResponse();
            if (rawResponse?.Headers != null &&
                !rawResponse.Headers.TryGetValue("Azure-AsyncOperation", out statusUri))
            {
                rawResponse.Headers.TryGetValue("Location", out statusUri);
            }
        }

        var message = completed
            ? $"Virtual machine '{vmName}' {powerAction} operation completed successfully."
            : statusUri is not null
                ? $"Virtual machine '{vmName}' {powerAction} operation initiated. Poll 'statusUri' to track completion."
                : $"Virtual machine '{vmName}' {powerAction} operation initiated. Use instance view to check status.";

        return new VmPowerStateResult(vmName, vmResource.Id.ToString(), resourceGroup, powerAction, message, completed, statusUri);
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
            var deleteOperation = await vmssResource.DeleteAsync(WaitUntil.Started, forceDeletion, cancellationToken);
            await WaitForLroCompletionAsync(deleteOperation, cancellationToken);
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

    private async Task<(ResourceIdentifier SubnetId, ResourceIdentifier? NsgId)> CreateOrGetVmssNetworkResourcesAsync(
        ResourceGroupResource resourceGroup,
        string vmssName,
        string location,
        string? virtualNetwork,
        string? subnet,
        string? networkSecurityGroup,
        bool noPublicIp,
        string osType,
        string? sourceAddressPrefix,
        CancellationToken cancellationToken)
    {
        var effectiveSourceAddressPrefix = sourceAddressPrefix ?? "*";
        var vnetName = virtualNetwork ?? $"{vmssName}-vnet";
        var subnetName = subnet ?? "default";

        // Create or get NSG.
        // Skip NSG creation entirely when there's no public IP and the user didn't ask for an NSG —
        // a fully internal VMSS doesn't need an external-facing rule on the default-create path.
        ResourceIdentifier? nsgId = null;
        if (!noPublicIp || !string.IsNullOrEmpty(networkSecurityGroup))
        {
            var nsgName = networkSecurityGroup ?? $"{vmssName}-nsg";
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

                // Add appropriate security rule based on OS type.
                // WARNING: These rules allow access from any source IP for quick-start scenarios.
                // For production use, restrict SourceAddressPrefix to specific IP ranges.
                var isWindows = osType.Equals("Windows", StringComparison.OrdinalIgnoreCase);

                if (isWindows)
                {
                    if (effectiveSourceAddressPrefix == "*")
                    {
                        _logger.LogWarning("Creating VMSS NSG with RDP (port 3389) open to all sources. For production, restrict the source IP range using --source-address-prefix.");
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
                        _logger.LogWarning("Creating VMSS NSG with SSH (port 22) open to all sources. For production, restrict the source IP range using --source-address-prefix.");
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
                    WaitUntil.Started,
                    nsgName,
                    nsgData,
                    cancellationToken);
                await WaitForLroCompletionAsync(nsgOperation, cancellationToken);
                nsgResource = nsgOperation.Value;
            }

            nsgId = nsgResource.Id;
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
                WaitUntil.Started,
                vnetName,
                vnetData,
                cancellationToken);
            await WaitForLroCompletionAsync(vnetOperation, cancellationToken);
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
                WaitUntil.Started,
                subnetName,
                subnetData,
                cancellationToken);
            await WaitForLroCompletionAsync(subnetOperation, cancellationToken);
            subnetResource = subnetOperation.Value;
        }

        return (subnetResource.Id, nsgId);
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

        var creationData = CreateDiskCreationData(source, TenantService.CloudConfiguration.ArmEnvironment, galleryImageReference, galleryImageReferenceLun, uploadType, uploadSizeBytes);

        var diskData = new ManagedDiskData(new(resolvedLocation))
        {
            CreationData = creationData
        };

        if (sizeGb.HasValue)
        {
            diskData.DiskSizeGB = sizeGb.Value;
        }

        if (!string.IsNullOrEmpty(sku))
        {
            diskData.Sku = new() { Name = new(sku) };
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
            diskData.HyperVGeneration = new(hyperVGeneration);
        }

        if (maxShares.HasValue)
        {
            diskData.MaxShares = maxShares.Value;
        }

        if (!string.IsNullOrEmpty(networkAccessPolicy))
        {
            diskData.NetworkAccessPolicy = new(networkAccessPolicy);
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
            diskData.Encryption ??= new();
            if (!string.IsNullOrEmpty(diskEncryptionSet))
            {
                diskData.Encryption.DiskEncryptionSetId = new(diskEncryptionSet);
            }

            if (!string.IsNullOrEmpty(encryptionType))
            {
                diskData.Encryption.EncryptionType = new(encryptionType);
            }
        }

        if (!string.IsNullOrEmpty(diskAccessId))
        {
            diskData.DiskAccessId = new(diskAccessId);
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
            diskData.SecurityProfile = new()
            {
                SecurityType = new(securityType)
            };
        }

        _logger.LogInformation("Creating disk {DiskName} in resource group {ResourceGroup}", diskName, resourceGroup);

        var createOperation = await rgResource.Value.GetManagedDisks()
            .CreateOrUpdateAsync(WaitUntil.Started, diskName, diskData, cancellationToken);
        await WaitForLroCompletionAsync(createOperation, cancellationToken);

        return ConvertToDiskModel(createOperation.Value, resourceGroup);
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
            diskPatch.Sku = new() { Name = new(sku) };
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
            diskPatch.NetworkAccessPolicy = new(networkAccessPolicy);
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
            diskPatch.Encryption ??= new();
            if (!string.IsNullOrEmpty(diskEncryptionSet))
            {
                diskPatch.Encryption.DiskEncryptionSetId = new(diskEncryptionSet);
            }

            if (!string.IsNullOrEmpty(encryptionType))
            {
                diskPatch.Encryption.EncryptionType = new(encryptionType);
            }
        }

        if (!string.IsNullOrEmpty(diskAccessId))
        {
            diskPatch.DiskAccessId = new(diskAccessId);
        }

        if (!string.IsNullOrEmpty(tier))
        {
            diskPatch.Tier = tier;
        }

        _logger.LogInformation("Updating disk {DiskName} in resource group {ResourceGroup}", diskName, resourceGroup);

        var updateOperation = await diskResource.Value.UpdateAsync(WaitUntil.Started, diskPatch, cancellationToken);
        await WaitForLroCompletionAsync(updateOperation, cancellationToken);

        return ConvertToDiskModel(updateOperation.Value, resourceGroup);
    }

    private static DiskCreationData CreateDiskCreationData(string? source, ArmEnvironment armEnvironment, string? galleryImageReference = null, int? galleryImageReferenceLun = null, string? uploadType = null, long? uploadSizeBytes = null)
    {
        if (!string.IsNullOrEmpty(uploadType))
        {
            var createOption = uploadType.Equals("UploadWithSecurityData", StringComparison.OrdinalIgnoreCase)
                ? DiskCreateOption.UploadPreparedSecure
                : DiskCreateOption.Upload;

            return new(createOption)
            {
                UploadSizeBytes = uploadSizeBytes
            };
        }

        if (!string.IsNullOrEmpty(galleryImageReference))
        {
            var creationData = new DiskCreationData(DiskCreateOption.FromImage)
            {
                GalleryImageReference = new()
                {
                    Id = new(galleryImageReference)
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
            return new(DiskCreateOption.Empty);
        }

        // Blob URIs start with http:// or https:// - validate via EndpointValidator
        if (source.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", armEnvironment);
            return new(DiskCreateOption.Import)
            {
                SourceUri = new(source)
            };
        }

        // Otherwise treat as a resource ID (snapshot or managed disk)
        return new(DiskCreateOption.Copy)
        {
            SourceResourceId = new(source)
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

            var deleteOperation = await diskResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

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

    // ============================================================
    // Guided-create discovery (read-only): SKUs, Images, Quota, Regions
    // ============================================================

    public async Task<List<VmSkuInfo>> ListVmSkusAsync(
        string subscription,
        string location,
        int? minVCpus = null,
        double? minMemoryGb = null,
        string? familyPrefix = null,
        int? top = null,
        bool includePricing = false,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var filter = $"location eq '{location}'";
        var results = new List<VmSkuInfo>();
        var limit = top ?? 20;

        await foreach (var sku in subscriptionResource.GetComputeResourceSkusAsync(filter, includeExtendedLocations: null, cancellationToken))
        {
            if (!string.Equals(sku.ResourceType, "virtualMachines", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrEmpty(familyPrefix) &&
                !(sku.Name?.StartsWith(familyPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                  sku.Family?.StartsWith(familyPrefix, StringComparison.OrdinalIgnoreCase) == true))
                continue;

            int? vCpus = null;
            double? memoryGb = null;
            int? maxDataDisks = null;
            bool? acceleratedNetworking = null;
            bool? premiumIo = null;
            int? gpus = null;
            bool? vmScaleSetsSupported = null;

            if (sku.Capabilities is not null)
            {
                foreach (var cap in sku.Capabilities)
                {
                    switch (cap.Name)
                    {
                        case "vCPUs":
                            if (int.TryParse(cap.Value, out var v))
                                vCpus = v;
                            break;
                        case "MemoryGB":
                            if (double.TryParse(cap.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var m))
                                memoryGb = m;
                            break;
                        case "MaxDataDiskCount":
                            if (int.TryParse(cap.Value, out var d))
                                maxDataDisks = d;
                            break;
                        case "AcceleratedNetworkingEnabled":
                            acceleratedNetworking = string.Equals(cap.Value, "True", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "PremiumIO":
                            premiumIo = string.Equals(cap.Value, "True", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "GPUs":
                            if (int.TryParse(cap.Value, out var g))
                                gpus = g;
                            break;
                        case "VMScaleSetsSupported":
                            vmScaleSetsSupported = string.Equals(cap.Value, "True", StringComparison.OrdinalIgnoreCase);
                            break;
                    }
                }
            }

            if (minVCpus.HasValue && (!vCpus.HasValue || vCpus.Value < minVCpus.Value))
                continue;
            if (minMemoryGb.HasValue && (!memoryGb.HasValue || memoryGb.Value < minMemoryGb.Value))
                continue;

            var zones = sku.LocationInfo?
                .SelectMany(li => li.Zones ?? Enumerable.Empty<string>())
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            var restrictions = sku.Restrictions?
                .Select(r => $"{r.RestrictionsType}:{r.ReasonCode}")
                .ToList();

            results.Add(new VmSkuInfo(
                Name: sku.Name ?? string.Empty,
                Family: sku.Family,
                Size: sku.Size,
                Tier: sku.Tier,
                VCpus: vCpus,
                MemoryGb: memoryGb,
                MaxDataDisks: maxDataDisks,
                AcceleratedNetworking: acceleratedNetworking,
                PremiumIo: premiumIo,
                Gpus: gpus,
                Zones: zones,
                Restrictions: restrictions,
                PayAsYouGoHourlyUsd: null,
                SpotHourlyUsd: null,
                VMScaleSetsSupported: vmScaleSetsSupported));

            if (results.Count >= limit)
                break;
        }

        if (includePricing && results.Count > 0)
        {
            // Fan out Retail Prices API lookups in parallel with bounded concurrency.
            // The API is unauthenticated and serial calls dominate latency when --include-pricing is set.
            using var gate = new SemaphoreSlim(8);
            var priceTasks = results
                .Select(async (info, idx) =>
                {
                    if (string.IsNullOrEmpty(info.Name))
                        return (idx, (decimal?)null, (decimal?)null);

                    await gate.WaitAsync(cancellationToken);
                    try
                    {
                        var (pay, spot) = await TryFetchRetailPricesAsync(info.Name, location, cancellationToken);
                        return (idx, pay, spot);
                    }
                    finally
                    {
                        gate.Release();
                    }
                })
                .ToList();

            var prices = await Task.WhenAll(priceTasks);
            foreach (var (idx, pay, spot) in prices)
            {
                results[idx] = results[idx] with { PayAsYouGoHourlyUsd = pay, SpotHourlyUsd = spot };
            }
        }

        return results;
    }

    private async Task<(decimal? PayAsYouGo, decimal? Spot)> TryFetchRetailPricesAsync(
        string armSkuName,
        string location,
        CancellationToken cancellationToken)
    {
        try
        {
            var filter = $"serviceName eq 'Virtual Machines' and armSkuName eq '{armSkuName}' and armRegionName eq '{location}' and priceType eq 'Consumption'";
            var url = $"https://prices.azure.com/api/retail/prices?$filter={Uri.EscapeDataString(filter)}&$top=50";

            var httpClient = _httpClientFactory.CreateClient(RetailPricesClientName);
            using var resp = await httpClient.GetAsync(url, cancellationToken);
            if (!resp.IsSuccessStatusCode)
                return (null, null);

            var json = await resp.Content.ReadAsStringAsync(cancellationToken);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("Items", out var items))
                return (null, null);

            decimal? pay = null;
            decimal? spot = null;
            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("retailPrice", out var priceEl))
                    continue;
                if (!item.TryGetProperty("productName", out var prodEl))
                    continue;
                if (!item.TryGetProperty("skuName", out var skuEl))
                    continue;

                var productName = prodEl.GetString() ?? string.Empty;
                var skuName = skuEl.GetString() ?? string.Empty;
                if (productName.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                    continue;

                var price = priceEl.GetDecimal();
                if (skuName.Contains("Spot", StringComparison.OrdinalIgnoreCase))
                {
                    spot ??= price;
                }
                else if (skuName.Contains("Low Priority", StringComparison.OrdinalIgnoreCase))
                {
                    // skip
                }
                else
                {
                    pay ??= price;
                }
            }
            return (pay, spot);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Retail Prices API lookup failed for {Sku} in {Location}", armSkuName, location);
            return (null, null);
        }
    }

    public async Task<List<VmImageInfo>> ListVmImagesAsync(
        string subscription,
        string location,
        string? alias = null,
        string? publisher = null,
        string? offer = null,
        string? sku = null,
        int? top = null,
        bool includeSharedGallery = false,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<VmImageInfo>();
        var limit = top ?? 50;

        // Case 1: alias resolution (single or all)
        if (!string.IsNullOrEmpty(alias))
        {
            if (s_imageAliases.TryGetValue(alias, out var src))
            {
                results.Add(ToImageInfo(alias, src));
            }
            return results;
        }

        var noMarketplaceFilter = string.IsNullOrEmpty(publisher) && string.IsNullOrEmpty(offer) && string.IsNullOrEmpty(sku);

        if (noMarketplaceFilter)
        {
            // Default: return the built-in alias catalog
            foreach (var kvp in s_imageAliases)
            {
                results.Add(ToImageInfo(kvp.Key, kvp.Value));
                if (results.Count >= limit)
                    break;
            }

            // Optionally append shared-gallery images visible to this subscription.
            if (includeSharedGallery && results.Count < limit)
            {
                await AppendSharedGalleryImagesAsync(results, subscription, location, limit, tenant, retryPolicy, cancellationToken);
            }

            return results;
        }

        // Case 2: marketplace listing (requires publisher + offer + sku for version listing)
        if (string.IsNullOrEmpty(publisher) || string.IsNullOrEmpty(offer) || string.IsNullOrEmpty(sku))
        {
            return results;
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var azureLocation = new Azure.Core.AzureLocation(location);
        try
        {
            var versions = subscriptionResource.GetVirtualMachineImagesAsync(
                azureLocation, publisher!, offer!, sku!, cancellationToken: cancellationToken);

            await foreach (var version in versions)
            {
                results.Add(new VmImageInfo(
                    Alias: null,
                    Publisher: publisher,
                    Offer: offer,
                    Sku: sku,
                    Version: version.Name,
                    Urn: $"{publisher}:{offer}:{sku}:{version.Name}",
                    OsType: null,
                    HyperVGeneration: null,
                    Source: "marketplace"));

                if (results.Count >= limit)
                    break;
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogDebug(ex, "GetVirtualMachineImagesAsync failed for {Publisher}/{Offer}/{Sku} in {Location}", publisher, offer, sku, location);
        }

        // Optionally also enumerate shared-gallery images so the marketplace
        // and gallery results are unified in a single response.
        if (includeSharedGallery && results.Count < limit)
        {
            await AppendSharedGalleryImagesAsync(results, subscription, location, limit, tenant, retryPolicy, cancellationToken);
        }

        return results;
    }

    private async Task AppendSharedGalleryImagesAsync(
        List<VmImageInfo> results,
        string subscription,
        string location,
        int limit,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                SubscriptionResource.CreateResourceIdentifier(subscription));

            var azureLocation = new Azure.Core.AzureLocation(location);
            var galleries = subscriptionResource.GetSharedGalleries(azureLocation).GetAllAsync(null, cancellationToken);

            await foreach (var gallery in galleries)
            {
                if (results.Count >= limit)
                    return;

                SharedGalleryImageCollection imageCollection;
                try
                {
                    imageCollection = gallery.GetSharedGalleryImages();
                }
                catch (RequestFailedException ex)
                {
                    _logger.LogDebug(ex, "GetSharedGalleryImages failed for gallery {Gallery} in {Location}", gallery.Data?.Name, location);
                    continue;
                }

                await foreach (var image in imageCollection.GetAllAsync(null, cancellationToken))
                {
                    if (results.Count >= limit)
                        return;

                    SharedGalleryImageVersionCollection versionCollection;
                    try
                    {
                        versionCollection = image.GetSharedGalleryImageVersions();
                    }
                    catch (RequestFailedException ex)
                    {
                        _logger.LogDebug(ex, "GetSharedGalleryImageVersions failed for image {Image} in gallery {Gallery}", image.Data?.Name, gallery.Data?.Name);
                        continue;
                    }

                    await foreach (var version in versionCollection.GetAllAsync(null, cancellationToken))
                    {
                        results.Add(new VmImageInfo(
                            Alias: null,
                            Publisher: image.Data?.Identifier?.Publisher,
                            Offer: image.Data?.Identifier?.Offer,
                            Sku: image.Data?.Identifier?.Sku,
                            Version: version.Data?.Name,
                            Urn: version.Data?.Id?.ToString(),
                            OsType: image.Data?.OSType.ToString(),
                            HyperVGeneration: image.Data?.HyperVGeneration?.ToString(),
                            Source: "sharedGallery"));

                        if (results.Count >= limit)
                            return;
                    }
                }
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogDebug(ex, "Shared gallery enumeration failed in {Location}", location);
        }
    }

    private static VmImageInfo ToImageInfo(string alias, ImageSource src)
    {
        if (src.IsSharedGallery)
        {
            return new VmImageInfo(
                Alias: alias,
                Publisher: null,
                Offer: null,
                Sku: null,
                Version: null,
                Urn: src.SharedGalleryImageUniqueId,
                OsType: ComputeUtilities.DetermineOsType(null, alias),
                HyperVGeneration: null,
                Source: "sharedGallery");
        }

        return new VmImageInfo(
            Alias: alias,
            Publisher: src.Publisher,
            Offer: src.Offer,
            Sku: src.Sku,
            Version: src.Version,
            Urn: $"{src.Publisher}:{src.Offer}:{src.Sku}:{src.Version}",
            OsType: ComputeUtilities.DetermineOsType(null, alias),
            HyperVGeneration: null,
            Source: "alias");
    }

    // VMSS-relevant usage dimensions surfaced alongside any family-prefix filter so the
    // guided VMSS Flex flow can see scale-set / low-priority / availability-set headroom
    // even when the user asked for a single SKU family.
    private static readonly HashSet<string> s_vmssQuotaDimensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "virtualMachineScaleSets",
        "lowPriorityCores",
        "availabilitySets"
    };

    public async Task<List<VmQuotaInfo>> CheckVmQuotaAsync(
        string subscription,
        string location,
        string? familyPrefix = null,
        int? requestedVCpus = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var results = new List<VmQuotaInfo>();
        var azureLocation = new Azure.Core.AzureLocation(location);

        await foreach (var usage in Azure.ResourceManager.Compute.ComputeExtensions.GetUsagesAsync(subscriptionResource, azureLocation, cancellationToken))
        {
            var nameValue = usage.Name?.Value ?? string.Empty;

            // Always surface VMSS planning dimensions, even when a family prefix is set,
            // so the guided flow has the context it needs to recommend Flex.
            var isVmssDimension = s_vmssQuotaDimensions.Contains(nameValue);

            if (!string.IsNullOrEmpty(familyPrefix) &&
                !nameValue.StartsWith(familyPrefix, StringComparison.OrdinalIgnoreCase) &&
                !isVmssDimension)
                continue;

            var available = Math.Max(0, usage.Limit - usage.CurrentValue);
            var percent = usage.Limit > 0 ? (double)usage.CurrentValue / usage.Limit * 100.0 : 0.0;
            var nearLimit = percent > 80.0;

            string status;
            if (requestedVCpus.HasValue && available < requestedVCpus.Value)
                status = "Insufficient";
            else if (nearLimit)
                status = "NearLimit";
            else
                status = "Sufficient";

            results.Add(new VmQuotaInfo(
                Name: nameValue,
                LocalizedName: usage.Name?.LocalizedValue,
                Unit: usage.Unit.ToString(),
                CurrentValue: usage.CurrentValue,
                Limit: usage.Limit,
                Available: available,
                PercentUsed: Math.Round(percent, 2),
                NearLimit: nearLimit,
                Status: status));
        }

        return results;
    }

    // Lightweight workload→SKU-family hint table. Used only for rationale text in region recommendations.
    // Last verified: 2026-05 — review against current Azure SKU catalog if families are added or renamed.
    private static readonly Dictionary<string, string> s_workloadFamilyHints = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gpu"] = "Standard_N",
        ["ai"] = "Standard_N",
        ["training"] = "Standard_N",
        ["inference"] = "Standard_N",
        ["render"] = "Standard_N",
        ["batch"] = "Standard_F",
        ["compute"] = "Standard_F",
        ["hpc"] = "Standard_H",
        ["memory"] = "Standard_E",
        ["database"] = "Standard_E",
        ["analytics"] = "Standard_E",
        ["web"] = "Standard_D",
        ["api"] = "Standard_D",
        ["general"] = "Standard_D",
        ["dev"] = "Standard_B",
        ["test"] = "Standard_B",
        ["sandbox"] = "Standard_B",
        ["burstable"] = "Standard_B"
    };

    // Lightweight tier-1 region preference list. The full list comes from the API; this just biases scoring.
    // Last verified: 2026-05 — popularity weights are heuristic; re-check when new regions GA.
    private static readonly Dictionary<string, int> s_regionPopularity = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eastus"] = 10,
        ["eastus2"] = 9,
        ["westus2"] = 9,
        ["westus3"] = 8,
        ["centralus"] = 8,
        ["northeurope"] = 9,
        ["westeurope"] = 9,
        ["uksouth"] = 8,
        ["southeastasia"] = 8,
        ["japaneast"] = 7,
        ["australiaeast"] = 7,
        ["canadacentral"] = 7,
        ["brazilsouth"] = 6,
        ["southafricanorth"] = 6,
        ["centralindia"] = 7,
        ["francecentral"] = 7,
        ["germanywestcentral"] = 7,
        ["norwayeast"] = 6,
        ["swedencentral"] = 6,
        ["switzerlandnorth"] = 6,
        ["uaenorth"] = 6,
        ["koreacentral"] = 7
    };

    // Regions with documented AZ support — sufficient for ranking, not authoritative.
    // Last verified: 2026-05 — cross-check at https://learn.microsoft.com/azure/reliability/availability-zones-region-support when adding regions.
    private static readonly HashSet<string> s_azRegions = new(StringComparer.OrdinalIgnoreCase)
    {
        "eastus", "eastus2", "westus2", "westus3", "centralus", "southcentralus",
        "canadacentral", "brazilsouth",
        "northeurope", "westeurope", "uksouth", "francecentral", "germanywestcentral",
        "norwayeast", "swedencentral", "switzerlandnorth",
        "uaenorth", "southafricanorth",
        "australiaeast", "centralindia", "japaneast", "koreacentral", "southeastasia",
        "eastasia", "qatarcentral"
    };

    // Hints that signal the user is thinking about scale-out / HA / production
    // workloads. When any of these appear in the workload hint, we apply a
    // stronger weight to availability-zone-rich regions so the recommendation
    // surface naturally biases toward VMSS Flex-friendly regions.
    private static readonly string[] s_vmssHaHints =
    {
        "scale", "ha", "production", "prod", "vmss"
    };

    public async Task<List<VmRegionRecommendation>> RecommendVmRegionsAsync(
        string subscription,
        string? workloadHint = null,
        string? geographyPreference = null,
        bool requireAvailabilityZones = false,
        int? top = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));

        var limit = top ?? 5;
        var hintLower = workloadHint?.ToLowerInvariant() ?? string.Empty;
        var geoLower = geographyPreference?.ToLowerInvariant();

        // Detect a family hint for rationale text only (no filtering — quota lives elsewhere)
        string? preferredFamily = null;
        foreach (var kvp in s_workloadFamilyHints)
        {
            if (hintLower.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                preferredFamily = kvp.Value;
                break;
            }
        }

        // Bias toward AZ-rich regions when the user signals scale-out / HA / production
        // intent (or explicitly required AZs). This is what makes the unified guided
        // flow recommend Flex-friendly regions by default.
        var prefersVmssLayout = s_vmssHaHints.Any(h => hintLower.Contains(h, StringComparison.OrdinalIgnoreCase));
        var azWeight = (prefersVmssLayout || requireAvailabilityZones) ? 8 : 3;

        var candidates = new List<VmRegionRecommendation>();

        await foreach (var loc in subscriptionResource.GetLocationsAsync(includeExtendedLocations: false, cancellationToken))
        {
            var name = loc.Name ?? string.Empty;
            if (string.IsNullOrEmpty(name))
                continue;

            var hasZones = s_azRegions.Contains(name);
            if (requireAvailabilityZones && !hasZones)
                continue;

            int score = 0;
            var rationaleParts = new List<string>();

            if (s_regionPopularity.TryGetValue(name, out var popularity))
            {
                score += popularity;
                if (popularity >= 8)
                    rationaleParts.Add("tier-1 region with broad SKU coverage");
            }

            if (hasZones)
            {
                score += azWeight;
                rationaleParts.Add(azWeight > 3
                    ? "supports Availability Zones (favored for VMSS Flex / HA)"
                    : "supports Availability Zones");
            }

            if (!string.IsNullOrEmpty(geoLower))
            {
                var geography = loc.Metadata?.Geography?.ToLowerInvariant() ?? string.Empty;
                var geographyGroup = loc.Metadata?.GeographyGroup?.ToLowerInvariant() ?? string.Empty;
                if (geography.Contains(geoLower) || geographyGroup.Contains(geoLower) || name.Contains(geoLower))
                {
                    score += 5;
                    rationaleParts.Add($"matches geography preference '{geographyPreference}'");
                }
            }

            if (preferredFamily is not null)
            {
                rationaleParts.Add($"hint suggests {preferredFamily}* family — verify SKU availability with compute_vm_list_skus (or the top-level alias compute_list_skus)");
            }

            if (rationaleParts.Count == 0)
            {
                rationaleParts.Add("available in subscription");
            }

            candidates.Add(new VmRegionRecommendation(
                Name: name,
                DisplayName: loc.DisplayName,
                Geography: loc.Metadata?.Geography,
                PhysicalLocation: loc.Metadata?.PhysicalLocation,
                AvailabilityZones: hasZones,
                Score: score,
                Rationale: string.Join("; ", rationaleParts)));
        }

        return candidates
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Name)
            .Take(limit)
            .ToList();
    }
}
