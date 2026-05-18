// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options;

public static class ComputeOptionDefinitions
{
    public const string VmNameName = "vm-name";
    public const string VmssNameName = "vmss-name";
    public const string InstanceIdName = "instance-id";
    public const string LocationName = "location";
    public const string DiskName = "disk-name";
    public const string SizeGbName = "size-gb";
    public const string SkuName = "sku";
    public const string OsTypeName = "os-type";
    public const string ZoneName = "zone";
    public const string HyperVGenerationName = "hyper-v-generation";
    public const string DiskIopsReadWriteName = "disk-iops-read-write";
    public const string DiskMbpsReadWriteName = "disk-mbps-read-write";
    public const string MaxSharesName = "max-shares";
    public const string NetworkAccessPolicyName = "network-access-policy";
    public const string EnableBurstingName = "enable-bursting";
    public const string SourceName = "source";
    public const string TagsName = "tags";
    public const string DiskEncryptionSetName = "disk-encryption-set";
    public const string EncryptionTypeName = "encryption-type";
    public const string DiskAccessIdName = "disk-access";
    public const string TierName = "tier";
    public const string GalleryImageReferenceName = "gallery-image-reference";
    public const string GalleryImageReferenceLunName = "gallery-image-reference-lun";
    public const string UploadTypeName = "upload-type";
    public const string UploadSizeBytesName = "upload-size-bytes";
    public const string SecurityTypeName = "security-type";

    public static readonly Option<string> Source = new($"--{SourceName}")
    {
        Description = "Source to create the disk from, including a resource ID of a snapshot or disk, or a blob URI of a VHD. When a source is provided, --size-gb is optional and defaults to the source size.",
        Required = false
    };
    public const string VmSizeName = "vm-size";
    public const string ImageName = "image";
    public const string AdminUsernameName = "admin-username";
    public const string AdminPasswordName = "admin-password";
    public const string SshPublicKeyName = "ssh-public-key";
    public const string VirtualNetworkName = "virtual-network";
    public const string SubnetName = "subnet";
    public const string PublicIpAddressName = "public-ip-address";
    public const string NetworkSecurityGroupName = "network-security-group";
    public const string NoPublicIpName = "no-public-ip";
    public const string SourceAddressPrefixName = "source-address-prefix";
    public const string OsDiskSizeGbName = "os-disk-size-gb";
    public const string OsDiskTypeName = "os-disk-type";

    public static readonly Option<string> Disk = new($"--{DiskName}", "--name")
    {
        Description = "The name of the disk",
        Required = false
    };

    public static readonly Option<int> SizeGb = new($"--{SizeGbName}", "-z")
    {
        Description = "Size of the disk in GB. Max size: 4095 GB.",
        Required = false
    };

    public static readonly Option<string> Sku = new($"--{SkuName}")
    {
        Description = "Underlying storage SKU. Accepted values: Premium_LRS, PremiumV2_LRS, Premium_ZRS, StandardSSD_LRS, StandardSSD_ZRS, Standard_LRS, UltraSSD_LRS.",
        Required = false
    };

    public static readonly Option<string> OsType = new($"--{OsTypeName}")
    {
        Description = "The Operating System type of the disk. Accepted values: Linux, Windows.",
        Required = false
    };

    public static readonly Option<string> Zone = new($"--{ZoneName}")
    {
        Description = "Availability zone into which to provision the resource.",
        Required = false
    };

    public static readonly Option<string> HyperVGeneration = new($"--{HyperVGenerationName}")
    {
        Description = "The hypervisor generation of the Virtual Machine. Applicable to OS disks only. Accepted values: V1, V2.",
        Required = false
    };

    public static readonly Option<long> DiskIopsReadWrite = new($"--{DiskIopsReadWriteName}")
    {
        Description = "The number of IOPS allowed for this disk. Only settable for UltraSSD disks.",
        Required = false
    };

    public static readonly Option<long> DiskMbpsReadWrite = new($"--{DiskMbpsReadWriteName}")
    {
        Description = "The bandwidth allowed for this disk in MBps. Only settable for UltraSSD disks.",
        Required = false
    };

