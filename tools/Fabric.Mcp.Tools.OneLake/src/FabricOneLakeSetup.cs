// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Commands.Item;
using Fabric.Mcp.Tools.OneLake.Commands.Table;
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

        // Register table commands
        services.AddSingleton<TableConfigGetCommand>();
        services.AddSingleton<TableListCommand>();
        services.AddSingleton<TableGetCommand>();
        services.AddSingleton<TableNamespaceListCommand>();
        services.AddSingleton<TableNamespaceGetCommand>();
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

        // Register all commands at the onelake level (flat structure with verb_object naming)
        var listWorkspaces = serviceProvider.GetRequiredService<OneLakeWorkspaceListCommand>();
        fabricOneLake.AddCommand(listWorkspaces.Name, listWorkspaces);

        var listItems = serviceProvider.GetRequiredService<OneLakeItemListCommand>();
        fabricOneLake.AddCommand(listItems.Name, listItems);

        var listItemsDfs = serviceProvider.GetRequiredService<OneLakeItemDataListCommand>();
        fabricOneLake.AddCommand(listItemsDfs.Name, listItemsDfs);

        var listFiles = serviceProvider.GetRequiredService<PathListCommand>();
        fabricOneLake.AddCommand(listFiles.Name, listFiles);

        var downloadFile = serviceProvider.GetRequiredService<BlobGetCommand>();
        fabricOneLake.AddCommand(downloadFile.Name, downloadFile);

        var uploadFile = serviceProvider.GetRequiredService<BlobPutCommand>();
        fabricOneLake.AddCommand(uploadFile.Name, uploadFile);

        var deleteFile = serviceProvider.GetRequiredService<BlobDeleteCommand>();
        fabricOneLake.AddCommand(deleteFile.Name, deleteFile);

        var createDirectory = serviceProvider.GetRequiredService<DirectoryCreateCommand>();
        fabricOneLake.AddCommand(createDirectory.Name, createDirectory);

        var deleteDirectory = serviceProvider.GetRequiredService<DirectoryDeleteCommand>();
        fabricOneLake.AddCommand(deleteDirectory.Name, deleteDirectory);

        var getTableConfig = serviceProvider.GetRequiredService<TableConfigGetCommand>();
        fabricOneLake.AddCommand(getTableConfig.Name, getTableConfig);

        var listTableNamespaces = serviceProvider.GetRequiredService<TableNamespaceListCommand>();
        fabricOneLake.AddCommand(listTableNamespaces.Name, listTableNamespaces);

        var getTableNamespace = serviceProvider.GetRequiredService<TableNamespaceGetCommand>();
        fabricOneLake.AddCommand(getTableNamespace.Name, getTableNamespace);

        var listTables = serviceProvider.GetRequiredService<TableListCommand>();
        fabricOneLake.AddCommand(listTables.Name, listTables);

        var getTable = serviceProvider.GetRequiredService<TableGetCommand>();
        fabricOneLake.AddCommand(getTable.Name, getTable);

        return fabricOneLake;
    }
}