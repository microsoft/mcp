// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Monitor.Commands.ActivityLog;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor;

public class MonitorSetup : IAreaSetup
{
    public string Name => "monitor";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IMonitorService, MonitorService>();
        services.AddSingleton<IMonitorService, MonitorService>();
        services.AddSingleton<IMonitorHealthModelService, MonitorHealthModelService>();
        services.AddSingleton<IResourceResolverService, ResourceResolverService>();
        services.AddSingleton<IMetricsQueryClientService, MetricsQueryClientService>();
        services.AddSingleton<IMonitorMetricsService, MonitorMetricsService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Monitor command group
        var monitor = new CommandGroup(Name, "Azure Monitor operations - Commands for querying and analyzing Azure Monitor logs and metrics.");
        rootGroup.AddSubGroup(monitor);

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

        workspaceLogs.AddCommand("query", new WorkspaceLogQueryCommand(loggerFactory.CreateLogger<WorkspaceLogQueryCommand>()));
        resourceLogs.AddCommand("query", new ResourceLogQueryCommand(loggerFactory.CreateLogger<ResourceLogQueryCommand>()));

        workspaces.AddCommand("list", new WorkspaceListCommand(loggerFactory.CreateLogger<WorkspaceListCommand>()));
        monitorTable.AddCommand("list", new TableListCommand(loggerFactory.CreateLogger<TableListCommand>()));

        monitorTableType.AddCommand("list", new TableTypeListCommand(loggerFactory.CreateLogger<TableTypeListCommand>()));

        var health = new CommandGroup("healthmodels", "Azure Monitor Health Models operations - Commands for working with Azure Monitor Health Models.");
        monitor.AddSubGroup(health);

        var entity = new CommandGroup("entity", "Entity operations - Commands for working with entities in Azure Monitor Health Models.");
        health.AddSubGroup(entity);

        entity.AddCommand("gethealth", new EntityGetHealthCommand(loggerFactory.CreateLogger<EntityGetHealthCommand>()));

        // Create Metrics command group and register commands
        var metrics = new CommandGroup("metrics", "Azure Monitor metrics operations - Commands for querying and analyzing Azure Monitor metrics.");
        monitor.AddSubGroup(metrics);

        metrics.AddCommand("query", new MetricsQueryCommand(loggerFactory.CreateLogger<MetricsQueryCommand>()));
        metrics.AddCommand("definitions", new MetricsDefinitionsCommand(loggerFactory.CreateLogger<MetricsDefinitionsCommand>()));

        // Create ActivityLog command group and register commands
        var activityLog = new CommandGroup("activitylog", "Azure Monitor activity log operations - Commands for querying and analyzing activity logs for Azure resources.");
        monitor.AddSubGroup(activityLog);

        activityLog.AddCommand("list", new ActivityLogListCommand(loggerFactory.CreateLogger<ActivityLogListCommand>()));
    }
}
