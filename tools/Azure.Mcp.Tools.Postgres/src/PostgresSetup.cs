// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Commands.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Postgres;

public class PostgresSetup : IAreaSetup
{
    public string Name => "postgres";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPostgresService, PostgresService>();

        services.AddSingleton<DatabaseListCommand>();
        services.AddSingleton<DatabaseQueryCommand>();

        services.AddSingleton<TableListCommand>();
        services.AddSingleton<TableSchemaGetCommand>();

        services.AddSingleton<ServerListCommand>();
        services.AddSingleton<ServerConfigGetCommand>();

        services.AddSingleton<ServerParamGetCommand>();
        services.AddSingleton<ServerParamSetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var pg = new CommandGroup(Name, "PostgreSQL operations - Commands for managing Azure Database for PostgreSQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.");

        var database = new CommandGroup("database", "PostgreSQL database operations");
        pg.AddSubGroup(database);

        var databaseList = serviceProvider.GetRequiredService<DatabaseListCommand>();
        database.AddCommand(databaseList.Name, databaseList);
        var databaseQuery = serviceProvider.GetRequiredService<DatabaseQueryCommand>();
        database.AddCommand(databaseQuery.Name, databaseQuery);

        var table = new CommandGroup("table", "PostgreSQL table operations");
        pg.AddSubGroup(table);

        var tableList = serviceProvider.GetRequiredService<TableListCommand>();
        table.AddCommand(tableList.Name, tableList);

        var schema = new CommandGroup("schema", "PostgreSQL table schema operations");
        table.AddSubGroup(schema);
        var tableSchemaGet = serviceProvider.GetRequiredService<TableSchemaGetCommand>();
        schema.AddCommand(tableSchemaGet.Name, tableSchemaGet);

        var server = new CommandGroup("server", "PostgreSQL server operations");
        pg.AddSubGroup(server);
        var serverList = serviceProvider.GetRequiredService<ServerListCommand>();
        server.AddCommand(serverList.Name, serverList);

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
