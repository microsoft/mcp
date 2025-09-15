// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Commands.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres;

public class PostgresSetup : IAreaSetup
{
    public string Name => "postgres";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPostgresService, PostgresService>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var pg = new CommandGroup(Name, "PostgreSQL operations - Commands for managing Azure Database for PostgreSQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.");
        rootGroup.AddSubGroup(pg);

        var database = new CommandGroup("database", "PostgreSQL database operations");
        pg.AddSubGroup(database);
        database.AddCommand("list", new DatabaseListCommand(loggerFactory.CreateLogger<DatabaseListCommand>()));
        database.AddCommand("query", new DatabaseQueryCommand(loggerFactory.CreateLogger<DatabaseQueryCommand>()));

        var table = new CommandGroup("table", "PostgreSQL table operations");
        pg.AddSubGroup(table);
        table.AddCommand("list", new TableListCommand(loggerFactory.CreateLogger<TableListCommand>()));

        var schema = new CommandGroup("schema", "PostgreSQL table schema operations");
        table.AddSubGroup(schema);
        schema.AddCommand("get", new TableSchemaGetCommand(loggerFactory.CreateLogger<TableSchemaGetCommand>()));

        var server = new CommandGroup("server", "PostgreSQL server operations");
        pg.AddSubGroup(server);
        server.AddCommand("list", new ServerListCommand(loggerFactory.CreateLogger<ServerListCommand>()));

        var config = new CommandGroup("config", "PostgreSQL server configuration operations");
        server.AddSubGroup(config);
        config.AddCommand("get", new ServerConfigGetCommand(loggerFactory.CreateLogger<ServerConfigGetCommand>()));

        var param = new CommandGroup("param", "PostgreSQL server parameter operations");
        server.AddSubGroup(param);
        param.AddCommand("get", new ServerParamGetCommand(loggerFactory.CreateLogger<ServerParamGetCommand>()));
        param.AddCommand("set", new ServerParamSetCommand(loggerFactory.CreateLogger<ServerParamSetCommand>()));
    }
}
