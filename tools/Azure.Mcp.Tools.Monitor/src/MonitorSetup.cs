// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.ActivityLog;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Detectors;
using Azure.Mcp.Tools.Monitor.Generators;
using Azure.Mcp.Tools.Monitor.Pipeline;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor;

public class MonitorSetup : IAreaSetup
{
    public string Name => "monitor";

    public string Title => "Azure Monitor";
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMonitorService, MonitorService>();
        services.AddSingleton<IMonitorHealthModelService, MonitorHealthModelService>();
        services.AddSingleton<IMonitorWebTestService, MonitorWebTestService>();
        services.AddSingleton<IResourceResolverService, ResourceResolverService>();
        services.AddSingleton<IMonitorMetricsService, MonitorMetricsService>();

        services.AddSingleton<ILanguageDetector, DotNetLanguageDetector>();
        services.AddSingleton<ILanguageDetector, NodeJsLanguageDetector>();
        services.AddSingleton<ILanguageDetector, PythonLanguageDetector>();
        services.AddSingleton<IAppTypeDetector, DotNetAppTypeDetector>();
        services.AddSingleton<IAppTypeDetector, NodeJsAppTypeDetector>();
        services.AddSingleton<IAppTypeDetector, PythonAppTypeDetector>();
        services.AddSingleton<IInstrumentationDetector, DotNetInstrumentationDetector>();
        services.AddSingleton<IInstrumentationDetector, NodeJsInstrumentationDetector>();
        services.AddSingleton<IInstrumentationDetector, PythonInstrumentationDetector>();

        services.AddSingleton<IGenerator, AspNetCoreGreenfieldGenerator>();
        services.AddSingleton<IGenerator, AspNetCoreBrownfieldGenerator>();
        services.AddSingleton<IGenerator, AspNetClassicGreenfieldGenerator>();
        services.AddSingleton<IGenerator, AspNetClassicBrownfieldGenerator>();
        services.AddSingleton<IGenerator, WorkerServiceGreenfieldGenerator>();
        services.AddSingleton<IGenerator, WorkerServiceBrownfieldGenerator>();
        services.AddSingleton<IGenerator, ConsoleBrownfieldGenerator>();
        services.AddSingleton<IGenerator, DotNetEnhancementGenerator>();
        services.AddSingleton<IGenerator, ExpressGreenfieldGenerator>();
        services.AddSingleton<IGenerator, FastifyGreenfieldGenerator>();
        services.AddSingleton<IGenerator, NestJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, NextJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, LangchainJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, PostgresNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, MongoDBNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, RedisNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, MySQLNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, WinstonNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, BunyanNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, ConsoleNodeJsGreenfieldGenerator>();
        services.AddSingleton<IGenerator, PythonGreenfieldGenerator>();

        services.AddSingleton<WorkspaceAnalyzer>();
        services.AddSingleton<OrchestratorTool>();
        services.AddSingleton<SendBrownfieldAnalysisTool>();
        services.AddSingleton<SendEnhancementSelectTool>();

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
        services.AddSingleton<WebTestsCreateOrUpdateCommand>();

        services.AddSingleton<GetLearningResourceCommand>();
        services.AddSingleton<OrchestratorStartCommand>();
        services.AddSingleton<OrchestratorNextCommand>();
        services.AddSingleton<SendBrownfieldAnalysisCommand>();
        services.AddSingleton<SendEnhancementSelectCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Monitor command group
        var monitor = new CommandGroup(Name,
            """
            Monitor operations - Commands for managing Azure Monitor workspaces, querying and analyzing logs and metrics, listing
            tables and table types, working with health models and entities, web tests, and orchestrating instrumentation
            workflows. Use this tool to list Log Analytics workspaces, tables, and table types; run KQL queries against workspace
            and resource logs; retrieve health for monitor entities; query metrics and metric definitions; inspect resource activity
            logs; manage availability web tests; and guide instrumentation onboarding and enhancement flows. Covers Azure Monitor
            observability workflows. Set learn=true to discover sub-commands.
            """,
            Title);

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
        workspaceLogs.AddCommand(serviceProvider.GetRequiredService<WorkspaceLogQueryCommand>());
        workspaceLogs.AddCommand(serviceProvider.GetRequiredService<WorkspaceLogQueryCommand>());

        workspaces.AddCommand(serviceProvider.GetRequiredService<WorkspaceListCommand>());
        workspaces.AddCommand(serviceProvider.GetRequiredService<TableListCommand>());

        monitorTableType.AddCommand(serviceProvider.GetRequiredService<TableTypeListCommand>());

        var health = new CommandGroup("healthmodels", "Azure Monitor Health Models operations - Commands for working with Azure Monitor Health Models.");
        monitor.AddSubGroup(health);

        var entity = new CommandGroup("entity", "Entity operations - Commands for working with entities in Azure Monitor Health Models.");
        health.AddSubGroup(entity);

        entity.AddCommand(serviceProvider.GetRequiredService<EntityGetHealthCommand>());

        // Create Metrics command group and register commands
        var metrics = new CommandGroup("metrics", "Azure Monitor metrics operations - Commands for querying and analyzing Azure Monitor metrics.");
        monitor.AddSubGroup(metrics);

        metrics.AddCommand(serviceProvider.GetRequiredService<MetricsQueryCommand>());
        metrics.AddCommand(serviceProvider.GetRequiredService<MetricsDefinitionsCommand>());

        var activityLog = new CommandGroup("activitylog", "Azure Monitor activity log operations - Commands for querying and analyzing activity logs for Azure resources.");
        monitor.AddSubGroup(activityLog);

        activityLog.AddCommand(serviceProvider.GetRequiredService<ActivityLogListCommand>());

        // Register Monitor.WebTest sub-group commands
        var webTests = new CommandGroup("webtests", "Azure Monitor Web Test operations - Commands for working with Azure Availability/Web Tests.");
        monitor.AddSubGroup(webTests);

        webTests.AddCommand(serviceProvider.GetRequiredService<WebTestsGetCommand>());
        webTests.AddCommand(serviceProvider.GetRequiredService<WebTestsCreateOrUpdateCommand>());

        var instrumentation = new CommandGroup("instrumentation", "Azure Monitor instrumentation operations - Commands for orchestrated onboarding and migration steps.");
        monitor.AddSubGroup(instrumentation);

        instrumentation.AddCommand(serviceProvider.GetRequiredService<GetLearningResourceCommand>());
        instrumentation.AddCommand(serviceProvider.GetRequiredService<OrchestratorStartCommand>());
        instrumentation.AddCommand(serviceProvider.GetRequiredService<OrchestratorNextCommand>());
        instrumentation.AddCommand(serviceProvider.GetRequiredService<SendBrownfieldAnalysisCommand>());
        instrumentation.AddCommand(serviceProvider.GetRequiredService<SendEnhancementSelectCommand>());

        return monitor;
    }
}