    public static readonly Option<int> MaxShares = new($"--{MaxSharesName}")
    {
        Description = "The maximum number of VMs that can attach to the disk at the same time. Value greater than one indicates a shared disk.",
        Required = false
    };

    public static readonly Option<string> NetworkAccessPolicy = new($"--{NetworkAccessPolicyName}")
    {
        Description = "Policy for accessing the disk via network. Accepted values: AllowAll, AllowPrivate, DenyAll.",
        Required = false
    };

    public static readonly Option<string> EnableBursting = new($"--{EnableBurstingName}")
    {
        Description = "Enable on-demand bursting beyond the provisioned performance target of the disk. Does not apply to Ultra disks. Accepted values: true, false.",
        Required = false
    };

    public static readonly Option<string> Tags = new($"--{TagsName}")
    {
        Description = "Space-separated tags in 'key=value' format. Use '' to clear existing tags.",
        Required = false
    };

    public static readonly Option<string> DiskEncryptionSet = new($"--{DiskEncryptionSetName}")
    {
        Description = "Resource ID of the disk encryption set to use for enabling encryption at rest.",
        Required = false
    };

    public static readonly Option<string> EncryptionType = new($"--{EncryptionTypeName}")
    {
        Description = "Encryption type of the disk. Accepted values: EncryptionAtRestWithCustomerKey, EncryptionAtRestWithPlatformAndCustomerKeys, EncryptionAtRestWithPlatformKey.",
        Required = false
    };

    public static readonly Option<string> DiskAccessId = new($"--{DiskAccessIdName}")
    {
        Description = "Resource ID of the disk access resource for using private endpoints on disks.",
        Required = false
    };

    public static readonly Option<string> Tier = new($"--{TierName}")
    {
        Description = "Performance tier of the disk (e.g., P10, P15, P20, P30, P40, P50, P60, P70, P80). Applicable to Premium SSD disks only.",
        Required = false
    };

    public static readonly Option<string> UploadType = new($"--{UploadTypeName}")
    {
        Description = "Type of upload for the disk. Accepted values: Upload, UploadWithSecurityData. When specified, the disk is created in a ReadyToUpload state.",
        Required = false
    };

    public static readonly Option<long> UploadSizeBytes = new($"--{UploadSizeBytesName}")
    {
        Description = "The size in bytes (including the VHD footer of 512 bytes) of the content to be uploaded. Required when --upload-type is specified.",
        Required = false
    };

    public static readonly Option<string> SecurityType = new($"--{SecurityTypeName}")
    {
        Description = "Security type of the managed disk. Accepted values: ConfidentialVM_DiskEncryptedWithCustomerKey, ConfidentialVM_DiskEncryptedWithPlatformKey, ConfidentialVM_VMGuestStateOnlyEncryptedWithPlatformKey, Standard, TrustedLaunch. Required when --upload-type is UploadWithSecurityData.",
        Required = false
    };

    public static readonly Option<string> GalleryImageReference = new($"--{GalleryImageReferenceName}")
    {
        Description = "Resource ID of a Shared Image Gallery image version to use as the source for the disk. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/galleries/{gallery}/images/{image}/versions/{version}.",
        Required = false
    };

    public static readonly Option<int?> GalleryImageReferenceLun = new($"--{GalleryImageReferenceLunName}")
    {
        Description = "LUN (Logical Unit Number) of the data disk in the gallery image version. If specified, the disk is created from the data disk at this LUN. If not specified, the disk is created from the OS disk of the image.",
        Required = false
    };

    public static readonly Option<string> VmName = new($"--{VmNameName}", "--name")
    {
        Description = "The name of the virtual machine",
        Required = false
    };

    public static readonly Option<bool> InstanceView = new("--instance-view")
    {
        Description = "Include instance view details (only available when retrieving a specific VM)",
        Required = false
    };

    public static readonly Option<string> VmssName = new($"--{VmssNameName}", "--name")
    {
        Description = "The name of the virtual machine scale set",
        Required = false
    };

