// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Mcp.Tools.Storage.Table.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Storage;

public class StorageSetup : IAreaSetup
{
    public string Name => "storage";

    public string Title => "Manage Azure Storage Account";

    public void ConfigureServices(IServiceCollection services)
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

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var storage = new CommandGroup(Name,
            """
            Storage operations - Commands for creating, listing, getting, and managing Azure Storage accounts,
            blob containers, blobs, and tables. Use this tool to create storage accounts, list and get storage
            account details (SKU, location, HNS, HTTPS-only settings), create and list blob containers, list
            and get blob properties, upload files to blob storage, and list tables. Covers Azure Blob Storage,
            Azure Table Storage, and storage account management. Do not use for Azure Cosmos DB containers,
            Azure Container Registry, or Azure Managed Lustre file systems.
            """,
            Title);

        // Create Storage subgroups
        var storageAccount = new CommandGroup("account", "Storage accounts operations - Commands for listing and managing Storage accounts in your Azure subscription.");
        storage.AddSubGroup(storageAccount);

        var blobs = new CommandGroup("blob", "Storage blob operations - Commands for uploading, downloading, and managing blob in your Azure Storage accounts.");
        storage.AddSubGroup(blobs);

        // Create a containers subgroup under blobs
        var blobContainer = new CommandGroup("container", "Storage blob container operations - Commands for managing blob containers in your Azure Storage accounts.");
        blobs.AddSubGroup(blobContainer);

        // Register Storage commands
        var accountCreate = serviceProvider.GetRequiredService<AccountCreateCommand>();
        storageAccount.AddCommand(accountCreate.Name, accountCreate);
        var accountGet = serviceProvider.GetRequiredService<AccountGetCommand>();
        storageAccount.AddCommand(accountGet.Name, accountGet);

        var blobGet = serviceProvider.GetRequiredService<BlobGetCommand>();
        blobs.AddCommand(blobGet.Name, blobGet);
        var blobUpload = serviceProvider.GetRequiredService<BlobUploadCommand>();
        blobs.AddCommand(blobUpload.Name, blobUpload);

        var containerCreate = serviceProvider.GetRequiredService<ContainerCreateCommand>();
        blobContainer.AddCommand(containerCreate.Name, containerCreate);
        var containerGet = serviceProvider.GetRequiredService<ContainerGetCommand>();
        blobContainer.AddCommand(containerGet.Name, containerGet);

        // Create Table subgroup under storage
        var tables = new CommandGroup("table", "Storage table operations - Commands for managing tables in your Azure Storage accounts.");
        storage.AddSubGroup(tables);

        var tableList = serviceProvider.GetRequiredService<TableListCommand>();
        tables.AddCommand(tableList.Name, tableList);

        return storage;
    }
}
