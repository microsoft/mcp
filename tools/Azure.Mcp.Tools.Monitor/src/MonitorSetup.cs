// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Commands.ActivityLog;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels;
using Azure.Mcp.Tools.Monitor.Commands.Ingestion;
using Azure.Mcp.Tools.Monitor.Commands.Instrumentation;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;
using Azure.Mcp.Tools.Monitor.Instrumentation.Generators;
using Azure.Mcp.Tools.Monitor.Instrumentation.Pipeline;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Azure.Mcp.Tools.Monitor.Tools.Instrumentation;
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
        services.AddSingleton<IMonitorLogSearchService, MonitorLogSearchService>();
        services.AddSingleton<IMonitorHealthModelService, MonitorHealthModelService>();
        services.AddSingleton<IMonitorWebTestService, MonitorWebTestService>();
        services.AddSingleton<IResourceResolverService, ResourceResolverService>();
        services.AddSingleton<IMonitorMetricsService, MonitorMetricsService>();
        services.AddSingleton<IMonitorIngestionService, MonitorIngestionService>();
        services.AddSingleton<IMonitorTracesService, MonitorTracesService>();

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

        services.AddSingleton<WorkspaceLogQueryCommand>();
        services.AddSingleton<WorkspaceLogSearchCommand>();
        services.AddSingleton<ResourceLogQueryCommand>();

        services.AddSingleton<WorkspaceListCommand>();
        services.AddSingleton<TableListCommand>();

        services.AddSingleton<TableTypeListCommand>();

        services.AddSingleton<HealthModelListCommand>();
        services.AddSingleton<HealthModelGetCommand>();

        services.AddSingleton<MetricsQueryCommand>();
        services.AddSingleton<MetricsBatchQueryCommand>();
        services.AddSingleton<MetricsDefinitionsCommand>();

        services.AddSingleton<ActivityLogListCommand>();

        services.AddSingleton<IngestionUploadCommand>();
        services.AddSingleton<IngestionValidateCommand>();

        services.AddSingleton<WebTestsGetCommand>();
        services.AddSingleton<WebTestsCreateOrUpdateCommand>();

        services.AddSingleton<GetLearningResourceCommand>();
        services.AddSingleton<OrchestratorStartCommand>();
        services.AddSingleton<OrchestratorNextCommand>();
        services.AddSingleton<SendBrownfieldAnalysisCommand>();
        services.AddSingleton<SendEnhancementSelectCommand>();

        services.AddSingleton<TraceAccountListCommand>();
        services.AddSingleton<TraceFieldListCommand>();
        services.AddSingleton<TraceQueryRunCommand>();
        services.AddSingleton<TraceQueryResumeCommand>();
        services.AddSingleton<TraceQueryCancelCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Monitor command group
        var monitor = new CommandGroup(Name,
            "Monitor operations - Commands for managing Azure Monitor workspaces, querying and analyzing logs and " +
            "metrics, listing tables and table types, working with health models and entities, web tests, and " +
            "orchestrating instrumentation workflows. Use this tool to list Log Analytics workspaces, tables, and " +
            "table types; run KQL queries against workspace and resource logs; retrieve health for monitor entities; " +
            "query metrics and metric definitions; inspect resource activity logs; manage availability web tests; " +
            "and guide instrumentation onboarding and enhancement flows. Covers Azure Monitor observability " +
            "workflows. Set learn=true to discover sub-commands.",
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
        workspaceLogs.AddCommand<WorkspaceLogQueryCommand>(serviceProvider);
        workspaceLogs.AddCommand<WorkspaceLogSearchCommand>(serviceProvider);

        resourceLogs.AddCommand<ResourceLogQueryCommand>(serviceProvider);

        workspaces.AddCommand<WorkspaceListCommand>(serviceProvider);

        monitorTable.AddCommand<TableListCommand>(serviceProvider);

        monitorTableType.AddCommand<TableTypeListCommand>(serviceProvider);

        var health = new CommandGroup("healthmodels", "Azure Monitor Health Models operations - Commands for listing and retrieving Azure Monitor Health Models (Microsoft.CloudHealth/healthmodels).");
        monitor.AddSubGroup(health);

        health.AddCommand<HealthModelListCommand>(serviceProvider);
        health.AddCommand<HealthModelGetCommand>(serviceProvider);

        // Create Metrics command group and register commands
        var metrics = new CommandGroup("metrics", "Azure Monitor metrics operations - Commands for querying and analyzing Azure Monitor metrics.");
        monitor.AddSubGroup(metrics);

        metrics.AddCommand<MetricsQueryCommand>(serviceProvider);
        metrics.AddCommand<MetricsBatchQueryCommand>(serviceProvider);
        metrics.AddCommand<MetricsDefinitionsCommand>(serviceProvider);

        var activityLog = new CommandGroup("activitylog", "Azure Monitor activity log operations - Commands for querying and analyzing activity logs for Azure resources.");
        monitor.AddSubGroup(activityLog);

        activityLog.AddCommand<ActivityLogListCommand>(serviceProvider);

        var ingestion = new CommandGroup("ingestion", "Azure Monitor log ingestion operations - Commands for uploading custom log data through data collection rules and validating it beforehand.");
        monitor.AddSubGroup(ingestion);

        ingestion.AddCommand<IngestionUploadCommand>(serviceProvider);
        ingestion.AddCommand<IngestionValidateCommand>(serviceProvider);

        // Register Monitor.WebTest sub-group commands
        var webTests = new CommandGroup("webtests", "Azure Monitor Web Test operations - Commands for working with Azure Availability/Web Tests.");
        monitor.AddSubGroup(webTests);

        webTests.AddCommand<WebTestsGetCommand>(serviceProvider);
        webTests.AddCommand<WebTestsCreateOrUpdateCommand>(serviceProvider);

        var instrumentation = new CommandGroup("instrumentation", "Azure Monitor instrumentation operations - Commands for orchestrated onboarding and migration steps.");
        monitor.AddSubGroup(instrumentation);

        instrumentation.AddCommand<GetLearningResourceCommand>(serviceProvider);
        instrumentation.AddCommand<OrchestratorStartCommand>(serviceProvider);
        instrumentation.AddCommand<OrchestratorNextCommand>(serviceProvider);
        instrumentation.AddCommand<SendBrownfieldAnalysisCommand>(serviceProvider);
        instrumentation.AddCommand<SendEnhancementSelectCommand>(serviceProvider);

        // Distributed tracing - a Monitor sub-feature backed by
        // Microsoft.Monitor/accounts/traceContainers on an Azure Monitor Workspace.
        var traces = new CommandGroup("traces", "Azure Monitor distributed-trace operations - Commands for listing distributed-trace accounts (Azure Monitor Workspaces enabled for Trace Store) and querying trace data using the caller's Azure identity.");
        monitor.AddSubGroup(traces);

        var traceAccounts = new CommandGroup("account", "Distributed-trace account operations - Commands for listing Azure Monitor Workspaces enabled for Trace Store.");
        traces.AddSubGroup(traceAccounts);
        traceAccounts.AddCommand<TraceAccountListCommand>(serviceProvider);

        var traceFields = new CommandGroup("field", "Distributed-trace field-name operations - Commands for discovering the trace + span schema exposed by a trace account.");
        traces.AddSubGroup(traceFields);
        traceFields.AddCommand<TraceFieldListCommand>(serviceProvider);

        var traceQuery = new CommandGroup("query", "Distributed-trace query operations - Commands for running, resuming, and cancelling queries against Trace Store.");
        traces.AddSubGroup(traceQuery);
        traceQuery.AddCommand<TraceQueryRunCommand>(serviceProvider);
        traceQuery.AddCommand<TraceQueryResumeCommand>(serviceProvider);
        traceQuery.AddCommand<TraceQueryCancelCommand>(serviceProvider);

        return monitor;
    }
}