    public static readonly Option<string> InstanceId = new($"--{InstanceIdName}")
    {
        Description = "The instance ID of the virtual machine in the scale set",
        Required = false
    };

    public static readonly Option<string> Location = new($"--{LocationName}", "-l")
    {
        Description = "The Azure region/location. Defaults to the resource group's location if not specified.",
        Required = false
    };

    public static readonly Option<string> VmSize = new($"--{VmSizeName}", "--size")
    {
        Description = "The VM size used for the VM or for each scale set instance (e.g., Standard_D2s_v3, Standard_B2s). Defaults to Standard_D2s_v5 if not specified",
        Required = false
    };

    public static readonly Option<string> Image = new($"--{ImageName}")
    {
        Description = "The OS image to use. Can be a URN (publisher:offer:sku:version), a shared gallery image ID (starting with '/sharedGalleries/'), or an alias such as 'Ubuntu2404' or 'Win2022Datacenter'.",
        Required = false
    };

    public static readonly Option<string> AdminUsername = new($"--{AdminUsernameName}")
    {
        Description = "The admin username for the VM or scale set instance(s). Required for VM or VMSS creation",
        Required = false
    };

    public static readonly Option<string> AdminPassword = new($"--{AdminPasswordName}")
    {
        Description = "The admin password for Windows VMs or scale set instances, or when SSH key is not provided for Linux VMs or scale set instances",
        Required = false
    };

    public static readonly Option<string> SshPublicKey = new($"--{SshPublicKeyName}")
    {
        Description = "SSH public key for Linux VMs or scale set instances. Can be the key content or path to a file",
        Required = false
    };



    public static readonly Option<string> VirtualNetwork = new($"--{VirtualNetworkName}", "--vnet")
    {
        Description = "Name of an existing virtual network to use. If not specified, a new one will be created",
        Required = false
    };

    public static readonly Option<string> Subnet = new($"--{SubnetName}")
    {
        Description = "Name of the subnet within the virtual network",
        Required = false
    };

    public static readonly Option<string> PublicIpAddress = new($"--{PublicIpAddressName}")
    {
        Description = "Name of the public IP address to use or create",
        Required = false
    };

    public static readonly Option<string> NetworkSecurityGroup = new($"--{NetworkSecurityGroupName}", "--nsg")
    {
        Description = "Name of the network security group to use or create",
        Required = false
    };

    public static readonly Option<bool> NoPublicIp = new($"--{NoPublicIpName}")
    {
        Description = "Do not create or assign a public IP address",
        Required = false
    };

    public static readonly Option<string> SourceAddressPrefix = new($"--{SourceAddressPrefixName}")
    {
        Description = "Source IP address range for NSG inbound rules (e.g., '203.0.113.0/24' or a specific IP). Defaults to '*' (any source)",
        Required = false
    };



    public static readonly Option<int> OsDiskSizeGb = new($"--{OsDiskSizeGbName}")
    {
        Description = "OS disk size in GB. Defaults based on image requirements",
        Required = false
    };

    public static readonly Option<string> OsDiskType = new($"--{OsDiskTypeName}")
    {
        Description = "OS disk type: 'Premium_LRS', 'StandardSSD_LRS', 'Standard_LRS'. Defaults based on VM or scale set instance size",
        Required = false
    };

    // VMSS-specific options
    public const string InstanceCountName = "instance-count";
    public const string UpgradePolicyName = "upgrade-policy";

    public static readonly Option<int> InstanceCount = new($"--{InstanceCountName}")
    {
        Description = "Number of VM instances in the scale set. Default is 2",
        Required = false
    };

    public static readonly Option<string> UpgradePolicy = new($"--{UpgradePolicyName}")
    {
        Description = "Upgrade policy mode: 'Automatic', 'Manual', or 'Rolling'. Default is 'Manual'",
        Required = false
    };

    public const string CapacityName = "capacity";

    public static readonly Option<int?> Capacity = new($"--{CapacityName}")
    {
        Description = "Number of VM instances (capacity) in the scale set",
        Required = false
    };

