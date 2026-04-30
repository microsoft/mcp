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
        var pg = new CommandGroup(Name, "PostgreSQL operations - Commands for managing Azure Database for PostgreSQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.", Title);

        // Consolidated hierarchical list command
        pg.AddCommand<PostgresListCommand>(serviceProvider);

        var database = new CommandGroup("database", "PostgreSQL database operations");
        pg.AddSubGroup(database);

        database.AddCommand<DatabaseQueryCommand>(serviceProvider);

        var table = new CommandGroup("table", "PostgreSQL table operations");
        pg.AddSubGroup(table);

        var schema = new CommandGroup("schema", "PostgreSQL table schema operations");
        table.AddSubGroup(schema);
        schema.AddCommand<TableSchemaGetCommand>(serviceProvider);

        var server = new CommandGroup("server", "PostgreSQL server operations");
        pg.AddSubGroup(server);

        var config = new CommandGroup("config", "PostgreSQL server configuration operations");
        server.AddSubGroup(config);
        config.AddCommand<ServerConfigGetCommand>(serviceProvider);

        var param = new CommandGroup("param", "PostgreSQL server parameter operations");
        server.AddSubGroup(param);
        param.AddCommand<ServerParamGetCommand>(serviceProvider);
        param.AddCommand<ServerParamSetCommand>(serviceProvider);

        return pg;
    }
}
