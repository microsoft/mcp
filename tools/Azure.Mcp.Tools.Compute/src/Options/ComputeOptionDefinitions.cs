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
}
