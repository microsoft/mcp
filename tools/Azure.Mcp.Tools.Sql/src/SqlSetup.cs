// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql;

public class SqlSetup : IAreaSetup
{
    public string Name => "sql";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISqlService, SqlService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var sql = new CommandGroup(Name, "Azure SQL operations - Commands for managing Azure SQL databases, servers, and elastic pools. Includes operations for listing databases, configuring server settings, managing firewall rules, Entra ID administrators, and elastic pool resources.");
        rootGroup.AddSubGroup(sql);

        var database = new CommandGroup("db", "SQL database operations");
        sql.AddSubGroup(database);

        database.AddCommand("show", new DatabaseShowCommand(loggerFactory.CreateLogger<DatabaseShowCommand>()));
        database.AddCommand("list", new DatabaseListCommand(loggerFactory.CreateLogger<DatabaseListCommand>()));
        database.AddCommand("create", new DatabaseCreateCommand(loggerFactory.CreateLogger<DatabaseCreateCommand>()));
        database.AddCommand("delete", new DatabaseDeleteCommand(loggerFactory.CreateLogger<DatabaseDeleteCommand>()));

        var server = new CommandGroup("server", "SQL server operations");
        sql.AddSubGroup(server);

        server.AddCommand("create", new ServerCreateCommand(loggerFactory.CreateLogger<ServerCreateCommand>()));
        server.AddCommand("delete", new ServerDeleteCommand(loggerFactory.CreateLogger<ServerDeleteCommand>()));
        server.AddCommand("show", new ServerShowCommand(loggerFactory.CreateLogger<ServerShowCommand>()));

        var elasticPool = new CommandGroup("elastic-pool", "SQL elastic pool operations");
        sql.AddSubGroup(elasticPool);
        elasticPool.AddCommand("list", new ElasticPoolListCommand(loggerFactory.CreateLogger<ElasticPoolListCommand>()));

        var entraAdmin = new CommandGroup("entra-admin", "SQL server Microsoft Entra ID administrator operations");
        server.AddSubGroup(entraAdmin);
        entraAdmin.AddCommand("list", new EntraAdminListCommand(loggerFactory.CreateLogger<EntraAdminListCommand>()));

        var firewallRule = new CommandGroup("firewall-rule", "SQL server firewall rule operations");
        server.AddSubGroup(firewallRule);

        firewallRule.AddCommand("list", new FirewallRuleListCommand(loggerFactory.CreateLogger<FirewallRuleListCommand>()));
        firewallRule.AddCommand("create", new FirewallRuleCreateCommand(loggerFactory.CreateLogger<FirewallRuleCreateCommand>()));
        firewallRule.AddCommand("delete", new FirewallRuleDeleteCommand(loggerFactory.CreateLogger<FirewallRuleDeleteCommand>()));
    }
}
