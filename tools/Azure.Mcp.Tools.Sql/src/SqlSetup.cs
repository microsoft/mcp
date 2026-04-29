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

        database.AddCommand(serviceProvider.GetRequiredService<DatabaseGetCommand>());
        database.AddCommand(serviceProvider.GetRequiredService<DatabaseCreateCommand>());
        database.AddCommand(serviceProvider.GetRequiredService<DatabaseRenameCommand>());
        database.AddCommand(serviceProvider.GetRequiredService<DatabaseUpdateCommand>());
        database.AddCommand(serviceProvider.GetRequiredService<DatabaseDeleteCommand>());

        var server = new CommandGroup("server", "SQL server operations");
        sql.AddSubGroup(server);

        server.AddCommand(serviceProvider.GetRequiredService<ServerGetCommand>());
        server.AddCommand(serviceProvider.GetRequiredService<ServerCreateCommand>());
        server.AddCommand(serviceProvider.GetRequiredService<ServerDeleteCommand>());

        var elasticPool = new CommandGroup("elastic-pool", "SQL elastic pool operations");
        sql.AddSubGroup(elasticPool);
        elasticPool.AddCommand(serviceProvider.GetRequiredService<ElasticPoolListCommand>());

        var entraAdmin = new CommandGroup("entra-admin", "SQL server Microsoft Entra ID administrator operations");
        server.AddSubGroup(entraAdmin);

        entraAdmin.AddCommand(serviceProvider.GetRequiredService<EntraAdminListCommand>());

        var firewallRule = new CommandGroup("firewall-rule", "SQL server firewall rule operations");
        server.AddSubGroup(firewallRule);

        firewallRule.AddCommand(serviceProvider.GetRequiredService<FirewallRuleListCommand>());
        firewallRule.AddCommand(serviceProvider.GetRequiredService<FirewallRuleCreateCommand>());
        firewallRule.AddCommand(serviceProvider.GetRequiredService<FirewallRuleDeleteCommand>());

        return sql;
    }
}
