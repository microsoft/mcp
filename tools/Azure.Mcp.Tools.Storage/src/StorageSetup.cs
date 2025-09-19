// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
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
            Storage operations - Commands for managing and accessing Azure Storage accounts and the Blobs service for
            scalable cloud storage solutions. Use this tool when you need to list storage accounts and work with blob
            containers and blobs. This tool focuses on object storage scenarios. This tool is a hierarchical MCP command
            router where sub-commands are routed to MCP servers that require specific fields inside the "parameters" object.
            To invoke a command, set "command" and wrap its arguments in "parameters". Set "learn=true" to discover available
            sub-commands for different Azure Storage service operations including blobs. Note that this tool requires
            appropriate Storage account permissions and will only access storage resources accessible to the authenticated user.
            """);
        rootGroup.AddSubGroup(storage);

        // Create Storage subgroups
        var storageAccount = new CommandGroup("account", "Storage accounts operations - Commands for listing and managing Storage accounts in your Azure subscription.");
        storage.AddSubGroup(storageAccount);

        var blobs = new CommandGroup("blob", "Storage blob operations - Commands for uploading, downloading, and managing blob in your Azure Storage accounts.");
        storage.AddSubGroup(blobs);

        // Create a containers subgroup under blobs
        var blobContainer = new CommandGroup("container", "Storage blob container operations - Commands for managing blob containers in your Azure Storage accounts.");
        blobs.AddSubGroup(blobContainer);

        // Register Storage commands
        storageAccount.AddCommand("create", new AccountCreateCommand(loggerFactory.CreateLogger<AccountCreateCommand>()));
        storageAccount.AddCommand("get", new AccountGetCommand(loggerFactory.CreateLogger<AccountGetCommand>()));

        blobs.AddCommand("get", new BlobGetCommand(loggerFactory.CreateLogger<BlobGetCommand>()));
        blobs.AddCommand("upload", new BlobUploadCommand(loggerFactory.CreateLogger<BlobUploadCommand>()));

        blobContainer.AddCommand("create", new ContainerCreateCommand(loggerFactory.CreateLogger<ContainerCreateCommand>()));
        blobContainer.AddCommand("get", new ContainerGetCommand(loggerFactory.CreateLogger<ContainerGetCommand>()));
    }
}
