// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Mcp.Tools.Storage.Table.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Storage;

public sealed class StorageRegistration : IAreaRegistration
{
    public static string AreaName => "storage";

    public static string AreaTitle => "Manage Azure Storage Account";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Manage Azure Storage Account operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "account",
                Description = "Storage accounts operations - Commands for listing and managing Storage accounts in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a2cf843a-57f2-45ea-8078-59b0be0805e6",
                        Name = "create",
                        Description = "Creates an Azure Storage account in the specified resource group and location and returns the created storage account information including name, location, SKU, access settings, and configuration details.",
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
                                Name = "account",
                                Description = "The name of the Azure Storage account to create. Must be globally unique, 3-24 characters, lowercase letters and numbers only.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region where the storage account will be created (e.g., 'eastus', 'westus2').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sku",
                                Description = "The storage account SKU. Valid values: Standard_LRS, Standard_GRS, Standard_RAGRS, Standard_ZRS, Premium_LRS, Premium_ZRS, Standard_GZRS, Standard_RAGZRS.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "access-tier",
                                Description = "The default access tier for blob storage. Valid values: Hot, Cool.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "enable-hierarchical-namespace",
                                Description = "Whether to enable hierarchical namespace (Data Lake Storage Gen2) for the storage account.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(AccountCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "eb2363f1-f21f-45fc-ad63-bacfbae8c45c",
                        Name = "get",
                        Description = "Retrieves detailed information about Azure Storage accounts, including account name, location, SKU, kind, hierarchical namespace status, HTTPS-only settings, and blob public access configuration. If a specific account name is not provided, the command will return details for all accounts in a subscription.",
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
                                Name = "account",
                                Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(AccountGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "blob",
                Description = "Storage blob operations - Commands for uploading, downloading, and managing blob in your Azure Storage accounts.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "d6bdc190-e68f-49af-82e7-9cf6ec9b8183",
                        Name = "get",
                        Description = "List/get/show blobs in a blob container in Storage account. Use this tool to list the blobs in a container or get details for a specific blob. Shows blob properties including metadata, size, last modification time, and content properties. If no blob specified, lists all blobs present in the container. Required: account, container <container>, subscription <subscription>. Optional: blob <blob>, tenant <tenant>. Returns: blob name, size, lastModified, contentType, contentMD5, metadata, and blob properties. Do not use this tool to list containers in the storage account.",
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
                                Name = "account",
                                Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "container",
                                Description = "The name of the container to access within the storage account.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "blob",
                                Description = "The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(AccountGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "aafb82ac-e35a-4800-b362-c642a3ac1e17",
                        Name = "upload",
                        Description = "Uploads a local file to an Azure Storage blob, only if the blob does not exist, returning the last modified time, ETag, and content hash of the uploaded blob.",
                        Title = "Upload",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "account",
                                Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "container",
                                Description = "The name of the container to access within the storage account.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "blob",
                                Description = "The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "local-file-path",
                                Description = "The local file path to read content from or to write content to. This should be the full path to the file on your local system.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(BlobUploadCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "container",
                        Description = "Storage blob container operations - Commands for managing blob containers in your Azure Storage accounts.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "f5088334-e630-4df0-a5be-ac87787acad0",
                                Name = "create",
                                Description = "Create/provision a new Azure Storage blob container in a storage account. Required: --account <account>, --container <container>, --subscription <subscription>. Optional: --public-access-level, --tenant <tenant>. Returns: container name, lastModified, eTag, leaseStatus, publicAccessLevel, hasImmutabilityPolicy, hasLegalHold. Creates a logical container for organizing blobs within a storage account.",
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
                                        Name = "account",
                                        Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "container",
                                        Description = "The name of the container to access within the storage account.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(AccountCreateCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "e96eb850-abb8-431d-bdc6-7ccd0a24838e",
                                Name = "get",
                                Description = "Show/list containers in a storage account. Use this tool to list all blob containers in the storage account or show details for a specific Storage container. Displays container properties including access policies, lease status, and metadata. If no container specified, shows all containers in the storage account. Required: account <account>, subscription <subscription>. Optional: container <container>, tenant <tenant>. Returns: container name, lastModified, leaseStatus, publicAccessLevel, metadata, and container properties. Do not use this tool to list blobs in a container.",
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
                                        Name = "account",
                                        Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "container",
                                        Description = "The name of the container to access within the storage account.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(AccountGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "table",
                Description = "Storage table operations - Commands for managing tables in your Azure Storage accounts.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "1236ad1d-baf1-4b95-8c1d-420637ce08da",
                        Name = "list",
                        Description = "List all tables in an Azure Storage account. Shows table names for the specified storage account. Required: account, subscription. Optional: tenant. Returns: table names. Do not use this tool for Cosmos DB tables or Kusto/Data Explorer tables.",
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
                                Name = "account",
                                Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TableListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<AccountCreateCommand>();
        services.AddSingleton<AccountGetCommand>();
        services.AddSingleton<BlobGetCommand>();
        services.AddSingleton<BlobUploadCommand>();
        services.AddSingleton<ContainerCreateCommand>();
        services.AddSingleton<ContainerGetCommand>();
        services.AddSingleton<TableListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(AccountCreateCommand) => serviceProvider.GetRequiredService<AccountCreateCommand>(),
            nameof(AccountGetCommand) => serviceProvider.GetRequiredService<AccountGetCommand>(),
            nameof(BlobUploadCommand) => serviceProvider.GetRequiredService<BlobUploadCommand>(),
            nameof(TableListCommand) => serviceProvider.GetRequiredService<TableListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in storage area.")
        };
}