    // Additional VMSS update options
    public const string OverprovisionName = "overprovision";
    public const string EnableAutoOsUpgradeName = "enable-auto-os-upgrade";
    public const string ScaleInPolicyName = "scale-in-policy";
    public static readonly Option<bool?> Overprovision = new($"--{OverprovisionName}")
    {
        Description = "Enable or disable overprovisioning. When enabled, Azure provisions more VMs than requested and deletes extra VMs after deployment",
        Required = false
    };

    public static readonly Option<bool?> EnableAutoOsUpgrade = new($"--{EnableAutoOsUpgradeName}")
    {
        Description = "Enable automatic OS image upgrades. Requires health probes or Application Health extension",
        Required = false
    };

    public static readonly Option<string> ScaleInPolicy = new($"--{ScaleInPolicyName}")
    {
        Description = "Scale-in policy to determine which VMs to remove: 'Default', 'NewestVM', or 'OldestVM'",
        Required = false
    };

    // Power state options
    public const string PowerActionName = "power-action";
    public const string NoWaitName = "no-wait";
    public const string SkipShutdownName = "skip-shutdown";

    public static readonly Option<string> PowerAction = new($"--{PowerActionName}")
    {
        Description = "The power action to apply to the VM (not the current power state). Accepted values: start, stop, deallocate, restart.",
        Required = false
    };

    public static readonly Option<bool> NoWait = new($"--{NoWaitName}")
    {
        Description = "Return immediately without waiting for the operation to complete.",
        Required = false
    };

    public static readonly Option<bool> SkipShutdown = new($"--{SkipShutdownName}")
    {
        Description = "Skip the graceful OS shutdown and force power off. Only compatible with the 'stop' state.",
        Required = false
    };

    // Delete options
    public const string ForceDeletionName = "force-deletion";

    public static readonly Option<bool> ForceDeletion = new($"--{ForceDeletionName}")
    {
        Description = "Force delete the resource even if it is in a running or failed state (passes forceDeletion=true to the Azure API)",
        Required = false
    };

    // VM update options
    public const string LicenseTypeName = "license-type";
    public const string BootDiagnosticsName = "boot-diagnostics";
    public const string UserDataName = "user-data";

    public static readonly Option<string> LicenseType = new($"--{LicenseTypeName}")
    {
        Description = "License type for Azure Hybrid Benefit: 'Windows_Server', 'Windows_Client', 'RHEL_BYOS', 'SLES_BYOS', or 'None' to disable",
        Required = false
    };

    public static readonly Option<string> BootDiagnostics = new($"--{BootDiagnosticsName}")
    {
        Description = "Enable or disable boot diagnostics: 'true' or 'false'",
        Required = false
    };

    public static readonly Option<string> UserData = new($"--{UserDataName}")
    {
        Description = "Base64-encoded user data for the VM. Use to update custom data scripts",
        Required = false
    };

    // Guided-create discovery options (vm sku list, image list, quota check, region recommend)
    public const string MinVCpusName = "min-vcpus";
    public const string MinMemoryGbName = "min-memory-gb";
    public const string FamilyPrefixName = "family-prefix";
    public const string TopName = "top";
    public const string IncludePricingName = "include-pricing";
    public const string AliasName = "alias";
    public const string PublisherName = "publisher";
    public const string OfferName = "offer";
    public const string ImageSkuName = "image-sku";
    public const string IncludeSharedGalleryName = "include-shared-gallery";
    public const string RequestedVCpusName = "requested-vcpus";
    public const string WorkloadHintName = "workload-hint";
    public const string GeographyPreferenceName = "geography-preference";
    public const string RequireAvailabilityZonesName = "require-availability-zones";
    public const string SingleInstanceName = "single-instance";

    public static readonly Option<int?> MinVCpus = new($"--{MinVCpusName}")
    {
        Description = "Minimum number of vCPUs the SKU must have. Use to filter the SKU catalog down to candidates that meet your workload's CPU floor.",
        Required = false
    };

