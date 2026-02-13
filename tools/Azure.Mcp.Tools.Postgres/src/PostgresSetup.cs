// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Auth;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Commands.Table;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Postgres;

public class PostgresSetup : IAreaSetup
{
    public string Name => "postgres";

    public string Title => "Azure Database for PostgreSQL";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEntraTokenProvider, EntraTokenProvider>();
        services.AddSingleton<IDbProvider, DbProvider>();
        services.AddSingleton<IPostgresService, PostgresService>();

        services.AddSingleton<PostgresListCommand>();
        services.AddSingleton<DatabaseQueryCommand>();
        services.AddSingleton<TableSchemaGetCommand>();
        services.AddSingleton<ServerConfigGetCommand>();
        services.AddSingleton<ServerParamGetCommand>();
        services.AddSingleton<ServerParamSetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var pg = new CommandGroup(Name, "PostgreSQL operations – Commands to manage Azure Database for PostgreSQL Flexible Server resources, including listing servers and databases, executing SQL queries, managing schemas, and configuring server parameters.", Title);

        // Consolidated hierarchical list command
        var postgresList = serviceProvider.GetRequiredService<PostgresListCommand>();
        pg.AddCommand(postgresList.Name, postgresList);

        var database = new CommandGroup("database", "PostgreSQL database operations");
        pg.AddSubGroup(database);

        var databaseQuery = serviceProvider.GetRequiredService<DatabaseQueryCommand>();
        database.AddCommand(databaseQuery.Name, databaseQuery);

        var table = new CommandGroup("table", "PostgreSQL table operations");
        pg.AddSubGroup(table);

        var schema = new CommandGroup("schema", "PostgreSQL table schema operations");
        table.AddSubGroup(schema);
        var tableSchemaGet = serviceProvider.GetRequiredService<TableSchemaGetCommand>();
        schema.AddCommand(tableSchemaGet.Name, tableSchemaGet);

        var server = new CommandGroup("server", "PostgreSQL server operations");
        pg.AddSubGroup(server);

        var config = new CommandGroup("config", "PostgreSQL server configuration operations");
        server.AddSubGroup(config);
        var serverConfigGet = serviceProvider.GetRequiredService<ServerConfigGetCommand>();
        config.AddCommand(serverConfigGet.Name, serverConfigGet);

        var param = new CommandGroup("param", "PostgreSQL server parameter operations");
        server.AddSubGroup(param);
        var serverParamGet = serviceProvider.GetRequiredService<ServerParamGetCommand>();
        param.AddCommand(serverParamGet.Name, serverParamGet);
        var serverParamSet = serviceProvider.GetRequiredService<ServerParamSetCommand>();
        param.AddCommand(serverParamSet.Name, serverParamSet);

        return pg;
    }
}
