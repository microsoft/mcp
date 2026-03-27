// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Compute;

public sealed class ComputeRegistration : IAreaRegistration
{
    public static string AreaName => "compute";

    public static string AreaTitle => "Manage Azure Compute Resources";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Manage Azure Compute Resources operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "disk",
                Description = "Managed Disk operations - Get details about Azure managed disks in your subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "3f8a1b2c-5d6e-4a7b-8c9d-0e1f2a3b4c5d",
                        Name = "create",
                        Description = "Creates a new Azure managed disk in the specified resource group. Supports creating empty disks (specify --size-gb), disks from a source such as a snapshot, another managed disk, or a blob URI (specify --source), disks from a Shared Image Gallery image version (specify --gallery-image-reference), or disks ready for upload (specify --upload-type and --upload-size-bytes). If location is not specified, defaults to the resource group's location. Supports configuring disk size, storage SKU (e.g., Premium_LRS, Standard_LRS, UltraSSD_LRS), OS type, availability zone, hypervisor generation, tags, encryption settings, performance tier, shared disk, on-demand bursting, and IOPS/throughput limits for UltraSSD disks. Create a disk with network access policy DenyAll, AllowAll, or AllowPrivate and associate a disk access resource during creation.",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-name",
                                Description = "The name of the disk",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "source",
                                Description = "Source to create the disk from, including a resource ID of a snapshot or disk, or a blob URI of a VHD. When a source is provided, --size-gb is optional and defaults to the source size.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region/location. Defaults to the resource group's location if not specified.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "size-gb",
                                Description = "Size of the disk in GB. Max size: 4095 GB.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku",
                                Description = "Underlying storage SKU. Accepted values: Premium_LRS, PremiumV2_LRS, Premium_ZRS, StandardSSD_LRS, StandardSSD_ZRS, Standard_LRS, UltraSSD_LRS.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-type",
                                Description = "The Operating System type of the disk. Accepted values: Linux, Windows.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "zone",
                                Description = "Availability zone into which to provision the resource.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "hyper-v-generation",
                                Description = "The hypervisor generation of the Virtual Machine. Applicable to OS disks only. Accepted values: V1, V2.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "max-shares",
                                Description = "The maximum number of VMs that can attach to the disk at the same time. Value greater than one indicates a shared disk.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "network-access-policy",
                                Description = "Policy for accessing the disk via network. Accepted values: AllowAll, AllowPrivate, DenyAll.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "enable-bursting",
                                Description = "Enable on-demand bursting beyond the provisioned performance target of the disk. Does not apply to Ultra disks. Accepted values: true, false.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Space-separated tags in 'key=value' format. Use '' to clear existing tags.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-encryption-set",
                                Description = "Resource ID of the disk encryption set to use for enabling encryption at rest.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "encryption-type",
                                Description = "Encryption type of the disk. Accepted values: EncryptionAtRestWithCustomerKey, EncryptionAtRestWithPlatformAndCustomerKeys, EncryptionAtRestWithPlatformKey.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-access",
                                Description = "Resource ID of the disk access resource for using private endpoints on disks.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tier",
                                Description = "Performance tier of the disk (e.g., P10, P15, P20, P30, P40, P50, P60, P70, P80). Applicable to Premium SSD disks only.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "gallery-image-reference",
                                Description = "Resource ID of a Shared Image Gallery image version to use as the source for the disk. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/galleries/{gallery}/images/{image}/versions/{version}.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "gallery-image-reference-lun",
                                Description = "LUN (Logical Unit Number) of the data disk in the gallery image version. If specified, the disk is created from the data disk at this LUN. If not specified, the disk is created from the OS disk of the image.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-iops-read-write",
                                Description = "The number of IOPS allowed for this disk. Only settable for UltraSSD disks.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-mbps-read-write",
                                Description = "The bandwidth allowed for this disk in MBps. Only settable for UltraSSD disks.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "upload-type",
                                Description = "Type of upload for the disk. Accepted values: Upload, UploadWithSecurityData. When specified, the disk is created in a ReadyToUpload state.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "upload-size-bytes",
                                Description = "The size in bytes (including the VHD footer of 512 bytes) of the content to be uploaded. Required when --upload-type is specified.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "security-type",
                                Description = "Security type of the managed disk. Accepted values: ConfidentialVM_DiskEncryptedWithCustomerKey, ConfidentialVM_DiskEncryptedWithPlatformKey, ConfidentialVM_VMGuestStateOnlyEncryptedWithPlatformKey, Standard, TrustedLaunch. Required when --upload-type is UploadWithSecurityData.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "a7c3e9f1-4b82-4d5a-9e6c-1f3d8b2a7c4e",
                        Name = "delete",
                        Description = "Deletes an Azure managed disk from the specified resource group. This is an idempotent operation that returns Deleted = true if the disk was successfully removed, or Deleted = false if the disk was not found. The disk must not be attached to a virtual machine; detach it first before deleting.",
                        Title = "Delete",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-name",
                                Description = "The name of the disk",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "01ab6f7e-2b27-4d6e-b0cc-b29043efac8e",
                        Name = "get",
                        Description = "Lists available Azure managed disks or retrieves detailed information about a specific disk. Shows all disks in a subscription or resource group, including disk size, SKU, provisioning state, and OS type. Supports wildcard patterns in disk names (e.g., 'win_OsDisk*'). When disk name is provided without resource group, searches across the entire subscription. When resource group is specified, scopes the search to that resource group. Both parameters are optional.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-name",
                                Description = "The name of the disk",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "4a9b2c3d-6e7f-5b8c-9d0e-1f2a3b4c5d6e",
                        Name = "update",
                        Description = "Updates or modifies properties of an existing Azure managed disk that was previously created. If resource group is not specified, the disk is located by name within the subscription. Supports changing disk size (can only increase), storage SKU, IOPS and throughput limits (UltraSSD only), max shares for shared disk attachments, on-demand bursting, tags, encryption settings, disk access, and performance tier. Modify the network access policy to DenyAll, AllowAll, or AllowPrivate on an existing disk. Only specified properties are updated; unspecified properties remain unchanged.",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-name",
                                Description = "The name of the disk",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "size-gb",
                                Description = "Size of the disk in GB. Max size: 4095 GB.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku",
                                Description = "Underlying storage SKU. Accepted values: Premium_LRS, PremiumV2_LRS, Premium_ZRS, StandardSSD_LRS, StandardSSD_ZRS, Standard_LRS, UltraSSD_LRS.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-iops-read-write",
                                Description = "The number of IOPS allowed for this disk. Only settable for UltraSSD disks.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-mbps-read-write",
                                Description = "The bandwidth allowed for this disk in MBps. Only settable for UltraSSD disks.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "max-shares",
                                Description = "The maximum number of VMs that can attach to the disk at the same time. Value greater than one indicates a shared disk.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "network-access-policy",
                                Description = "Policy for accessing the disk via network. Accepted values: AllowAll, AllowPrivate, DenyAll.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "enable-bursting",
                                Description = "Enable on-demand bursting beyond the provisioned performance target of the disk. Does not apply to Ultra disks. Accepted values: true, false.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Space-separated tags in 'key=value' format. Use '' to clear existing tags.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-encryption-set",
                                Description = "Resource ID of the disk encryption set to use for enabling encryption at rest.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "encryption-type",
                                Description = "Encryption type of the disk. Accepted values: EncryptionAtRestWithCustomerKey, EncryptionAtRestWithPlatformAndCustomerKeys, EncryptionAtRestWithPlatformKey.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "disk-access",
                                Description = "Resource ID of the disk access resource for using private endpoints on disks.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tier",
                                Description = "Performance tier of the disk (e.g., P10, P15, P20, P30, P40, P50, P60, P70, P80). Applicable to Premium SSD disks only.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmUpdateCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "vm",
                Description = "Virtual Machine operations - Commands for managing and monitoring Azure Virtual Machines including lifecycle, status, creation, and size information.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b765ab9c-788d-4422-80aa-54488f6be648",
                        Name = "create",
                        Description = "Create, deploy, or provision a single Azure Virtual Machine (VM). Use this to launch a new Linux or Windows VM with SSH key or password authentication. Automatically creates networking resources (VNet, subnet, NSG, NIC, public IP) when not specified. Equivalent to 'az vm create'. Defaults to Standard_DS1_v2 size and Ubuntu 24.04 LTS if not specified. For Linux VMs with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content. Do not use this for creating Virtual Machine Scale Sets with multiple identical instances (use VMSS create instead).",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-name",
                                Description = "The name of the virtual machine",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region/location. Defaults to the resource group's location if not specified.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "admin-username",
                                Description = "The admin username for the VM. Required for VM creation",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "admin-password",
                                Description = "The admin password for Windows VMs or when SSH key is not provided for Linux VMs",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "ssh-public-key",
                                Description = "SSH public key for Linux VMs. Can be the key content or path to a file",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-size",
                                Description = "The VM size (e.g., Standard_D2s_v3, Standard_B2s). Defaults to Standard_DS1_v2 if not specified",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "image",
                                Description = "The OS image to use. Can be URN (publisher:offer:sku:version) or alias like 'Ubuntu2404', 'Win2022Datacenter'. Defaults to Ubuntu 24.04 LTS",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-type",
                                Description = "The Operating System type of the disk. Accepted values: Linux, Windows.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "virtual-network",
                                Description = "Name of an existing virtual network to use. If not specified, a new one will be created",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "subnet",
                                Description = "Name of the subnet within the virtual network",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "public-ip-address",
                                Description = "Name of the public IP address to use or create",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "network-security-group",
                                Description = "Name of the network security group to use or create",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "no-public-ip",
                                Description = "Do not create or assign a public IP address",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "source-address-prefix",
                                Description = "Source IP address range for NSG inbound rules (e.g., '203.0.113.0/24' or a specific IP). Defaults to '*' (any source)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "zone",
                                Description = "Availability zone into which to provision the resource.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-disk-size-gb",
                                Description = "OS disk size in GB. Defaults based on image requirements",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-disk-type",
                                Description = "OS disk type: 'Premium_LRS', 'StandardSSD_LRS', 'Standard_LRS'. Defaults based on VM size",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "d4e2c8a1-6f3b-4d9e-b8c7-1a2e3f4d5e6f",
                        Name = "delete",
                        Description = "Delete, remove, or destroy an Azure Virtual Machine (VM). Use this to permanently remove a VM that is no longer needed. Equivalent to 'az vm delete'. This operation is irreversible and the VM data will be lost. Use --force-deletion to force delete the VM even if it is in a running or failed state (passes forceDeletion=true to the Azure API). Associated resources like disks, NICs, and public IPs are NOT automatically deleted. Do not use this to delete Virtual Machine Scale Sets (use VMSS delete instead).",
                        Title = "Delete",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-name",
                                Description = "The name of the virtual machine",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "force-deletion",
                                Description = "Force delete the resource even if it is in a running or failed state (passes forceDeletion=true to the Azure API)",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c1a8b3e5-4f2d-4a6e-8c7b-9d2e3f4a5b6c",
                        Name = "get",
                        Description = "List or get Azure Virtual Machine (VM) configuration and properties in a resource group. By default, returns VM details including name, location, size, provisioning state, and OS type. When retrieving a specific VM with --vm-name and --instance-view, the response also includes power state (running/stopped/deallocated). Use this tool to retrieve VM configuration details.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-name",
                                Description = "The name of the virtual machine",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "instance-view",
                                Description = "Include instance view details (only available when retrieving a specific VM)",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "f330138e-8048-4a4a-8170-d8b6f958eaa4",
                        Name = "update",
                        Description = "Update, modify, or reconfigure an existing Azure Virtual Machine (VM). Use this to resize a VM, update tags, configure boot diagnostics, or change user data. Equivalent to 'az vm update'. The VM may need to be deallocated before resizing to certain sizes. Do not use this to create a new VM (use VM create) or to update Virtual Machine Scale Sets (use VMSS update).",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-name",
                                Description = "The name of the virtual machine",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-size",
                                Description = "The VM size (e.g., Standard_D2s_v3, Standard_B2s). Defaults to Standard_DS1_v2 if not specified",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Space-separated tags in 'key=value' format. Use '' to clear existing tags.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "license-type",
                                Description = "License type for Azure Hybrid Benefit: 'Windows_Server', 'Windows_Client', 'RHEL_BYOS', 'SLES_BYOS', or 'None' to disable",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "boot-diagnostics",
                                Description = "Enable or disable boot diagnostics: 'true' or 'false'",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "user-data",
                                Description = "Base64-encoded user data for the VM. Use to update custom data scripts",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmUpdateCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "vmss",
                Description = "Virtual Machine Scale Set operations - Commands for managing and monitoring Azure Virtual Machine Scale Sets including scale set details, instances, and rolling upgrades.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "c46a4bc5-cba6-4d99-991b-a9109fc689ad",
                        Name = "create",
                        Description = "Create, deploy, or provision an Azure Virtual Machine Scale Set (VMSS) for running multiple identical VM instances. Use this to deploy workloads that need horizontal scaling, load balancing, or high availability across instances. Equivalent to 'az vmss create'. Defaults to 2 instances, Standard_DS1_v2 size, and Ubuntu 24.04 LTS. For Linux VMSS with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content. Do not use this for creating a single standalone VM (use VM create instead).",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vmss-name",
                                Description = "The name of the virtual machine scale set",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region/location. Defaults to the resource group's location if not specified.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "admin-username",
                                Description = "The admin username for the VM. Required for VM creation",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "admin-password",
                                Description = "The admin password for Windows VMs or when SSH key is not provided for Linux VMs",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "ssh-public-key",
                                Description = "SSH public key for Linux VMs. Can be the key content or path to a file",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-size",
                                Description = "The VM size (e.g., Standard_D2s_v3, Standard_B2s). Defaults to Standard_DS1_v2 if not specified",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "image",
                                Description = "The OS image to use. Can be URN (publisher:offer:sku:version) or alias like 'Ubuntu2404', 'Win2022Datacenter'. Defaults to Ubuntu 24.04 LTS",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-type",
                                Description = "The Operating System type of the disk. Accepted values: Linux, Windows.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "instance-count",
                                Description = "Number of VM instances in the scale set. Default is 2",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "upgrade-policy",
                                Description = "Upgrade policy mode: 'Automatic', 'Manual', or 'Rolling'. Default is 'Manual'",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "virtual-network",
                                Description = "Name of an existing virtual network to use. If not specified, a new one will be created",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "subnet",
                                Description = "Name of the subnet within the virtual network",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "zone",
                                Description = "Availability zone into which to provision the resource.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-disk-size-gb",
                                Description = "OS disk size in GB. Defaults based on image requirements",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "os-disk-type",
                                Description = "OS disk type: 'Premium_LRS', 'StandardSSD_LRS', 'Standard_LRS'. Defaults based on VM size",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "e5f3d9b2-7a4c-4e8f-c9d8-2b3f4a5e6d7c",
                        Name = "delete",
                        Description = "Delete, remove, or destroy an Azure Virtual Machine Scale Set (VMSS) and all its VM instances. Use this to permanently remove a scale set that is no longer needed. Equivalent to 'az vmss delete'. This operation is irreversible and all VMSS instances will be lost. Use --force-deletion to force delete the VMSS even if it is in a running or failed state (passes forceDeletion=true to the Azure API). Do not use this to delete a single VM (use VM delete instead).",
                        Title = "Delete",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vmss-name",
                                Description = "The name of the virtual machine scale set",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "force-deletion",
                                Description = "Force delete the resource even if it is in a running or failed state (passes forceDeletion=true to the Azure API)",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "a5e2f7i9-8j6h-8e0i-2g1f-3h6i7j8e9f0g",
                        Name = "get",
                        Description = "List or get Azure Virtual Machine Scale Sets (VMSS) and their instances in a subscription or resource group. Returns scale set details including name, location, SKU, capacity, upgrade policy, and individual VM instance information.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "vmss-name",
                                Description = "The name of the virtual machine scale set",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "instance-id",
                                Description = "The instance ID of the virtual machine in the scale set",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "aaa0ad51-3c16-4ec2-99e2-b24f28a1e7d0",
                        Name = "update",
                        Description = "Update, modify, or reconfigure an existing Azure Virtual Machine Scale Set (VMSS). Use this to scale instance count, resize VMs, change upgrade policy, or update tags on a scale set. Equivalent to 'az vmss update'. Changes may require 'update-instances' to roll out to existing VMs. Do not use this to create a new VMSS (use VMSS create) or to update a single VM (use VM update).",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "vmss-name",
                                Description = "The name of the virtual machine scale set",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "upgrade-policy",
                                Description = "Upgrade policy mode: 'Automatic', 'Manual', or 'Rolling'. Default is 'Manual'",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "capacity",
                                Description = "Number of VM instances (capacity) in the scale set",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "vm-size",
                                Description = "The VM size (e.g., Standard_D2s_v3, Standard_B2s). Defaults to Standard_DS1_v2 if not specified",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "overprovision",
                                Description = "Enable or disable overprovisioning. When enabled, Azure provisions more VMs than requested and deletes extra VMs after deployment",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "enable-auto-os-upgrade",
                                Description = "Enable automatic OS image upgrades. Requires health probes or Application Health extension",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "scale-in-policy",
                                Description = "Scale-in policy to determine which VMs to remove: 'Default', 'NewestVM', or 'OldestVM'",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Space-separated tags in 'key=value' format. Use '' to clear existing tags.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(VmUpdateCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IComputeService, ComputeService>();
        // VM commands
        services.AddSingleton<VmGetCommand>();
        services.AddSingleton<VmCreateCommand>();
        services.AddSingleton<VmUpdateCommand>();
        services.AddSingleton<VmDeleteCommand>();
        // VMSS commands
        services.AddSingleton<VmssGetCommand>();
        services.AddSingleton<VmssCreateCommand>();
        services.AddSingleton<VmssUpdateCommand>();
        services.AddSingleton<VmssDeleteCommand>();
        // Disk commands
        services.AddSingleton<DiskCreateCommand>();
        services.AddSingleton<DiskDeleteCommand>();
        services.AddSingleton<DiskGetCommand>();
        services.AddSingleton<DiskUpdateCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(VmCreateCommand) => serviceProvider.GetRequiredService<VmCreateCommand>(),
            nameof(VmDeleteCommand) => serviceProvider.GetRequiredService<VmDeleteCommand>(),
            nameof(VmGetCommand) => serviceProvider.GetRequiredService<VmGetCommand>(),
            nameof(VmUpdateCommand) => serviceProvider.GetRequiredService<VmUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in compute area.")
        };
}
