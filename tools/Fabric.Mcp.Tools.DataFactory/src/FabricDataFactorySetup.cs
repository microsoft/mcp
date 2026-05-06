// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Extensions;
using Fabric.Mcp.Tools.DataFactory.Commands;
using Fabric.Mcp.Tools.DataFactory.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.DataFactory;

public class FabricDataFactorySetup : IAreaSetup
{
    public string Name => "datafactory";
    public string Title => "Microsoft Fabric Data Factory";

    public void ConfigureServices(IServiceCollection services)
    {
        // Register no-op notification service required by DataFactory.MCP.Core
        services.AddSingleton<IUserNotificationService, NullUserNotificationService>();

        // Register all DataFactory core services (auth, HTTP clients, pipeline/workspace/etc. services)
        services.AddDataFactoryMcpServices();

        // Register command wrappers
        services.AddSingleton<WorkspaceListCommand>();
        services.AddSingleton<PipelineListCommand>();
        services.AddSingleton<PipelineCreateCommand>();
        services.AddSingleton<PipelineGetCommand>();
        services.AddSingleton<PipelineRunCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var dataFactory = new CommandGroup(Name,
            """
            Microsoft Fabric Data Factory Operations - Manage Data Factory pipelines and workspaces.
            Use this tool when you need to:
            - List and manage Fabric workspaces
            - Create, view, and run data pipelines
            - Monitor pipeline execution status
            This tool provides operations for working with Data Factory resources within your Fabric tenant.
            """);

        dataFactory.AddCommand<WorkspaceListCommand>(serviceProvider);
        dataFactory.AddCommand<PipelineListCommand>(serviceProvider);
        dataFactory.AddCommand<PipelineCreateCommand>(serviceProvider);
        dataFactory.AddCommand<PipelineGetCommand>(serviceProvider);
        dataFactory.AddCommand<PipelineRunCommand>(serviceProvider);

        return dataFactory;
    }
}
