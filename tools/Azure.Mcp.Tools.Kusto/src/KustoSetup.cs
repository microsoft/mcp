// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.Kusto;

public class KustoSetup : IAreaSetup
{
    public string Name => "kusto";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IKustoService, KustoService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Kusto command group
        var kusto = new CommandGroup(Name, "Kusto operations - Commands for managing and querying Azure Data Explorer (Kusto) resources. Includes operations for listing clusters and databases, executing KQL queries, retrieving table schemas, and working with Kusto data analytics workloads.");
        rootGroup.AddSubGroup(kusto);

        // Create Kusto cluster subgroups
        var clusters = new CommandGroup("cluster", "Kusto cluster operations - Commands for listing clusters in your Azure subscription.");
        kusto.AddSubGroup(clusters);

        var databases = new CommandGroup("database", "Kusto database operations - Commands for listing databases in a cluster.");
        kusto.AddSubGroup(databases);

        var tables = new CommandGroup("table", "Kusto table operations - Commands for listing tables in a database.");
        kusto.AddSubGroup(tables);

        kusto.AddCommand("sample", new SampleCommand(loggerFactory.CreateLogger<SampleCommand>()));
        kusto.AddCommand("query", new QueryCommand(loggerFactory.CreateLogger<QueryCommand>()));

        clusters.AddCommand("list", new ClusterListCommand(loggerFactory.CreateLogger<ClusterListCommand>()));
        clusters.AddCommand("get", new ClusterGetCommand(loggerFactory.CreateLogger<ClusterGetCommand>()));

        databases.AddCommand("list", new DatabaseListCommand(loggerFactory.CreateLogger<DatabaseListCommand>()));

        tables.AddCommand("list", new TableListCommand(loggerFactory.CreateLogger<TableListCommand>()));
        tables.AddCommand("schema", new TableSchemaCommand(loggerFactory.CreateLogger<TableSchemaCommand>()));
    }
}
