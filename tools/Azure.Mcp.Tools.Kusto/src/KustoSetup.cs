// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Rendering;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Kusto;

public class KustoSetup : IAreaSetup
{
    public string Name => "kusto";

    public string Title => "Azure Data Explorer";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IKustoService, KustoService>();
        services.AddSingleton<IKustoChartRenderer, ScottPlotChartRenderer>();

        services.AddSingleton<SampleCommand>();
        services.AddSingleton<QueryCommand>();

        services.AddSingleton<ClusterListCommand>();
        services.AddSingleton<ClusterGetCommand>();

        services.AddSingleton<DatabaseListCommand>();

        services.AddSingleton<TableListCommand>();
        services.AddSingleton<TableSchemaCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Kusto command group
        var kusto = new CommandGroup(Name, "Kusto operations - Commands for managing and querying Azure Data Explorer (Kusto) resources. Includes operations for listing clusters and databases, executing KQL queries, retrieving table schemas, and working with Kusto data analytics workloads.", Title);

        // Create Kusto cluster subgroups
        var clusters = new CommandGroup("cluster", "Kusto cluster operations - Commands for listing clusters in your Azure subscription.");
        kusto.AddSubGroup(clusters);

        var databases = new CommandGroup("database", "Kusto database operations - Commands for listing databases in a cluster.");
        kusto.AddSubGroup(databases);

        var tables = new CommandGroup("table", "Kusto table operations - Commands for listing tables in a database.");
        kusto.AddSubGroup(tables);

        kusto.AddCommand<SampleCommand>(serviceProvider);
        kusto.AddCommand<QueryCommand>(serviceProvider);

        clusters.AddCommand<ClusterListCommand>(serviceProvider);
        clusters.AddCommand<ClusterGetCommand>(serviceProvider);

        databases.AddCommand<DatabaseListCommand>(serviceProvider);

        tables.AddCommand<TableListCommand>(serviceProvider);
        tables.AddCommand<TableSchemaCommand>(serviceProvider);

        return kusto;
    }
}