    public static readonly Option<double?> MinMemoryGb = new($"--{MinMemoryGbName}")
    {
        Description = "Minimum amount of memory (in GB) the SKU must have. Use to filter the SKU catalog down to candidates that meet your workload's memory floor.",
        Required = false
    };

    public static readonly Option<string> FamilyPrefix = new($"--{FamilyPrefixName}")
    {
        Description = "Prefix to filter SKUs by family or name (case-insensitive). Examples: 'Standard_D' for general-purpose, 'Standard_N' for GPU, 'Standard_E' for memory-optimized, 'Standard_F' for compute-optimized, 'Standard_B' for burstable.",
        Required = false
    };

    public static readonly Option<int?> Top = new($"--{TopName}")
    {
        Description = "Maximum number of results to return. Defaults to 20 for list-skus (SKU lists are large; raise explicitly if you need more) and 50 for list-images.",
        Required = false
    };

    public static readonly Option<bool> IncludePricing = new($"--{IncludePricingName}")
    {
        Description = "If true, augment each SKU with pay-as-you-go and spot retail hourly prices from the Azure Retail Prices API. Adds network latency; the API is unauthenticated.",
        Required = false
    };

    public static readonly Option<string> Alias = new($"--{AliasName}")
    {
        Description = "VM image alias to resolve (e.g., 'Ubuntu2404', 'Win2022Datacenter'). Returns the marketplace URN that alias currently maps to.",
        Required = false
    };

    public static readonly Option<string> Publisher = new($"--{PublisherName}")
    {
        Description = "Marketplace image publisher (e.g., 'Canonical', 'MicrosoftWindowsServer'). Pair with --offer and optional --image-sku.",
        Required = false
    };

    public static readonly Option<string> Offer = new($"--{OfferName}")
    {
        Description = "Marketplace image offer (e.g., 'ubuntu-24_04-lts', 'WindowsServer2022'). Pair with --publisher.",
        Required = false
    };

    public static readonly Option<string> ImageSku = new($"--{ImageSkuName}")
    {
        Description = "Marketplace image SKU (e.g., 'server', '2022-datacenter-azure-edition'). Pair with --publisher and --offer.",
        Required = false
    };

    public static readonly Option<bool> IncludeSharedGallery = new($"--{IncludeSharedGalleryName}")
    {
        Description = "If true, also enumerate image versions from Azure Compute Gallery (Shared Image Gallery) that this subscription has access to. Results carry source='sharedGallery' and a /sharedGalleries/.../images/.../versions/... resource ID in the urn field, which the create commands accept via --image. Off by default to keep the common marketplace listing fast.",
        Required = false
    };

    public static readonly Option<int?> RequestedVCpus = new($"--{RequestedVCpusName}")
    {
        Description = "Number of vCPUs you intend to deploy. Used to flag insufficient quota before a create is attempted.",
        Required = false
    };

    public static readonly Option<string> WorkloadHint = new($"--{WorkloadHintName}")
    {
        Description = "Free-form workload hint to rank regions (e.g., 'gpu training', 'low latency for europe users', 'general dev/test'). Used by compute_vm_region_recommend.",
        Required = false
    };

    public static readonly Option<string> GeographyPreference = new($"--{GeographyPreferenceName}")
    {
        Description = "Preferred geography substring used to bias region ranking (e.g., 'us', 'europe', 'asia'). Case-insensitive.",
        Required = false
    };

    public static readonly Option<bool> RequireAvailabilityZones = new($"--{RequireAvailabilityZonesName}")
    {
        Description = "If true, only recommend regions that support multiple Availability Zones.",
        Required = false
    };

    public static readonly Option<bool> SingleInstance = new($"--{SingleInstanceName}")
    {
        Description = "If true, provision a single non-scalable VM instead of a VMSS Flex scale set. Use this only when the workload can never scale out, never needs zonal spread, and never needs rolling upgrades. Off by default — the unified compute_create defaults to VMSS Flex (Flex with InstanceCount=1 is supported and preferred over a standalone VM for everything else).",
        Required = false
    };
}
