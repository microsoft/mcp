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
        database.AddCommand("list", serviceProvider.GetRequiredService<DatabaseListCommand>());
        database.AddCommand("query", serviceProvider.GetRequiredService<DatabaseQueryCommand>());

        var table = new CommandGroup("table", "PostgreSQL table operations");
        pg.AddSubGroup(table);
        table.AddCommand("list", serviceProvider.GetRequiredService<TableListCommand>());

        var schema = new CommandGroup("schema", "PostgreSQL table schema operations");
        table.AddSubGroup(schema);
        schema.AddCommand("get", serviceProvider.GetRequiredService<TableSchemaGetCommand>());

        var server = new CommandGroup("server", "PostgreSQL server operations");
        pg.AddSubGroup(server);
        server.AddCommand("list", serviceProvider.GetRequiredService<ServerListCommand>());

        var config = new CommandGroup("config", "PostgreSQL server configuration operations");
        server.AddSubGroup(config);
        config.AddCommand("get", serviceProvider.GetRequiredService<ServerConfigGetCommand>());

        var param = new CommandGroup("param", "PostgreSQL server parameter operations");
        server.AddSubGroup(param);
        param.AddCommand("get", serviceProvider.GetRequiredService<ServerParamGetCommand>());
        param.AddCommand("set", serviceProvider.GetRequiredService<ServerParamSetCommand>());

        return pg;
    }
}
