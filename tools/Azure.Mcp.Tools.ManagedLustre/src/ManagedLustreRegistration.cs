// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ManagedLustre;

public sealed class ManagedLustreRegistration : IAreaRegistration
{
    public static string AreaName => "managedlustre";

    public static string AreaTitle => "Azure Managed Lustre";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Managed Lustre operations - Commands for creating, updating, listing and inspecting Azure Managed Lustre file systems (AMLFS) used for high-performance computing workloads. The tool focuses on managing all the aspects related to Azure Managed Lustre file system instances.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "fs",
                Description = "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "814acadf-ee84-47f9-ad68-2d65ec7dbb07",
                        Name = "create",
                        Description = "Create an Azure Managed Lustre (AMLFS) file system using the specified network, capacity, maintenance window and availability zone. Optionally provides possibility to define Blob Integration, customer managed key encryption and root squash configuration.",
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
                                Name = "name",
                                Description = "The AMLFS resource name. Must be DNS-friendly (letters, numbers, hyphens). Example: --name amlfs-001",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "Azure region/region short name (use Azure location token, lowercase). Examples: uaenorth, swedencentral, eastus.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sku",
                                Description = "The AMLFS SKU. Exact allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "size",
                                Description = "The AMLFS size in TiB as an integer (no unit). Examples: 4, 12, 128.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "subnet-id",
                                Description = "Full subnet resource ID. Required format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}. Example: --subnet-id /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Network/virtualNetworks/vnet-001/subnets/subnet-001",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "zone",
                                Description = "Availability zone identifier. Use a single digit string matching the region's AZ labels (e.g. '1'). Example: --zone 1",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "maintenance-day",
                                Description = "Preferred maintenance day. Allowed values: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "maintenance-time",
                                Description = "Preferred maintenance time in UTC. Format: HH:MM (24-hour). Examples: 00:00, 23:00.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "hsm-container",
                                Description = "Full blob container resource ID for HSM integration. HPC Cache Resource Provider must have before deployment Storage Blob Data Contributor and Storage Account Contributor roles on parent Storage Account.Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Storage/storageAccounts/{account}/blobServices/default/containers/{container}. Example: --hsm-container /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stacc/blobServices/default/containers/hsm-container",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "hsm-log-container",
                                Description = "Full blob container resource ID for HSM logging. HPC Cache Resource Provider must have before deployment Storage Blob Data Contributor and Storage Account Contributor roles on parent Storage Account. Same format as --hsm-container. Example: --hsm-log-container /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stacc/blobServices/default/containers/hsm-logs",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "import-prefix",
                                Description = "Optional HSM import prefix (path prefix inside the container starting with /). Examples: '/ingest/', '/archive/2019/'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "root-squash-mode",
                                Description = "Root squash mode. Allowed values: All, RootOnly, None.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "no-squash-nid-list",
                                Description = "Comma-separated list of NIDs (network identifiers) not to squash. Example: '10.0.2.4@tcp;10.0.2.[6-8]@tcp'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "squash-uid",
                                Description = "Numeric UID to squash root to. Required in case root squash mode is not None. Example: --squash-uid 1000.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "squash-gid",
                                Description = "Numeric GID to squash root to. Required in case root squash mode is not None. Example: --squash-gid 1000.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "custom-encryption",
                                Description = "Enable customer-managed encryption using a Key Vault key. When true, --key-url and --source-vault required, with a user-assigned identity already configured for Key Vault key access.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "key-url",
                                Description = "Full Key Vault key URL. Format: https://{vaultName}.vault.azure.net/keys/{keyName}/{keyVersion}. Example: --key-url https://kv-amlfs-001.vault.azure.net/keys/key-amlfs-001/a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "source-vault",
                                Description = "Full Key Vault resource ID. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.KeyVault/vaults/{vaultName}. Example: --source-vault /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv-amlfs-001",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "user-assigned-identity-id",
                                Description = "User-assigned managed identity resource ID (full resource ID) to use for Key Vault access when custom encryption is enabled. The identity must have RBAC role to access the encryption key Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{name}. Example: --user-assigned-identity-id /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity1",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileSystemCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "723d9b34-9022-486e-83a7-f72d83bdafd2",
                        Name = "list",
                        Description = "Lists Azure Managed Lustre (AMLFS) file systems in a subscription or optional resource group including provisioning state, MGS address, tier, capacity (TiB), blob integration container, and maintenance window details. Use to inventory Azure Managed Lustre filesystems and to check their properties.",
                        Title = "List",
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
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileSystemListCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "db1bdf99-ac8a-4920-ab2e-15048623b2dc",
                        Name = "update",
                        Description = "Update maintenance window and/or root squash settings of an existing Azure Managed Lustre (AMLFS) file system. Provide either maintenance day and time or root squash fields (no-squash-nid-list, squash-uid, squash-gid). Root squash fields must be provided if root squash is not None. In case of maintenance window update, both maintenance day and maintenance time should be provided.",
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
                                Name = "name",
                                Description = "The AMLFS resource name. Must be DNS-friendly (letters, numbers, hyphens). Example: --name amlfs-001",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "maintenance-day",
                                Description = "Preferred maintenance day. Allowed values: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "maintenance-time",
                                Description = "Preferred maintenance time in UTC. Format: HH:MM (24-hour). Examples: 00:00, 23:00.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "no-squash-nid-list",
                                Description = "Comma-separated list of NIDs (network identifiers) not to squash. Example: '10.0.2.4@tcp;10.0.2.[6-8]@tcp'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "squash-uid",
                                Description = "Numeric UID to squash root to. Required in case root squash mode is not None. Example: --squash-uid 1000.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "squash-gid",
                                Description = "Numeric GID to squash root to. Required in case root squash mode is not None. Example: --squash-gid 1000.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "root-squash-mode",
                                Description = "Root squash mode. Allowed values: All, RootOnly, None.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileSystemUpdateCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "blob",
                        Description = "blob operations.",
                        SubGroups =
                        [
                            new CommandGroupDescriptor
                            {
                                Name = "autoexport",
                                Description = "autoexport operations.",
                                Commands =
                                [
                                    new CommandDescriptor
                                    {
                                        Id = "8e2f6d1b-3c9a-4f7e-b2d5-7a8c3e4f5b6d",
                                        Name = "cancel",
                                        Description = "Cancels a running auto export job for an Azure Managed Lustre filesystem. This stops the ongoing sync operation from the Lustre filesystem to the linked blob storage container. Use this to terminate an autoexport job that is in progress. Required options: - filesystem-name: The name of the AMLFS filesystem - job-name: The name of the autoexport job to cancel - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem",
                                        Title = "Cancel",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(AutoexportJobCancelCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "9f3e7c2a-4b8d-4e5f-a1c6-8d9e2f3b4a5c",
                                        Name = "create",
                                        Description = "Creates an auto export job for an Azure Managed Lustre filesystem to continuously export modified files to the linked blob storage container. The auto export job syncs changes from the Lustre filesystem to the configured HSM blob container. Use this to keep blob storage updated with changes in the filesystem. Required options: - filesystem-name: The name of the AMLFS filesystem - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport job. If not specified, a timestamped name will be generated.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "autoexport-prefix",
                                                Description = "Blob path/prefix that gets auto exported from the cluster namespace. Default: '/'. Note: Only 1 prefix is supported for autoexport jobs. Example: --autoexport-prefix /data",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "admin-status",
                                                Description = "The administrative status of the auto import job. Enable: job is active. Disable: disables the current active auto import job. Default: Enable. Allowed values: Enable, Disable.",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(FileSystemCreateCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "4c7a8e3d-9f2b-5a6e-c1d4-8b3e9a2f7c5d",
                                        Name = "delete",
                                        Description = "Deletes an auto export job for an Azure Managed Lustre filesystem. This permanently removes the job record from the filesystem. Use this to clean up completed, failed, or cancelled autoexport jobs. Required options: - filesystem-name: The name of the AMLFS filesystem - job-name: The name of the autoexport job to delete - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem",
                                        Title = "Delete",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(AutoexportJobDeleteCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "9a3b7e2f-4d6c-8a1e-b5f3-2c7d8e9a1b4f",
                                        Name = "get",
                                        Description = "Gets the details of auto export jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, configuration, and progress information of autoexport operations that sync data from the Lustre filesystem to the linked blob storage container. If job-name is provided, returns details of a specific job; otherwise returns all jobs for the filesystem. Required options: - filesystem-name: The name of the AMLFS filesystem - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem Optional options: - job-name: The name of a specific autoexport job (if omitted, all jobs are returned)",
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
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(SkuGetCommand)
                                    },
                                ],
                            },
                            new CommandGroupDescriptor
                            {
                                Name = "autoimport",
                                Description = "autoimport operations.",
                                Commands =
                                [
                                    new CommandDescriptor
                                    {
                                        Id = "9f3g1h2i-4d0b-5g8f-c3e6-8b9d4f6g7c8h",
                                        Name = "cancel",
                                        Description = "Cancels a running auto import job for an Azure Managed Lustre filesystem. This stops the ongoing sync operation from the linked blob storage container to the Lustre filesystem. Use this to terminate an autoimport job that is in progress. Required options: - filesystem-name: The name of the AMLFS filesystem - job-name: The name of the autoimport job to cancel - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem",
                                        Title = "Cancel",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(AutoexportJobCancelCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
                                        Name = "create",
                                        Description = "Creates an auto import job for an Azure Managed Lustre filesystem to continuously import new or modified files from the linked blob storage container. The auto import job syncs changes from the configured HSM blob container to the Lustre filesystem. Use this to keep the filesystem updated with changes in blob storage. Required options: - filesystem-name: The name of the AMLFS filesystem - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem Optional parameters: - job-name: Custom name for the job (default: autoimport-{timestamp}) - conflict-resolution-mode: How to handle conflicts (Fail/Skip/OverwriteIfDirty/OverwriteAlways, default: Skip) - autoimport-prefixes: Array of blob paths/prefixes to auto import (default: '/', max: 100) - admin-status: Administrative status (Enable/Disable, default: Enable) - enable-deletions: Enable deletions during auto import (default: false) - maximum-errors: Max errors before failure (-1: infinite, 0: immediate exit, default: none)",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoimport job. If not specified, a timestamped name will be generated.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "conflict-resolution-mode",
                                                Description = "How the auto import job handles conflicts. Fail: stop immediately on conflict. Skip: pass over the conflict. OverwriteIfDirty: delete and re-import if conflicting type, dirty, or currently released. OverwriteAlways: extends OverwriteIfDirty to include releasing restored but not dirty files. Default: Skip. Allowed values: Fail, Skip, OverwriteIfDirty, OverwriteAlways.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "autoimport-prefixes",
                                                Description = "Array of blob paths/prefixes that get auto imported to the cluster namespace. Default: '/'. Maximum: 100 paths. Example: --autoimport-prefixes /data --autoimport-prefixes /logs",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "admin-status",
                                                Description = "The administrative status of the auto import job. Enable: job is active. Disable: disables the current active auto import job. Default: Enable. Allowed values: Enable, Disable.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "enable-deletions",
                                                Description = "Whether to enable deletions during auto import. This only affects overwrite-dirty mode. Default: false.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "maximum-errors",
                                                Description = "Total non-conflict-oriented errors (e.g., OS errors) that import will tolerate before exiting with failure. -1: infinite. 0: exit immediately on any error.",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(FileSystemCreateCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "0h4i2j3k-5e1c-6h9g-d4f7-9c0e5g7h8d9i",
                                        Name = "delete",
                                        Description = "Deletes an auto import job for an Azure Managed Lustre filesystem. This permanently removes the job record from the filesystem. Use this to clean up completed, failed, or cancelled autoimport jobs. Required options: - filesystem-name: The name of the AMLFS filesystem - job-name: The name of the autoimport job to delete - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem",
                                        Title = "Delete",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(AutoexportJobDeleteCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
                                        Name = "get",
                                        Description = "Gets the details of auto import jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, configuration, and progress information of autoimport operations that sync data from the linked blob storage container to the Lustre filesystem. If job-name is provided, returns details of a specific job; otherwise returns all jobs for the filesystem. Required options: - filesystem-name: The name of the AMLFS filesystem - resource-group: The resource group containing the filesystem - subscription: The subscription containing the filesystem Optional options: - job-name: The name of a specific autoimport job (if omitted, all jobs are returned)",
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
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(SkuGetCommand)
                                    },
                                ],
                            },
                            new CommandGroupDescriptor
                            {
                                Name = "import",
                                Description = "import operations.",
                                Commands =
                                [
                                    new CommandDescriptor
                                    {
                                        Id = "d3h5e7g9-1f4a-6d8e-0g2c-4f6a8d0f2e4g",
                                        Name = "cancel",
                                        Description = "Cancels a running import job for an Azure Managed Lustre filesystem. This stops the import operation and prevents further processing. The job cannot be resumed after cancellation. Required options: - filesystem-name: The name of the AMLFS filesystem - job-name: Name of the import job to cancel",
                                        Title = "Cancel",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(AutoexportJobCancelCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "b1f3c5e7-9d2a-4b8f-6c3e-1a7b9d2f5e8c",
                                        Name = "create",
                                        Description = "Creates a one-time import job for an Azure Managed Lustre filesystem to import files from the linked blob storage container. The import job performs a one-time sync of data from the configured HSM blob container to the Lustre filesystem. Use this to import specific prefixes or all data from blob storage into the filesystem at a point in time. Required options: - filesystem-name: The name of the AMLFS filesystem Optional options: - job-name: Name for the import job (auto-generated if not provided) - conflict-resolution-mode: How to handle conflicting files (Fail, Skip, OverwriteIfDirty, OverwriteAlways, default: Fail) - import-prefixes: Blob prefixes to import (default: imports all data from root '/') - maximum-errors: Maximum errors allowed before job failure (-1: infinite, 0: fail on first error, default: use service default)",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoimport job. If not specified, a timestamped name will be generated.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "conflict-resolution-mode",
                                                Description = "How the auto import job handles conflicts. Fail: stop immediately on conflict. Skip: pass over the conflict. OverwriteIfDirty: delete and re-import if conflicting type, dirty, or currently released. OverwriteAlways: extends OverwriteIfDirty to include releasing restored but not dirty files. Default: Skip. Allowed values: Fail, Skip, OverwriteIfDirty, OverwriteAlways.",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "import-prefixes",
                                                Description = "Array of blob paths/prefixes to import from blob storage. Default: '/'. Maximum: 100 paths. Example: --import-prefixes /data --import-prefixes /logs",
                                                TypeName = "string",
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "maximum-errors",
                                                Description = "Total non-conflict-oriented errors (e.g., OS errors) that import will tolerate before exiting with failure. -1: infinite. 0: exit immediately on any error.",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(FileSystemCreateCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "e4i6f8h0-2g5b-7e9f-1h3d-5g7b9e1g3f5h",
                                        Name = "delete",
                                        Description = "Deletes an import job for an Azure Managed Lustre filesystem. This removes the job record and history. The job must be completed or cancelled before it can be deleted. Required options: - filesystem-name: The name of the AMLFS filesystem - job-name: Name of the import job to delete",
                                        Title = "Delete",
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
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoexport/autoimport job",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(AutoexportJobDeleteCommand)
                                    },
                                    new CommandDescriptor
                                    {
                                        Id = "c2g4d6f8-0e3a-5c7d-9f1b-3e5a7c9f1d3f",
                                        Name = "get",
                                        Description = "Gets import job details or lists all import jobs for an Azure Managed Lustre filesystem. If job-name is provided, returns details for that specific job. If job-name is omitted, returns a list of all import jobs for the filesystem. Required options: - filesystem-name: The name of the AMLFS filesystem Optional options: - job-name: Name of specific import job to get (omit to list all jobs)",
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
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "filesystem-name",
                                                Description = "The name of the Azure Managed Lustre filesystem",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "job-name",
                                                Description = "The name of the autoimport job. If not specified, a timestamped name will be generated.",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(SkuGetCommand)
                                    },
                                ],
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "sku",
                        Description = "This group provides commands to discover and retrieve information about available Azure Managed Lustre SKUs, including supported tiers, performance characteristics, and regional availability. Use these commands to validate SKU options prior to provisioning or updating a filesystem.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "43f679ba-1b6e-4851-9315-f8ad16b789e5",
                                Name = "get",
                                Description = "Retrieves the available Azure Managed Lustre SKU, including increments, bandwidth, scale targets and zonal support. If a location is specified, the results will be filtered to that location.",
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
                                        Name = "location",
                                        Description = "Azure region/region short name (use Azure location token, lowercase). Examples: uaenorth, swedencentral, eastus.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(SkuGetCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "subnetsize",
                        Description = "Subnet size planning and validation operations for Azure Managed Lustre.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "3d3f6f27-218b-4915-9c1e-243dd53b16da",
                                Name = "ask",
                                Description = "Calculates the required subnet size for an Azure Managed Lustre file system given a SKU and size. Use to plan network deployment for AMLFS. Returns the number of required IPs.",
                                Title = "Ask",
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
                                        Name = "sku",
                                        Description = "The AMLFS SKU. Exact allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "size",
                                        Description = "The AMLFS size in TiB as an integer (no unit). Examples: 4, 12, 128.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(SubnetSizeAskCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "b6317bba-e28c-445b-9133-9cfbfe677698",
                                Name = "validate",
                                Description = "Validates that the provided subnet can host an Azure Managed Lustre filesystem for the given SKU and size.",
                                Title = "Validate",
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
                                        Name = "sku",
                                        Description = "The AMLFS SKU. Exact allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "size",
                                        Description = "The AMLFS size in TiB as an integer (no unit). Examples: 4, 12, 128.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "subnet-id",
                                        Description = "Full subnet resource ID. Required format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}. Example: --subnet-id /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Network/virtualNetworks/vnet-001/subnets/subnet-001",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "location",
                                        Description = "Azure region/region short name (use Azure location token, lowercase). Examples: uaenorth, swedencentral, eastus.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(SubnetSizeValidateCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IManagedLustreService, ManagedLustreService>();
        services.AddSingleton<FileSystemListCommand>();
        services.AddSingleton<FileSystemCreateCommand>();
        services.AddSingleton<FileSystemUpdateCommand>();
        services.AddSingleton<SubnetSizeAskCommand>();
        services.AddSingleton<SubnetSizeValidateCommand>();
        services.AddSingleton<SkuGetCommand>();
        services.AddSingleton<AutoexportJobCreateCommand>();
        services.AddSingleton<AutoexportJobCancelCommand>();
        services.AddSingleton<AutoexportJobGetCommand>();
        services.AddSingleton<AutoexportJobDeleteCommand>();
        services.AddSingleton<AutoimportJobCreateCommand>();
        services.AddSingleton<AutoimportJobCancelCommand>();
        services.AddSingleton<AutoimportJobGetCommand>();
        services.AddSingleton<AutoimportJobDeleteCommand>();
        services.AddSingleton<ImportJobCreateCommand>();
        services.AddSingleton<ImportJobCancelCommand>();
        services.AddSingleton<ImportJobGetCommand>();
        services.AddSingleton<ImportJobDeleteCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(FileSystemCreateCommand) => serviceProvider.GetRequiredService<FileSystemCreateCommand>(),
            nameof(FileSystemListCommand) => serviceProvider.GetRequiredService<FileSystemListCommand>(),
            nameof(FileSystemUpdateCommand) => serviceProvider.GetRequiredService<FileSystemUpdateCommand>(),
            nameof(AutoexportJobCancelCommand) => serviceProvider.GetRequiredService<AutoexportJobCancelCommand>(),
            nameof(AutoexportJobDeleteCommand) => serviceProvider.GetRequiredService<AutoexportJobDeleteCommand>(),
            nameof(SkuGetCommand) => serviceProvider.GetRequiredService<SkuGetCommand>(),
            nameof(SubnetSizeAskCommand) => serviceProvider.GetRequiredService<SubnetSizeAskCommand>(),
            nameof(SubnetSizeValidateCommand) => serviceProvider.GetRequiredService<SubnetSizeValidateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in managedlustre area.")
        };
}
