// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options;

internal static class ComputeOptionDescriptions
{
    internal const string DiskName = "The name of the disk.";
    internal const string SizeGb = "Size of the disk in GB. Max size: 4095 GB.";
    internal const string Sku = "Underlying storage SKU. Accepted values: Premium_LRS, PremiumV2_LRS, Premium_ZRS, StandardSSD_LRS, StandardSSD_ZRS, Standard_LRS, UltraSSD_LRS.";
    internal const string OsType = "Operating System type of the disk. Accepted values: Linux, Windows.";
    internal const string Zone = "Availability zone into which to provision the resource.";
    internal const string DiskIopsReadWrite = "The number of IOPS allowed for this disk. Only settable for UltraSSD disks.";
    internal const string DiskMbpsReadWrite = "The bandwidth allowed for this disk in MBps. Only settable for UltraSSD disks.";
    internal const string MaxShares = "The maximum number of VMs that can attach to the disk at the same time. Value greater than one indicates a shared disk.";
    internal const string NetworkAccessPolicy = "The network access policy for the disk via network. Accepted values: AllowAll, AllowPrivate, DenyAll.";
    internal const string EnableBursting = "Enable on-demand bursting beyond the provisioned performance target of the disk. Does not apply to Ultra disks.";
    internal const string Tags = "Comma-separated tags in 'key=value' format (e.g., 'env=prod,team=compute').";
    internal const string TagsUpdate = Tags + " Use '' to clear all existing tags.";
    internal const string DiskEncryptionSet = "Resource ID of the disk encryption set to use for enabling encryption at rest.";
    internal const string EncryptionType = "Encryption type of the disk. Accepted values: EncryptionAtRestWithCustomerKey, EncryptionAtRestWithPlatformAndCustomerKeys, EncryptionAtRestWithPlatformKey.";
    internal const string DiskAccess = "Resource ID of the disk access resource for using private endpoints on disks.";
    internal const string Tier = "Performance tier of the disk (e.g., P10, P15, P20, P30, P40, P50, P60, P70, P80). Applicable to Premium SSD disks only.";
    internal const string VmName = "The name of the virtual machine.";
    internal const string VmssName = "The name of the virtual machine scale set.";
    internal const string Location = "The Azure region/location. Defaults to the resource group's location if not specified.";
    internal const string VmSize = "The VM size (e.g., Standard_D2s_v3, Standard_B2s). Defaults to Standard_D2s_v5 if not specified.";
    internal const string Image = "The OS image to use. Can be a URN (publisher:offer:sku:version), a shared gallery image ID (starting with '/sharedGalleries/'), or an alias such as 'Ubuntu2404' or 'Win2022Datacenter'.";
    internal const string AdminUsername = "The admin username for the VM. Required for VM creation.";
    internal const string AdminPassword = "The admin password for Windows VMs or when SSH key is not provided for Linux VMs.";
    internal const string SshPublicKey = "SSH public key for Linux VMs. Can be the key content or path to a file.";
    internal const string VirtualNetwork = "Name of an existing virtual network to use. If not specified, a new one will be created.";
    internal const string Subnet = "Name of the subnet within the virtual network.";
    internal const string OsDiskSizeGb = "OS disk size in GB. Defaults based on image requirements.";
    internal const string OsDiskType = "OS disk type: 'Premium_LRS', 'StandardSSD_LRS', 'Standard_LRS'. Defaults based on VM size.";

    // VMSS-specific options
    internal const string UpgradePolicy = "Upgrade policy mode: 'Automatic', 'Manual', or 'Rolling'.";

    // Delete options
    internal const string ForceDeletion = "Force delete the resource even if it is in a running or failed state (passes forceDeletion=true to the Azure API)";
}
