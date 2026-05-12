// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;
using Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;
using global::DataFactory.MCP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.DataFactory;

public class DataFactoryAreaSetup : IAreaSetup
{
    public string Name => "datafactory";
    public string Title => "Microsoft Fabric Data Factory";

    public void ConfigureServices(IServiceCollection services)
    {
        // Register global::DataFactory.MCP.Core services (auth, HttpClients, all service implementations)
        services.AddDataFactoryMcpServices();

        // Register command instances
        services.AddSingleton<ListPipelinesCommand>();
        services.AddSingleton<CreatePipelineCommand>();
        services.AddSingleton<GetPipelineCommand>();
        services.AddSingleton<RunPipelineCommand>();
        services.AddSingleton<ListDataflowsCommand>();
        services.AddSingleton<CreateDataflowCommand>();
        services.AddSingleton<ExecuteQueryCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(Name,
            """
            Microsoft Fabric Data Factory Operations - Manage pipelines, dataflows, and workspaces.
            Use this tool when you need to:
            - List and manage workspaces
            - Create, get, list, and run pipelines
            - Work with dataflows and data transformations
            - Execute M (Power Query) expressions against dataflows
            """);

        group.AddCommand<ListPipelinesCommand>(serviceProvider);
        group.AddCommand<CreatePipelineCommand>(serviceProvider);
        group.AddCommand<GetPipelineCommand>(serviceProvider);
        group.AddCommand<RunPipelineCommand>(serviceProvider);
        group.AddCommand<ListDataflowsCommand>(serviceProvider);
        group.AddCommand<CreateDataflowCommand>(serviceProvider);
        group.AddCommand<ExecuteQueryCommand>(serviceProvider);

        return group;
    }
}
