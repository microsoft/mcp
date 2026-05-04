// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        fabricOneLake.AddCommand<OneLakeWorkspaceListCommand>(serviceProvider);
        fabricOneLake.AddCommand<OneLakeItemListCommand>(serviceProvider);
        fabricOneLake.AddCommand<OneLakeItemDataListCommand>(serviceProvider);
        fabricOneLake.AddCommand<PathListCommand>(serviceProvider);
        fabricOneLake.AddCommand<BlobGetCommand>(serviceProvider);
        fabricOneLake.AddCommand<BlobPutCommand>(serviceProvider);
        fabricOneLake.AddCommand<FileDeleteCommand>(serviceProvider);
        fabricOneLake.AddCommand<DirectoryCreateCommand>(serviceProvider);
        fabricOneLake.AddCommand<DirectoryDeleteCommand>(serviceProvider);
        fabricOneLake.AddCommand<TableConfigGetCommand>(serviceProvider);
        fabricOneLake.AddCommand<TableNamespaceListCommand>(serviceProvider);
        fabricOneLake.AddCommand<TableNamespaceGetCommand>(serviceProvider);
        fabricOneLake.AddCommand<TableListCommand>(serviceProvider);
        fabricOneLake.AddCommand<TableGetCommand>(serviceProvider);

        return fabricOneLake;
    }
}
