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

namespace Azure.Mcp.Tools.Sql;

public class SqlSetup : IAreaSetup
{
    public string Name => "sql";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISqlService, SqlService>();

        services.AddSingleton<DatabaseShowCommand>();
        services.AddSingleton<DatabaseListCommand>();
        services.AddSingleton<DatabaseCreateCommand>();
        services.AddSingleton<DatabaseUpdateCommand>();
        services.AddSingleton<DatabaseDeleteCommand>();

        services.AddSingleton<ServerCreateCommand>();
        services.AddSingleton<ServerDeleteCommand>();
        services.AddSingleton<ServerListCommand>();
        services.AddSingleton<ServerShowCommand>();

        services.AddSingleton<ElasticPoolListCommand>();

        services.AddSingleton<EntraAdminListCommand>();

        services.AddSingleton<FirewallRuleListCommand>();
        services.AddSingleton<FirewallRuleCreateCommand>();
        services.AddSingleton<FirewallRuleDeleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var sql = new CommandGroup(Name, "Azure SQL operations - Commands for managing Azure SQL databases, servers, and elastic pools. Includes operations for listing databases, configuring server settings, managing firewall rules, Entra ID administrators, and elastic pool resources.");

        var database = new CommandGroup("db", "SQL database operations");
        sql.AddSubGroup(database);

        var databaseShow = serviceProvider.GetRequiredService<DatabaseShowCommand>();
        database.AddCommand(databaseShow.Name, databaseShow);
        var databaseList = serviceProvider.GetRequiredService<DatabaseListCommand>();
        database.AddCommand(databaseList.Name, databaseList);
        var databaseCreate = serviceProvider.GetRequiredService<DatabaseCreateCommand>();
        database.AddCommand(databaseCreate.Name, databaseCreate);
        var databaseUpdate = serviceProvider.GetRequiredService<DatabaseUpdateCommand>();
        database.AddCommand(databaseUpdate.Name, databaseUpdate);
        var databaseDelete = serviceProvider.GetRequiredService<DatabaseDeleteCommand>();
        database.AddCommand(databaseDelete.Name, databaseDelete);

        var server = new CommandGroup("server", "SQL server operations");
        sql.AddSubGroup(server);

        var serverCreate = serviceProvider.GetRequiredService<ServerCreateCommand>();
        server.AddCommand(serverCreate.Name, serverCreate);
        var serverDelete = serviceProvider.GetRequiredService<ServerDeleteCommand>();
        server.AddCommand(serverDelete.Name, serverDelete);
        var serverList = serviceProvider.GetRequiredService<ServerListCommand>();
        server.AddCommand(serverList.Name, serverList);
        var serverShow = serviceProvider.GetRequiredService<ServerShowCommand>();
        server.AddCommand(serverShow.Name, serverShow);

        var elasticPool = new CommandGroup("elastic-pool", "SQL elastic pool operations");
        sql.AddSubGroup(elasticPool);
        var elasticPoolList = serviceProvider.GetRequiredService<ElasticPoolListCommand>();
        elasticPool.AddCommand(elasticPoolList.Name, elasticPoolList);

        var entraAdmin = new CommandGroup("entra-admin", "SQL server Microsoft Entra ID administrator operations");
        server.AddSubGroup(entraAdmin);

        var entraAdminList = serviceProvider.GetRequiredService<EntraAdminListCommand>();
        entraAdmin.AddCommand(entraAdminList.Name, entraAdminList);

        var firewallRule = new CommandGroup("firewall-rule", "SQL server firewall rule operations");
        server.AddSubGroup(firewallRule);

        var firewallRuleList = serviceProvider.GetRequiredService<FirewallRuleListCommand>();
        firewallRule.AddCommand(firewallRuleList.Name, firewallRuleList);
        var firewallRuleCreate = serviceProvider.GetRequiredService<FirewallRuleCreateCommand>();
        firewallRule.AddCommand(firewallRuleCreate.Name, firewallRuleCreate);
        var firewallRuleDelete = serviceProvider.GetRequiredService<FirewallRuleDeleteCommand>();
        firewallRule.AddCommand(firewallRuleDelete.Name, firewallRuleDelete);

        return sql;
    }
}
