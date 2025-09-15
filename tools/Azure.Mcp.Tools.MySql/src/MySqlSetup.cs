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
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var mysql = new CommandGroup(Name, "MySQL operations - Commands for managing Azure Database for MySQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.");
        rootGroup.AddSubGroup(mysql);

        var database = new CommandGroup("database", "MySQL database operations");
        mysql.AddSubGroup(database);
        database.AddCommand("list", new DatabaseListCommand(loggerFactory.CreateLogger<DatabaseListCommand>()));
        database.AddCommand("query", new DatabaseQueryCommand(loggerFactory.CreateLogger<DatabaseQueryCommand>()));

        var table = new CommandGroup("table", "MySQL table operations");
        mysql.AddSubGroup(table);
        table.AddCommand("list", new TableListCommand(loggerFactory.CreateLogger<TableListCommand>()));

        var schema = new CommandGroup("schema", "MySQL table schema operations");
        table.AddSubGroup(schema);
        schema.AddCommand("get", new TableSchemaGetCommand(loggerFactory.CreateLogger<TableSchemaGetCommand>()));

        var server = new CommandGroup("server", "MySQL server operations");
        mysql.AddSubGroup(server);
        server.AddCommand("list", new ServerListCommand(loggerFactory.CreateLogger<ServerListCommand>()));

        var config = new CommandGroup("config", "MySQL server configuration operations");
        server.AddSubGroup(config);
        config.AddCommand("get", new ServerConfigGetCommand(loggerFactory.CreateLogger<ServerConfigGetCommand>()));

        var param = new CommandGroup("param", "MySQL server parameter operations");
        server.AddSubGroup(param);
        param.AddCommand("get", new ServerParamGetCommand(loggerFactory.CreateLogger<ServerParamGetCommand>()));
        param.AddCommand("set", new ServerParamSetCommand(loggerFactory.CreateLogger<ServerParamSetCommand>()));
    }
}
