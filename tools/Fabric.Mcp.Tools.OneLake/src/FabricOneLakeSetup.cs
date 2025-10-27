// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Commands.Item;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<ItemCreateCommand>();
        
        // Register file commands
        services.AddSingleton<FileReadCommand>();
        services.AddSingleton<FileWriteCommand>();
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

        // Register workspace commands
        var oneLakeWorkspaceListCommand = serviceProvider.GetRequiredService<OneLakeWorkspaceListCommand>();
        fabricOneLake.AddCommand(oneLakeWorkspaceListCommand.Name, oneLakeWorkspaceListCommand);
        
        // Register item commands
        var oneLakeItemListCommand = serviceProvider.GetRequiredService<OneLakeItemListCommand>();
        fabricOneLake.AddCommand(oneLakeItemListCommand.Name, oneLakeItemListCommand);
        
        var itemCreateCommand = serviceProvider.GetRequiredService<ItemCreateCommand>();
        fabricOneLake.AddCommand(itemCreateCommand.Name, itemCreateCommand);
        
        // Register file commands
        var fileReadCommand = serviceProvider.GetRequiredService<FileReadCommand>();
        fabricOneLake.AddCommand(fileReadCommand.Name, fileReadCommand);
        
        var fileWriteCommand = serviceProvider.GetRequiredService<FileWriteCommand>();
        fabricOneLake.AddCommand(fileWriteCommand.Name, fileWriteCommand);

        return fabricOneLake;
    }
}