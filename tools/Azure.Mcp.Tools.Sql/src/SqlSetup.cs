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

        database.AddCommand("show", serviceProvider.GetRequiredService<DatabaseShowCommand>());
        database.AddCommand("list", serviceProvider.GetRequiredService<DatabaseListCommand>());
        database.AddCommand("create", serviceProvider.GetRequiredService<DatabaseCreateCommand>());
        database.AddCommand("update", serviceProvider.GetRequiredService<DatabaseUpdateCommand>());
        database.AddCommand("delete", serviceProvider.GetRequiredService<DatabaseDeleteCommand>());

        var server = new CommandGroup("server", "SQL server operations");
        sql.AddSubGroup(server);

        server.AddCommand("create", serviceProvider.GetRequiredService<ServerCreateCommand>());
        server.AddCommand("delete", serviceProvider.GetRequiredService<ServerDeleteCommand>());
        server.AddCommand("list", serviceProvider.GetRequiredService<ServerListCommand>());
        server.AddCommand("show", serviceProvider.GetRequiredService<ServerShowCommand>());

        var elasticPool = new CommandGroup("elastic-pool", "SQL elastic pool operations");
        sql.AddSubGroup(elasticPool);
        elasticPool.AddCommand("list", serviceProvider.GetRequiredService<ElasticPoolListCommand>());

        var entraAdmin = new CommandGroup("entra-admin", "SQL server Microsoft Entra ID administrator operations");
        server.AddSubGroup(entraAdmin);
        entraAdmin.AddCommand("list", serviceProvider.GetRequiredService<EntraAdminListCommand>());

        var firewallRule = new CommandGroup("firewall-rule", "SQL server firewall rule operations");
        server.AddSubGroup(firewallRule);

        firewallRule.AddCommand("list", serviceProvider.GetRequiredService<FirewallRuleListCommand>());
        firewallRule.AddCommand("create", serviceProvider.GetRequiredService<FirewallRuleCreateCommand>());
        firewallRule.AddCommand("delete", serviceProvider.GetRequiredService<FirewallRuleDeleteCommand>());

        return sql;
    }
}
