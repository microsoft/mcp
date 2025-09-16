// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Monitor;

public class MonitorSetup : IAreaSetup
{
    public string Name => "monitor";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMonitorService, MonitorService>();
        services.AddSingleton<IMonitorHealthModelService, MonitorHealthModelService>();
        services.AddSingleton<IResourceResolverService, ResourceResolverService>();
        services.AddSingleton<IMetricsQueryClientService, MetricsQueryClientService>();
        services.AddSingleton<IMonitorMetricsService, MonitorMetricsService>();

        services.AddSingleton<WorkspaceLogQueryCommand>();
        services.AddSingleton<ResourceLogQueryCommand>();

        services.AddSingleton<WorkspaceListCommand>();
        services.AddSingleton<TableListCommand>();

        services.AddSingleton<TableTypeListCommand>();

        services.AddSingleton<EntityGetHealthCommand>();

        services.AddSingleton<MetricsQueryCommand>();
        services.AddSingleton<MetricsDefinitionsCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Monitor command group
        var monitor = new CommandGroup(Name, "Azure Monitor operations - Commands for querying and analyzing Azure Monitor logs and metrics.");

        // Create Monitor subgroups
        var workspaces = new CommandGroup("workspace", "Log Analytics workspace operations - Commands for managing Log Analytics workspaces.");
        monitor.AddSubGroup(workspaces);

        var resources = new CommandGroup("resource", "Azure Monitor resource operations - Commands for resource-centric operations.");
        monitor.AddSubGroup(resources);

        var monitorTable = new CommandGroup("table", "Log Analytics workspace table operations - Commands for listing tables in Log Analytics workspaces.");
        monitor.AddSubGroup(monitorTable);

        var monitorTableType = new CommandGroup("type", "Log Analytics workspace table type operations - Commands for listing table types in Log Analytics workspaces.");
        monitorTable.AddSubGroup(monitorTableType);

        var workspaceLogs = new CommandGroup("log", "Azure Monitor logs operations - Commands for querying Log Analytics workspaces using KQL.");
        workspaces.AddSubGroup(workspaceLogs);

        var resourceLogs = new CommandGroup("log", "Azure Monitor resource logs operations - Commands for querying resource logs using KQL.");
        resources.AddSubGroup(resourceLogs);

        // Register Monitor commands

        workspaceLogs.AddCommand("query", serviceProvider.GetRequiredService<WorkspaceLogQueryCommand>());
        resourceLogs.AddCommand("query", serviceProvider.GetRequiredService<ResourceLogQueryCommand>());

        workspaces.AddCommand("list", serviceProvider.GetRequiredService<WorkspaceListCommand>());
        monitorTable.AddCommand("list", serviceProvider.GetRequiredService<TableListCommand>());

        monitorTableType.AddCommand("list", serviceProvider.GetRequiredService<TableTypeListCommand>());

        var health = new CommandGroup("healthmodels", "Azure Monitor Health Models operations - Commands for working with Azure Monitor Health Models.");
        monitor.AddSubGroup(health);

        var entity = new CommandGroup("entity", "Entity operations - Commands for working with entities in Azure Monitor Health Models.");
        health.AddSubGroup(entity);

        entity.AddCommand("gethealth", serviceProvider.GetRequiredService<EntityGetHealthCommand>());

        // Create Metrics command group and register commands
        var metrics = new CommandGroup("metrics", "Azure Monitor metrics operations - Commands for querying and analyzing Azure Monitor metrics.");
        monitor.AddSubGroup(metrics);

        metrics.AddCommand("query", serviceProvider.GetRequiredService<MetricsQueryCommand>());
        metrics.AddCommand("definitions", serviceProvider.GetRequiredService<MetricsDefinitionsCommand>());

        return monitor;
    }
}
