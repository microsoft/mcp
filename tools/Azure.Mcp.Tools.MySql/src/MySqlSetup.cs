// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql;

public class MySqlSetup : IAreaSetup
{
    public string Name => "mysql";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMySqlService, MySqlService>();

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
        var mysql = new CommandGroup(Name, "MySQL operations - Commands for managing Azure Database for MySQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.");

        var database = new CommandGroup("database", "MySQL database operations");
        mysql.AddSubGroup(database);
        database.AddCommand("list", serviceProvider.GetRequiredService<DatabaseListCommand>());
        database.AddCommand("query", serviceProvider.GetRequiredService<DatabaseQueryCommand>());

        var table = new CommandGroup("table", "MySQL table operations");
        mysql.AddSubGroup(table);
        table.AddCommand("list", serviceProvider.GetRequiredService<TableListCommand>());

        var schema = new CommandGroup("schema", "MySQL table schema operations");
        table.AddSubGroup(schema);
        schema.AddCommand("get", serviceProvider.GetRequiredService<TableSchemaGetCommand>());

        var server = new CommandGroup("server", "MySQL server operations");
        mysql.AddSubGroup(server);
        server.AddCommand("list", serviceProvider.GetRequiredService<ServerListCommand>());

        var config = new CommandGroup("config", "MySQL server configuration operations");
        server.AddSubGroup(config);
        config.AddCommand("get", serviceProvider.GetRequiredService<ServerConfigGetCommand>());

        var param = new CommandGroup("param", "MySQL server parameter operations");
        server.AddSubGroup(param);
        param.AddCommand("get", serviceProvider.GetRequiredService<ServerParamGetCommand>());
        param.AddCommand("set", serviceProvider.GetRequiredService<ServerParamSetCommand>());

        return mysql;
    }
}
