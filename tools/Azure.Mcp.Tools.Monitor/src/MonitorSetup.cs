// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Azure.Resource;
using Azure.Mcp.Tools.Monitor.Commands.ActivityLog;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Monitor;

public class MonitorSetup : IAreaSetup
{
    public string Name => "monitor";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IMonitorService, MonitorService>();
        services.AddSingleton<IMonitorService, MonitorService>();
        services.AddSingleton<IMonitorHealthModelService, MonitorHealthModelService>();
        services.AddSingleton<IMonitorWebTestService, MonitorWebTestService>();
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

        services.AddSingleton<ActivityLogListCommand>();

        services.AddSingleton<WebTestsGetCommand>();
        services.AddSingleton<WebTestsListCommand>();
        services.AddSingleton<WebTestsCreateCommand>();
        services.AddSingleton<WebTestsUpdateCommand>();
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

        var workspaceLogQuery = serviceProvider.GetRequiredService<WorkspaceLogQueryCommand>();
        workspaceLogs.AddCommand(workspaceLogQuery.Name, workspaceLogQuery);
        var resourceLogQuery = serviceProvider.GetRequiredService<ResourceLogQueryCommand>();
        resourceLogs.AddCommand(resourceLogQuery.Name, resourceLogQuery);

        var workspaceList = serviceProvider.GetRequiredService<WorkspaceListCommand>();
        workspaces.AddCommand(workspaceList.Name, workspaceList);
        var tableList = serviceProvider.GetRequiredService<TableListCommand>();
        monitorTable.AddCommand(tableList.Name, tableList);

        var tableTypeList = serviceProvider.GetRequiredService<TableTypeListCommand>();
        monitorTableType.AddCommand(tableTypeList.Name, tableTypeList);

        var health = new CommandGroup("healthmodels", "Azure Monitor Health Models operations - Commands for working with Azure Monitor Health Models.");
        monitor.AddSubGroup(health);

        var entity = new CommandGroup("entity", "Entity operations - Commands for working with entities in Azure Monitor Health Models.");
        health.AddSubGroup(entity);

        var entityGetHealth = serviceProvider.GetRequiredService<EntityGetHealthCommand>();
        entity.AddCommand(entityGetHealth.Name, entityGetHealth);

        // Create Metrics command group and register commands
        var metrics = new CommandGroup("metrics", "Azure Monitor metrics operations - Commands for querying and analyzing Azure Monitor metrics.");
        monitor.AddSubGroup(metrics);

        var metricsQuery = serviceProvider.GetRequiredService<MetricsQueryCommand>();
        metrics.AddCommand(metricsQuery.Name, metricsQuery);
        var metricsDefinitions = serviceProvider.GetRequiredService<MetricsDefinitionsCommand>();
        metrics.AddCommand(metricsDefinitions.Name, metricsDefinitions);

        var activityLog = new CommandGroup("activitylog", "Azure Monitor activity log operations - Commands for querying and analyzing activity logs for Azure resources.");
        monitor.AddSubGroup(activityLog);

        var activityLogList = serviceProvider.GetRequiredService<ActivityLogListCommand>();
        activityLog.AddCommand(activityLogList.Name, activityLogList);

        // Register Monitor.WebTest sub-group commands
        var webTests = new CommandGroup("webtests", "Azure Monitor Web Test operations - Commands for working with Azure Availability/Web Tests.");
        monitor.AddSubGroup(webTests);

        var webTestGet = serviceProvider.GetRequiredService<WebTestsGetCommand>();
        webTests.AddCommand(webTestGet.Name, webTestGet);
        var webTestList = serviceProvider.GetRequiredService<WebTestsListCommand>();
        webTests.AddCommand(webTestList.Name, webTestList);
        var webTestCreate = serviceProvider.GetRequiredService<WebTestsCreateCommand>();
        webTests.AddCommand(webTestCreate.Name, webTestCreate);
        var webTestUpdate = serviceProvider.GetRequiredService<WebTestsUpdateCommand>();
        webTests.AddCommand(webTestUpdate.Name, webTestUpdate);

        return monitor;
    }
}
