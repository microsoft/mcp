// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Commands.Item;
using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.OneLake;

public class FabricOneLakeSetup : IAreaSetup
{
    public string Name => "onelake";
    public string Title => "Microsoft Fabric OneLake";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOneLakeService, OneLakeService>();
        services.AddHttpClient<OneLakeService>();

        // Register workspace commands
        services.AddSingleton<OneLakeWorkspaceListCommand>();

        // Register item commands
        services.AddSingleton<OneLakeItemListCommand>();
        services.AddSingleton<OneLakeItemDataListCommand>();
        services.AddSingleton<ItemCreateCommand>();

        // Register file commands
        services.AddSingleton<FileReadCommand>();
        services.AddSingleton<FileWriteCommand>();
        services.AddSingleton<FileDeleteCommand>();
        services.AddSingleton<PathListCommand>();

        // Register blob commands
        services.AddSingleton<BlobPutCommand>();
        services.AddSingleton<BlobGetCommand>();
        services.AddSingleton<BlobDeleteCommand>();
        services.AddSingleton<BlobListCommand>();

        // Register directory commands
        services.AddSingleton<DirectoryCreateCommand>();
        services.AddSingleton<DirectoryDeleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricOneLake = new CommandGroup(Name,
            """
            Microsoft Fabric OneLake Operations - Manage and interact with OneLake data lake storage.
            OneLake is Microsoft Fabric's built-in data lake that provides unified storage for all
            analytics workloads. Use this tool when you need to:
            - Manage OneLake folders and files
            - Configure data access and permissions
            - Monitor OneLake storage usage and performance
            - Integrate with other Fabric workloads through OneLake
            This tool provides operations for working with OneLake resources within your Fabric tenant.
            """);

        var workspaceGroup = new CommandGroup("workspace", "Workspace operations - Commands for listing and managing OneLake workspaces.");
        fabricOneLake.AddSubGroup(workspaceGroup);

        var itemGroup = new CommandGroup("item", "Item operations - Commands for listing and creating OneLake items.");
        fabricOneLake.AddSubGroup(itemGroup);

        var fileGroup = new CommandGroup("file", "File operations - Commands for reading, writing, deleting, and browsing OneLake files using the DFS API.");
        fabricOneLake.AddSubGroup(fileGroup);

        var blobGroup = new CommandGroup("blob", "Blob operations - Commands for interacting with OneLake's blob-compatible endpoints.");
        fabricOneLake.AddSubGroup(blobGroup);

        var uploadGroup = new CommandGroup("upload", "Upload operations - Commands for uploading files into OneLake storage.");
        fabricOneLake.AddSubGroup(uploadGroup);

        var downloadGroup = new CommandGroup("download", "Download operations - Commands for retrieving files from OneLake storage.");
        fabricOneLake.AddSubGroup(downloadGroup);

        var directoryGroup = new CommandGroup("directory", "Directory operations - Commands for creating and deleting OneLake directories.");
        fabricOneLake.AddSubGroup(directoryGroup);

        var oneLakeWorkspaceListCommand = serviceProvider.GetRequiredService<OneLakeWorkspaceListCommand>();
        workspaceGroup.AddCommand(oneLakeWorkspaceListCommand.Name, oneLakeWorkspaceListCommand);

        var oneLakeItemListCommand = serviceProvider.GetRequiredService<OneLakeItemListCommand>();
        itemGroup.AddCommand(oneLakeItemListCommand.Name, oneLakeItemListCommand);

        var oneLakeItemDataListCommand = serviceProvider.GetRequiredService<OneLakeItemDataListCommand>();
        itemGroup.AddCommand(oneLakeItemDataListCommand.Name, oneLakeItemDataListCommand);

        var itemCreateCommand = serviceProvider.GetRequiredService<ItemCreateCommand>();
        itemGroup.AddCommand(itemCreateCommand.Name, itemCreateCommand);

        var fileReadCommand = serviceProvider.GetRequiredService<FileReadCommand>();
        fileGroup.AddCommand(fileReadCommand.Name, fileReadCommand);

        var fileWriteCommand = serviceProvider.GetRequiredService<FileWriteCommand>();
        fileGroup.AddCommand(fileWriteCommand.Name, fileWriteCommand);

        var fileDeleteCommand = serviceProvider.GetRequiredService<FileDeleteCommand>();
        fileGroup.AddCommand(fileDeleteCommand.Name, fileDeleteCommand);

        var pathListCommand = serviceProvider.GetRequiredService<PathListCommand>();
        fileGroup.AddCommand(pathListCommand.Name, pathListCommand);

        var blobPutCommand = serviceProvider.GetRequiredService<BlobPutCommand>();
        uploadGroup.AddCommand(blobPutCommand.Name, blobPutCommand);

        var blobGetCommand = serviceProvider.GetRequiredService<BlobGetCommand>();
        downloadGroup.AddCommand(blobGetCommand.Name, blobGetCommand);

        var blobDeleteCommand = serviceProvider.GetRequiredService<BlobDeleteCommand>();
        blobGroup.AddCommand(blobDeleteCommand.Name, blobDeleteCommand);

        var blobListCommand = serviceProvider.GetRequiredService<BlobListCommand>();
        blobGroup.AddCommand(blobListCommand.Name, blobListCommand);

        var directoryCreateCommand = serviceProvider.GetRequiredService<DirectoryCreateCommand>();
        directoryGroup.AddCommand(directoryCreateCommand.Name, directoryCreateCommand);

        var directoryDeleteCommand = serviceProvider.GetRequiredService<DirectoryDeleteCommand>();
        directoryGroup.AddCommand(directoryDeleteCommand.Name, directoryDeleteCommand);

        return fabricOneLake;
    }
}
