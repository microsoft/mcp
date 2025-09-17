// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Batch;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Commands.DataLake.Directory;
using Azure.Mcp.Tools.Storage.Commands.DataLake.FileSystem;
using Azure.Mcp.Tools.Storage.Commands.Queue.Message;
using Azure.Mcp.Tools.Storage.Commands.Share.File;
using Azure.Mcp.Tools.Storage.Commands.Table;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage;

public class StorageSetup : IAreaSetup
{
    public string Name => "storage";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IStorageService, StorageService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var storage = new CommandGroup(Name,
            """
            Storage operations - Commands for managing and accessing Azure Storage accounts and their data services
            including Blobs, Data Lake Gen 2, Shares, Tables, and Queues for scalable cloud storage solutions. Use
            this tool when you need to list storage accounts, work with blob containers and blobs, access file shares,
            querying table storage, handle queue messages. This tool focuses on object storage, file storage,
            simple NoSQL table storage scenarios, and queue messaging. This tool is a hierarchical MCP command router
            where sub-commands are routed to MCP servers that require specific fields inside the "parameters" object.
            To invoke a command, set "command" and wrap its arguments in "parameters". Set "learn=true" to discover
            available sub-commands for different Azure Storage service operations including blobs, datalake, shares,
            tables, and queues. Note that this tool requires appropriate Storage account permissions and will only
            access storage resources accessible to the authenticated user.
            """);
        rootGroup.AddSubGroup(storage);

        // Create Storage subgroups
        var storageAccount = new CommandGroup("account", "Storage account operations - Commands for listing and managing Storage account in your Azure subscription.");
        storage.AddSubGroup(storageAccount);

        var tables = new CommandGroup("table", "Storage table operations - Commands for working with Azure Table Storage, including listing and querying table.");
        storage.AddSubGroup(tables);

        var blobs = new CommandGroup("blob", "Storage blob operations - Commands for uploading, downloading, and managing blob in your Azure Storage accounts.");
        storage.AddSubGroup(blobs);

        // Create Batch subgroup under blobs
        var batch = new CommandGroup("batch", "Storage batch operations - Commands for performing batch operations on multiple storage blobs efficiently.");
        blobs.AddSubGroup(batch);

        // Create a containers subgroup under blobs
        var blobContainer = new CommandGroup("container", "Storage blob container operations - Commands for managing blob containers in your Azure Storage accounts.");
        blobs.AddSubGroup(blobContainer);

        // Create Data Lake subgroup under storage
        var dataLake = new CommandGroup("datalake", "Data Lake Storage operations - Commands for managing Azure Data Lake Storage Gen2 file systems and paths.");
        storage.AddSubGroup(dataLake);

        // Create file-system subgroup under datalake
        var fileSystem = new CommandGroup("file-system", "Data Lake file system operations - Commands for managing file systems and paths in Azure Data Lake Storage Gen2.");
        dataLake.AddSubGroup(fileSystem);

        // Create directory subgroup under datalake
        var directory = new CommandGroup("directory", "Data Lake directory operations - Commands for managing directories in Azure Data Lake Storage Gen2.");
        dataLake.AddSubGroup(directory);

        // Create Queue subgroup under storage
        var queues = new CommandGroup("queue", "Storage queue operations - Commands for managing Azure Storage queues and queue messages.");
        storage.AddSubGroup(queues);

        // Create message subgroup under queue
        var queueMessage = new CommandGroup("message", "Storage queue message operations - Commands for sending and managing messages in Azure Storage queues.");
        queues.AddSubGroup(queueMessage);

        // Create file shares subgroup under storage
        var shares = new CommandGroup("share", "File share operations - Commands for managing Azure Storage file shares and their contents.");
        storage.AddSubGroup(shares);

        // Create file subgroup under shares
        var shareFiles = new CommandGroup("file", "File share file operations - Commands for managing files and directories within Azure Storage file shares.");
        shares.AddSubGroup(shareFiles);

        // Register Storage commands
        storageAccount.AddCommand("create", new AccountCreateCommand(loggerFactory.CreateLogger<AccountCreateCommand>()));
        storageAccount.AddCommand("get", new AccountGetCommand(loggerFactory.CreateLogger<AccountGetCommand>()));

        tables.AddCommand("list", new TableListCommand(loggerFactory.CreateLogger<TableListCommand>()));

        blobs.AddCommand("get", new BlobGetCommand(loggerFactory.CreateLogger<BlobGetCommand>()));
        blobs.AddCommand("upload", new BlobUploadCommand(loggerFactory.CreateLogger<BlobUploadCommand>()));

        batch.AddCommand("set-tier", new BatchSetTierCommand(loggerFactory.CreateLogger<BatchSetTierCommand>()));

        blobContainer.AddCommand("create", new ContainerCreateCommand(loggerFactory.CreateLogger<ContainerCreateCommand>()));
        blobContainer.AddCommand("get", new ContainerGetCommand(loggerFactory.CreateLogger<ContainerGetCommand>()));

        fileSystem.AddCommand("list-paths", new FileSystemListPathsCommand(loggerFactory.CreateLogger<FileSystemListPathsCommand>()));

        directory.AddCommand("create", new DirectoryCreateCommand(loggerFactory.CreateLogger<DirectoryCreateCommand>()));

        queueMessage.AddCommand("send", new QueueMessageSendCommand(loggerFactory.CreateLogger<QueueMessageSendCommand>()));

        shareFiles.AddCommand("list", new FileListCommand(loggerFactory.CreateLogger<FileListCommand>()));
    }
}
