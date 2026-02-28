// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options;

public static class ComputeOptionDefinitions
{
    public const string VmNameName = "vm-name";
    public const string VmssNameName = "vmss-name";
    public const string InstanceIdName = "instance-id";
    public const string LocationName = "location";
    public const string DiskName = "disk";
    public const string VmSizeName = "vm-size";
    public const string ImageName = "image";
    public const string AdminUsernameName = "admin-username";
    public const string AdminPasswordName = "admin-password";
    public const string SshPublicKeyName = "ssh-public-key";
    public const string WorkloadName = "workload";
    public const string OsTypeName = "os-type";
    public const string VirtualNetworkName = "virtual-network";
    public const string SubnetName = "subnet";
    public const string PublicIpAddressName = "public-ip-address";
    public const string NetworkSecurityGroupName = "network-security-group";
    public const string NoPublicIpName = "no-public-ip";
    public const string ZoneName = "zone";
    public const string OsDiskSizeGbName = "os-disk-size-gb";
    public const string OsDiskTypeName = "os-disk-type";

    public static readonly Option<string> Disk = new($"--{DiskName}", "--name")
    {
        Description = "The name of the disk",
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
        Description = "The Azure region/location",
        Required = true
    };

    public static readonly Option<string> VmSize = new($"--{VmSizeName}", "--size")
    {
        Description = "The VM size (e.g., Standard_D2s_v3, Standard_B2s). If not specified, will be determined based on workload",
        Required = false
    };

    public static readonly Option<string> Image = new($"--{ImageName}")
    {
        Description = "The OS image to use. Can be URN (publisher:offer:sku:version) or alias like 'Ubuntu2404', 'Win2022Datacenter'. Defaults to Ubuntu 24.04 LTS",
        Required = false
    };

    public static readonly Option<string> AdminUsername = new($"--{AdminUsernameName}")
    {
        Description = "The admin username for the VM. Required for VM creation",
        Required = false
    };

    public static readonly Option<string> AdminPassword = new($"--{AdminPasswordName}")
    {
        Description = "The admin password for Windows VMs or when SSH key is not provided for Linux VMs",
        Required = false
    };

    public static readonly Option<string> SshPublicKey = new($"--{SshPublicKeyName}")
    {
        Description = "SSH public key for Linux VMs. Can be the key content or path to a file",
        Required = false
    };

    public static readonly Option<string> Workload = new($"--{WorkloadName}", "-w")
    {
        Description = "The type of workload to run. Used to suggest appropriate VM size and configuration. Options: development, web, database, compute, memory, gpu, general",
        Required = false
    };

    public static readonly Option<string> OsType = new($"--{OsTypeName}")
    {
        Description = "The operating system type: 'linux' or 'windows'. Defaults to 'linux'",
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

    public static readonly Option<string> Zone = new($"--{ZoneName}", "-z")
    {
        Description = "Availability zone for the VM (e.g., '1', '2', '3')",
        Required = false
    };

    public static readonly Option<int> OsDiskSizeGb = new($"--{OsDiskSizeGbName}")
    {
        Description = "OS disk size in GB. Defaults based on image requirements",
        Required = false
    };

    public static readonly Option<string> OsDiskType = new($"--{OsDiskTypeName}")
    {
        Description = "OS disk type: 'Premium_LRS', 'StandardSSD_LRS', 'Standard_LRS'. Defaults based on VM size",
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

    public static readonly Option<int> Capacity = new($"--{CapacityName}")
    {
        Description = "Number of VM instances (capacity) in the scale set",
        Required = false
    };

    // Additional VMSS update options
    public const string OverprovisionName = "overprovision";
    public const string EnableAutoOsUpgradeName = "enable-auto-os-upgrade";
    public const string ScaleInPolicyName = "scale-in-policy";
    public const string TagsName = "tags";

    public static readonly Option<bool> Overprovision = new($"--{OverprovisionName}")
    {
        Description = "Enable or disable overprovisioning. When enabled, Azure provisions more VMs than requested and deletes extra VMs after deployment",
        Required = false
    };

    public static readonly Option<bool> EnableAutoOsUpgrade = new($"--{EnableAutoOsUpgradeName}")
    {
        Description = "Enable automatic OS image upgrades. Requires health probes or Application Health extension",
        Required = false
    };

    public static readonly Option<string> ScaleInPolicy = new($"--{ScaleInPolicyName}")
    {
        Description = "Scale-in policy to determine which VMs to remove: 'Default', 'NewestVM', or 'OldestVM'",
        Required = false
    };

    public static readonly Option<string> Tags = new($"--{TagsName}")
    {
        Description = "Resource tags in format 'key1=value1,key2=value2'. Use empty string to clear all tags",
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
}
