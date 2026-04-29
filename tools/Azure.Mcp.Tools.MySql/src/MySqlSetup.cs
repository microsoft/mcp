// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.MySql;

public class MySqlSetup : IAreaSetup
{
    public string Name => "mysql";

    public string Title => "Azure Database for MySQL";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMySqlService, MySqlService>();

        services.AddSingleton<MySqlListCommand>();
        services.AddSingleton<DatabaseQueryCommand>();
        services.AddSingleton<TableSchemaGetCommand>();
        services.AddSingleton<ServerConfigGetCommand>();
        services.AddSingleton<ServerParamGetCommand>();
        services.AddSingleton<ServerParamSetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var mysql = new CommandGroup(Name, "MySQL operations - Commands for managing Azure Database for MySQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.", Title);

        // Consolidated hierarchical list command
        mysql.AddCommand(serviceProvider.GetRequiredService<MySqlListCommand>());

        var database = new CommandGroup("database", "MySQL database operations");
        mysql.AddSubGroup(database);

        database.AddCommand(serviceProvider.GetRequiredService<DatabaseQueryCommand>());

        var table = new CommandGroup("table", "MySQL table operations");
        mysql.AddSubGroup(table);

        var schema = new CommandGroup("schema", "MySQL table schema operations");
        table.AddSubGroup(schema);
        schema.AddCommand(serviceProvider.GetRequiredService<TableSchemaGetCommand>());

        var server = new CommandGroup("server", "MySQL server operations");
        mysql.AddSubGroup(server);

        var config = new CommandGroup("config", "MySQL server configuration operations");
        server.AddSubGroup(config);
        config.AddCommand(serviceProvider.GetRequiredService<ServerConfigGetCommand>());

        var param = new CommandGroup("param", "MySQL server parameter operations");
        server.AddSubGroup(param);

        param.AddCommand(serviceProvider.GetRequiredService<ServerParamGetCommand>());
        param.AddCommand(serviceProvider.GetRequiredService<ServerParamSetCommand>());

        return mysql;
    }
}
