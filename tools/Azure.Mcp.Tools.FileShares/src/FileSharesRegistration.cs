// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.FileShares;

public sealed class FileSharesRegistration : IAreaRegistration
{
    public static string AreaName => "fileshares";

    public static string AreaTitle => "Azure File Shares";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "File Shares operations - Commands for managing Azure File Shares.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "a9e1f0b2-c3d4-4e5f-a6b7-c8d9e0f1a2b3",
                Name = "limits",
                Description = "Get file share limits for a subscription and location",
                Title = "Get File Share Limits",
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
                        Description = "The Azure region/location name (e.g., eastus, westeurope)",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(FileShareGetLimitsCommand)
            },
            new CommandDescriptor
            {
                Id = "3c5e1fb2-3a8d-4f8e-8b0a-1c2d3e4f5a6b",
                Name = "rec",
                Description = "Get provisioning parameter recommendations for a file share based on desired storage size",
                Title = "Get File Share Provisioning Recommendation",
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
                        Description = "The Azure region/location name (e.g., eastus, westeurope)",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "provisioned-storage-in-gib",
                        Description = "The desired provisioned storage size of the share in GiB",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(FileShareGetProvisioningRecommendationCommand)
            },
            new CommandDescriptor
            {
                Id = "93d14ba8-5e75-4190-93dd-f47e932b849b",
                Name = "usage",
                Description = "Get file share usage data for a subscription and location",
                Title = "Get File Share Usage Data",
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
                        Description = "The Azure region/location name (e.g., eastus, westeurope)",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(FileShareGetUsageDataCommand)
            },
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "fileshare",
                Description = "File share operations - Commands for managing file shares.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
                        Name = "check-name-availability",
                        Description = "Check if a file share name is available",
                        Title = "Check File Share Name Availability",
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
                                Name = "name",
                                Description = "The name of the file share",
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
                        HandlerType = nameof(FileShareCheckNameAvailabilityCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e",
                        Name = "create",
                        Description = "Create a new Azure managed file share resource in a resource group. This creates a high-performance, fully managed file share accessible via NFS protocol.",
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
                                Description = "The name of the file share",
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
                            new OptionDescriptor
                            {
                                Name = "mount-name",
                                Description = "The mount name of the file share as seen by end users",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "media-tier",
                                Description = "The storage media tier (e.g., SSD)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "redundancy",
                                Description = "The redundancy level (e.g., Local, Zone)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "protocol",
                                Description = "The file sharing protocol (e.g., NFS)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioned-storage-in-gib",
                                Description = "The desired provisioned storage size of the share in GiB",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioned-io-per-sec",
                                Description = "The provisioned IO operations per second",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioned-throughput-mib-per-sec",
                                Description = "The provisioned throughput in MiB per second",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "public-network-access",
                                Description = "Public network access setting (Enabled or Disabled)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "nfs-root-squash",
                                Description = "NFS root squash setting (NoRootSquash, RootSquash, or AllSquash)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "allowed-subnets",
                                Description = "Comma-separated list of subnet IDs allowed to access the file share",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Resource tags as JSON (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileShareCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b",
                        Name = "delete",
                        Description = "Delete a file share",
                        Title = "Delete File Share",
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
                                Description = "The name of the file share",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileShareDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f",
                        Name = "get",
                        Description = "Get details of a specific file share or list all file shares. If --name is provided, returns a specific file share; otherwise, lists all file shares in the subscription or resource group.",
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
                                Description = "The name of the file share",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileShareGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "d7e8f9a0-b1c2-4d3e-4f5a-6b7c8d9e0f1a",
                        Name = "update",
                        Description = "Update an existing Azure managed file share resource. Allows updating mutable properties like provisioned storage, IOPS, throughput, and network access settings.",
                        Title = "Update",
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
                                Description = "The name of the file share",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioned-storage-in-gib",
                                Description = "The desired provisioned storage size of the share in GiB",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioned-io-per-sec",
                                Description = "The provisioned IO operations per second",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioned-throughput-mib-per-sec",
                                Description = "The provisioned throughput in MiB per second",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "public-network-access",
                                Description = "Public network access setting (Enabled or Disabled)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "nfs-root-squash",
                                Description = "NFS root squash setting (NoRootSquash, RootSquash, or AllSquash)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "allowed-subnets",
                                Description = "Comma-separated list of subnet IDs allowed to access the file share",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Resource tags as JSON (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(FileShareUpdateCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "peconnection",
                        Description = "Private endpoint connection operations - Commands for managing private endpoint connections.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "a8e9f7d6-c5b4-4a3d-9e2f-1c0b8a7d6e5f",
                                Name = "get",
                                Description = "Get details of a specific private endpoint connection or list all private endpoint connections for a file share. If --connection-name is provided, returns a specific connection; otherwise, lists all connections.",
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
                                        Name = "file-share-name",
                                        Description = "The name of the file share",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "connection-name",
                                        Description = "The name of the private endpoint connection",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(FileShareGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "c6d7e8f9-a0b1-4c2d-3e4f-5a6b7c8d9e0f",
                                Name = "update",
                                Description = "Update the state of a private endpoint connection for a file share. Use this to approve or reject private endpoint connection requests.",
                                Title = "Update",
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
                                        Name = "file-share-name",
                                        Description = "The name of the file share",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "connection-name",
                                        Description = "The name of the private endpoint connection",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "status",
                                        Description = "The connection status (Approved, Rejected, or Pending)",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "description",
                                        Description = "Description for the connection state change",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(FileShareUpdateCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "snapshot",
                        Description = "File share snapshot operations - Commands for managing file share snapshots.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c",
                                Name = "create",
                                Description = "Create a snapshot of an Azure managed file share. Snapshots are read-only point-in-time copies used for backup and recovery.",
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
                                        Name = "file-share-name",
                                        Description = "The name of the parent file share",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "snapshot-name",
                                        Description = "The name of the snapshot",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "metadata",
                                        Description = "Custom metadata for the snapshot as a JSON object (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(FileShareCreateCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f",
                                Name = "delete",
                                Description = "Delete a file share snapshot permanently. This operation cannot be undone.",
                                Title = "Delete File Share",
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
                                        Name = "file-share-name",
                                        Description = "The name of the parent file share",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "snapshot-name",
                                        Description = "The name of the snapshot",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(FileShareDeleteCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d",
                                Name = "get",
                                Description = "Get details of a specific file share snapshot or list all snapshots. If --snapshot-name is provided, returns a specific snapshot; otherwise, lists all snapshots for the file share.",
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
                                        Name = "file-share-name",
                                        Description = "The name of the parent file share",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "snapshot-name",
                                        Description = "The name of the snapshot",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(FileShareGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "b5c6d7e8-f9a0-4b1c-2d3e-4f5a6b7c8d9e",
                                Name = "update",
                                Description = "Update properties and metadata of an Azure managed file share snapshot, such as tags or retention policies.",
                                Title = "Update",
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
                                        Name = "file-share-name",
                                        Description = "The name of the parent file share",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "snapshot-name",
                                        Description = "The name of the snapshot",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "metadata",
                                        Description = "Custom metadata for the snapshot as a JSON object (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(FileShareUpdateCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IFileSharesService, FileSharesService>();
        services.AddSingleton<FileShareGetCommand>();
        services.AddSingleton<FileShareCreateCommand>();
        services.AddSingleton<FileShareUpdateCommand>();
        services.AddSingleton<FileShareDeleteCommand>();
        services.AddSingleton<FileShareCheckNameAvailabilityCommand>();
        services.AddSingleton<SnapshotGetCommand>();
        services.AddSingleton<SnapshotCreateCommand>();
        services.AddSingleton<SnapshotUpdateCommand>();
        services.AddSingleton<SnapshotDeleteCommand>();
        services.AddSingleton<PrivateEndpointConnectionGetCommand>();
        services.AddSingleton<PrivateEndpointConnectionUpdateCommand>();
        services.AddSingleton<FileShareGetLimitsCommand>();
        services.AddSingleton<FileShareGetProvisioningRecommendationCommand>();
        services.AddSingleton<FileShareGetUsageDataCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(FileShareGetLimitsCommand) => serviceProvider.GetRequiredService<FileShareGetLimitsCommand>(),
            nameof(FileShareGetProvisioningRecommendationCommand) => serviceProvider.GetRequiredService<FileShareGetProvisioningRecommendationCommand>(),
            nameof(FileShareGetUsageDataCommand) => serviceProvider.GetRequiredService<FileShareGetUsageDataCommand>(),
            nameof(FileShareCheckNameAvailabilityCommand) => serviceProvider.GetRequiredService<FileShareCheckNameAvailabilityCommand>(),
            nameof(FileShareCreateCommand) => serviceProvider.GetRequiredService<FileShareCreateCommand>(),
            nameof(FileShareDeleteCommand) => serviceProvider.GetRequiredService<FileShareDeleteCommand>(),
            nameof(FileShareGetCommand) => serviceProvider.GetRequiredService<FileShareGetCommand>(),
            nameof(FileShareUpdateCommand) => serviceProvider.GetRequiredService<FileShareUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in fileshares area.")
        };
}
