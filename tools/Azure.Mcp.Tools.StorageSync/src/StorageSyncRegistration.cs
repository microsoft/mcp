// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.StorageSync;

public sealed class StorageSyncRegistration : IAreaRegistration
{
    public static string AreaName => "storagesync";

    public static string AreaTitle => "Manage Azure Storage Sync Services";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Manage Azure Storage Sync Services operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "cloudendpoint",
                Description = "Cloud Endpoint operations - Create, get, delete, and manage cloud endpoints in your sync groups.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "96f096a2-d36f-4361-aa74-4e393e7f48a5",
                        Name = "changedetection",
                        Description = "Trigger change detection on a cloud endpoint to sync file changes.",
                        Title = "Changedetection",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cloud-endpoint-name",
                                Description = "The name of the cloud endpoint",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "directory-path",
                                Description = "Relative path to a directory on the Azure File share for which change detection is to be performed",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "change-detection-mode",
                                Description = "Change detection mode: 'Default' (directory only) or 'Recursive' (directory and subdirectories). Applies to the directory specified in directory-path",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "paths",
                                Description = "Array of relative paths on the Azure File share to be included in change detection. Can be files and directories",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(CloudEndpointTriggerChangeDetectionCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "df0d4ae3-519a-44f1-ad30-d25a0985e9c2",
                        Name = "create",
                        Description = "Add a cloud endpoint to a sync group by connecting an Azure File Share. Cloud endpoints represent the Azure storage side of the sync relationship.",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cloud-endpoint-name",
                                Description = "The name of the cloud endpoint",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "storage-account-resource-id",
                                Description = "The resource ID of the Azure storage account",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "azure-file-share-name",
                                Description = "The name of the Azure file share",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "f5e76906-cc2a-41a4-b4f9-498221aaaf2e",
                        Name = "delete",
                        Description = "Delete a cloud endpoint from a sync group.",
                        Title = "Delete",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cloud-endpoint-name",
                                Description = "The name of the cloud endpoint",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "25dd8bb3-5ba3-4c0d-993d-54917f63d52e",
                        Name = "get",
                        Description = "List all cloud endpoints in a sync group or retrieve details about a specific cloud endpoint. Returns cloud endpoint properties including Azure File Share configuration, storage account details, and provisioning state. Use --cloud-endpoint-name for a specific endpoint.",
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
                                Name = "name",
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cloud-endpoint-name",
                                Description = "The name of the cloud endpoint",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "registeredserver",
                Description = "Registered Server operations - Get, update, and unregister servers in your Storage Sync service.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "fe3b07c3-9a11-465e-bfb6-6461b85b2e52",
                        Name = "get",
                        Description = "List all registered servers in a Storage Sync service or retrieve details about a specific registered server. Returns server properties including server ID, registration status, agent version, OS version, and last heartbeat. Use --server-id for a specific server.",
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
                                Name = "name",
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-id",
                                Description = "The ID/name of the registered server",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "346661e1-64be-463a-96c6-3626966f55fa",
                        Name = "unregister",
                        Description = "Unregister a server from a Storage Sync service.",
                        Title = "Unregister",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-id",
                                Description = "The ID/name of the registered server",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(RegisteredServerUnregisterCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c443ed00-f17f-46a8-a5d3-df128aa1606b",
                        Name = "update",
                        Description = "Update properties of a registered server.",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-id",
                                Description = "The ID/name of the registered server",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceUpdateCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "serverendpoint",
                Description = "Server Endpoint operations - Create, get, update, and delete server endpoints in your sync groups.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "fcbdf461-6fde-4cfb-a944-4a56a2be90e4",
                        Name = "create",
                        Description = "Add a server endpoint to a sync group by specifying a local server path to sync. Server endpoints represent the on-premises side of the sync relationship and include cloud tiering configuration.",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-endpoint-name",
                                Description = "The name of the server endpoint",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-resource-id",
                                Description = "The resource ID of the registered server",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-local-path",
                                Description = "The local folder path on the server for syncing",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cloud-tiering",
                                Description = "Enable cloud tiering on this endpoint",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "volume-free-space-percent",
                                Description = "Volume free space percentage to maintain (1-99, default 20)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tier-files-older-than-days",
                                Description = "Archive files not accessed for this many days",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "local-cache-mode",
                                Description = "Local cache mode: DownloadNewAndModifiedFiles, UpdateLocallyCachedFiles",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "ef6c2aa9-bb64-4f94-b18b-018e04b504c9",
                        Name = "delete",
                        Description = "Delete a server endpoint from a sync group.",
                        Title = "Delete",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-endpoint-name",
                                Description = "The name of the server endpoint",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "cf197b94-6aa6-403b-8679-3a1ce5440ca3",
                        Name = "get",
                        Description = "List all server endpoints in a sync group or retrieve details about a specific server endpoint. Returns server endpoint properties including local path, cloud tiering status, sync health, and provisioning state. Use --name for a specific endpoint.",
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
                                Name = "name",
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-endpoint-name",
                                Description = "The name of the server endpoint",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "7b35bb46-0a34-4e44-9d7c-148e9992b445",
                        Name = "update",
                        Description = "Update properties of a server endpoint.",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server-endpoint-name",
                                Description = "The name of the server endpoint",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cloud-tiering",
                                Description = "Enable cloud tiering on this endpoint",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "volume-free-space-percent",
                                Description = "Volume free space percentage to maintain (1-99, default 20)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tier-files-older-than-days",
                                Description = "Archive files not accessed for this many days",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "local-cache-mode",
                                Description = "Local cache mode: DownloadNewAndModifiedFiles, UpdateLocallyCachedFiles",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceUpdateCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "service",
                Description = "Storage Sync Service operations - Create, get, update, and delete Storage Sync services in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "7c76387f-c62e-48d1-af3b-d444d6b3b79c",
                        Name = "create",
                        Description = "Create a new Azure Storage Sync service resource in a resource group. This is the top-level service container that manages sync groups, registered servers, and synchronization workflows.",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region/location name (e.g., EastUS, WestEurope)",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "a7dcf4e2-fd1d-4d0a-acd3-f56ea5eceef6",
                        Name = "delete",
                        Description = "Delete an Azure Storage Sync service and all its associated resources.",
                        Title = "Delete",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "77734a55-8290-4c16-8b37-cf37277f018f",
                        Name = "get",
                        Description = "Retrieve Azure Storage Sync service details or list all Storage Sync services. Use --name to get a specific service, or omit it to list all services in the subscription or resource group. Shows service properties, location, provisioning state, and configuration.",
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
                                Name = "name",
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "15db4769-1941-4b1e-9514-867b0f68eb2c",
                        Name = "update",
                        Description = "Update properties of an existing Azure Storage Sync service.",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "incoming-traffic-policy",
                                Description = "Incoming traffic policy for the service (AllowAllTraffic or AllowVirtualNetworksOnly)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Tags to assign to the service (space-separated key=value pairs)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "identity-type",
                                Description = "Managed service identity type (None, SystemAssigned, UserAssigned, SystemAssigned,UserAssigned)",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceUpdateCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "syncgroup",
                Description = "Sync Group operations - Create, get, and delete sync groups in your Storage Sync service.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "3572833c-4fc2-4bb9-9eed-52ae8b8899b8",
                        Name = "create",
                        Description = "Create a sync group within an existing Storage Sync service. Sync groups define a sync topology and contain cloud endpoints (Azure File Shares) and server endpoints (local server paths) that sync together.",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c8f91bd7-ea1d-4af4-9703-fe83c43b34b5",
                        Name = "delete",
                        Description = "Remove a sync group from a Storage Sync service. Deleting a sync group also removes all associated cloud endpoints and server endpoints within that group.",
                        Title = "Delete",
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
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "95ce2336-19e6-40fb-a3ea-e2a76772036b",
                        Name = "get",
                        Description = "Get details about a specific sync group or list all sync groups. If --sync-group-name is provided, returns a specific sync group; otherwise, lists all sync groups in the Storage Sync service.",
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
                                Name = "name",
                                Description = "The name of the storage sync service",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sync-group-name",
                                Description = "The name of the sync group",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(StorageSyncServiceGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        // Register the service implementation
        services.AddSingleton<IStorageSyncService, StorageSyncService>();
        // Register StorageSyncService commands
        services.AddSingleton<StorageSyncServiceGetCommand>();
        services.AddSingleton<StorageSyncServiceCreateCommand>();
        services.AddSingleton<StorageSyncServiceUpdateCommand>();
        services.AddSingleton<StorageSyncServiceDeleteCommand>();
        // Register RegisteredServer commands
        services.AddSingleton<RegisteredServerGetCommand>();
        services.AddSingleton<RegisteredServerUpdateCommand>();
        services.AddSingleton<RegisteredServerUnregisterCommand>();
        // Register SyncGroup commands
        services.AddSingleton<SyncGroupGetCommand>();
        services.AddSingleton<SyncGroupCreateCommand>();
        services.AddSingleton<SyncGroupDeleteCommand>();
        // Register CloudEndpoint commands
        services.AddSingleton<CloudEndpointGetCommand>();
        services.AddSingleton<CloudEndpointCreateCommand>();
        services.AddSingleton<CloudEndpointDeleteCommand>();
        services.AddSingleton<CloudEndpointTriggerChangeDetectionCommand>();
        // Register ServerEndpoint commands
        services.AddSingleton<ServerEndpointGetCommand>();
        services.AddSingleton<ServerEndpointCreateCommand>();
        services.AddSingleton<ServerEndpointUpdateCommand>();
        services.AddSingleton<ServerEndpointDeleteCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(CloudEndpointTriggerChangeDetectionCommand) => serviceProvider.GetRequiredService<CloudEndpointTriggerChangeDetectionCommand>(),
            nameof(StorageSyncServiceCreateCommand) => serviceProvider.GetRequiredService<StorageSyncServiceCreateCommand>(),
            nameof(StorageSyncServiceDeleteCommand) => serviceProvider.GetRequiredService<StorageSyncServiceDeleteCommand>(),
            nameof(StorageSyncServiceGetCommand) => serviceProvider.GetRequiredService<StorageSyncServiceGetCommand>(),
            nameof(RegisteredServerUnregisterCommand) => serviceProvider.GetRequiredService<RegisteredServerUnregisterCommand>(),
            nameof(StorageSyncServiceUpdateCommand) => serviceProvider.GetRequiredService<StorageSyncServiceUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in storagesync area.")
        };
}
