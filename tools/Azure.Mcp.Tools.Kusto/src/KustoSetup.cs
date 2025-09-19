// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Kusto;

public class KustoSetup : IAreaSetup
{
    public string Name => "kusto";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IKustoService, KustoService>();

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
        var kusto = new CommandGroup(Name, "Kusto operations - Commands for managing and querying Azure Data Explorer (Kusto) resources. Includes operations for listing clusters and databases, executing KQL queries, retrieving table schemas, and working with Kusto data analytics workloads.");

        // Create Kusto cluster subgroups
        var clusters = new CommandGroup("cluster", "Kusto cluster operations - Commands for listing clusters in your Azure subscription.");
        kusto.AddSubGroup(clusters);

        var databases = new CommandGroup("database", "Kusto database operations - Commands for listing databases in a cluster.");
        kusto.AddSubGroup(databases);

        var tables = new CommandGroup("table", "Kusto table operations - Commands for listing tables in a database.");
        kusto.AddSubGroup(tables);

        var sampleCommand = serviceProvider.GetRequiredService<SampleCommand>();
        kusto.AddCommand(sampleCommand.Name, sampleCommand);
        var queryCommand = serviceProvider.GetRequiredService<QueryCommand>();
        kusto.AddCommand(queryCommand.Name, queryCommand);

        var clusterList = serviceProvider.GetRequiredService<ClusterListCommand>();
        clusters.AddCommand(clusterList.Name, clusterList);
        var clusterGet = serviceProvider.GetRequiredService<ClusterGetCommand>();
        clusters.AddCommand(clusterGet.Name, clusterGet);

        var databaseList = serviceProvider.GetRequiredService<DatabaseListCommand>();
        databases.AddCommand(databaseList.Name, databaseList);

        var tableList = serviceProvider.GetRequiredService<TableListCommand>();
        tables.AddCommand(tableList.Name, tableList);
        var tableSchema = serviceProvider.GetRequiredService<TableSchemaCommand>();
        tables.AddCommand(tableSchema.Name, tableSchema);

        return kusto;
    }
}
