// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Sql;

public class SqlSetup : IAreaSetup
{
    public string Name => "sql";

    public string Title => "Azure SQL Database";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISqlService, SqlService>();

        services.AddSingleton<DatabaseGetCommand>();
        services.AddSingleton<DatabaseCreateCommand>();
        services.AddSingleton<DatabaseRenameCommand>();
        services.AddSingleton<DatabaseUpdateCommand>();
        services.AddSingleton<DatabaseDeleteCommand>();

        services.AddSingleton<ServerGetCommand>();
        services.AddSingleton<ServerCreateCommand>();
        services.AddSingleton<ServerDeleteCommand>();

        services.AddSingleton<ElasticPoolListCommand>();

        services.AddSingleton<EntraAdminListCommand>();

        services.AddSingleton<FirewallRuleListCommand>();
        services.AddSingleton<FirewallRuleCreateCommand>();
        services.AddSingleton<FirewallRuleDeleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var sql = new CommandGroup(Name, "Azure SQL operations - Commands for managing Azure SQL databases, servers, and elastic pools. Includes operations for listing databases, configuring server settings, managing firewall rules, Entra ID administrators, and elastic pool resources.", Title);

        var database = new CommandGroup("db", "SQL database operations");
        sql.AddSubGroup(database);

        database.AddCommand<DatabaseGetCommand>(serviceProvider);
        database.AddCommand<DatabaseCreateCommand>(serviceProvider);
        database.AddCommand<DatabaseRenameCommand>(serviceProvider);
        database.AddCommand<DatabaseUpdateCommand>(serviceProvider);
        database.AddCommand<DatabaseDeleteCommand>(serviceProvider);

        var server = new CommandGroup("server", "SQL server operations");
        sql.AddSubGroup(server);

        server.AddCommand<ServerGetCommand>(serviceProvider);
        server.AddCommand<ServerCreateCommand>(serviceProvider);
        server.AddCommand<ServerDeleteCommand>(serviceProvider);

        var elasticPool = new CommandGroup("elastic-pool", "SQL elastic pool operations");
        sql.AddSubGroup(elasticPool);
        elasticPool.AddCommand<ElasticPoolListCommand>(serviceProvider);

        var entraAdmin = new CommandGroup("entra-admin", "SQL server Microsoft Entra ID administrator operations");
        server.AddSubGroup(entraAdmin);

        entraAdmin.AddCommand<EntraAdminListCommand>(serviceProvider);

        var firewallRule = new CommandGroup("firewall-rule", "SQL server firewall rule operations");
        server.AddSubGroup(firewallRule);

        firewallRule.AddCommand<FirewallRuleListCommand>(serviceProvider);
        firewallRule.AddCommand<FirewallRuleCreateCommand>(serviceProvider);
        firewallRule.AddCommand<FirewallRuleDeleteCommand>(serviceProvider);

        return sql;
    }
}
