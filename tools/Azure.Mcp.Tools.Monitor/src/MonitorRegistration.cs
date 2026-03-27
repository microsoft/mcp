// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Monitor;

public sealed class MonitorRegistration : IAreaRegistration
{
    public static string AreaName => "monitor";

    public static string AreaTitle => "Azure Monitor";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Monitor operations - Commands for querying and analyzing Azure Monitor logs and metrics.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "activitylog",
                Description = "Azure Monitor activity log operations - Commands for querying and analyzing activity logs for Azure resources.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "ffc0ed72-0622-4a27-bfd8-6df9b83adce8",
                        Name = "list",
                        Description = "Always use this tool if user is asking for activity logs for a resource. Lists activity logs for the specified Azure resource over the given prior number of hours. This command retrieves activity logs to help understand resource deployment history, modification activities, and access patterns. Returns activity log events with details including timestamp, operation name, status, and caller information. should be called to help retrieve information about why a resource failed to deploy or may not be working.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-name",
                                Description = "The name of the Azure resource to retrieve activity logs for.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-type",
                                Description = "The type of the Azure resource (e.g., 'Microsoft.Storage/storageAccounts'). Only provide this if needed to disambiguate between multiple resources with the same name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "hours",
                                Description = "The number of hours prior to now to retrieve activity logs for.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "event-level",
                                Description = "The level of activity logs to retrieve. Valid levels are: Critical, Error, Informational, Verbose, Warning. If not provided, returns all levels.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "top",
                                Description = "The maximum number of activity logs to retrieve.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WorkspaceListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "healthmodels",
                Description = "Azure Monitor Health Models operations - Commands for working with Azure Monitor Health Models.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "entity",
                        Description = "Entity operations - Commands for working with entities in Azure Monitor Health Models.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "80b23546-a6ac-4f0c-ad70-f51d6dff5543",
                                Name = "get",
                                Description = "Retrieve the health status of an entity for a given Azure Monitor Health Model. Use this tool ONLY when the user mentions a specific health model name and asks for health status, health events. This provides application-level health monitoring with custom health models, not basic Azure resource availability. For basic Azure resource availability status, use Resource Health tool instead `azmcp_resourcehealth_availability-status_get`. For querying logs from a Log Analystics workspace, use `azmcp_monitor_workspace_log_query`. For querying logs of a specific Azure resource, use `azmcp_monitor_resource_log_query`. Required arguments: - --entity: The entity to get health for - --health-model: The health model name",
                                Title = "Get",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "entity",
                                        Description = "The entity to get health for.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "health-model",
                                        Description = "The name of the health model for which to get the health.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(WebTestsGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "instrumentation",
                Description = "Azure Monitor instrumentation operations - Commands for orchestrated onboarding and migration steps.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "2c9f3785-4b97-4dd6-8489-af515638f0d5",
                        Name = "get-learning-resource",
                        Description = "List all available learning resources for Azure Monitor instrumentation or get the content of a specific resource by path. Returns all resource paths by default, or retrieves the full content when a path is specified. Note: For instrumenting an application, use orchestrator-start instead.",
                        Title = "Get Azure Monitor Learning Resource",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "path",
                                Description = "Learning resource path.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(GetLearningResourceCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "dd7d9a59-fb6d-436a-9e08-8bbdf6d5f9d5",
                        Name = "orchestrator-next",
                        Description = "Get the next instrumentation action after completing the current one. Call this ONLY after you have executed the EXACT instruction from the previous response. DO NOT skip steps. DO NOT improvise. DO NOT add extra code or commands. Expected workflow: 1. You received an action from orchestrator-start or orchestrator-next 2. You executed EXACTLY what the 'instruction' field told you to do 3. Now call this tool to get the next action Returns: The next action to execute, or 'complete' status when all steps are done.",
                        Title = "Get Next Azure Monitor Instrumentation Action",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "session-id",
                                Description = "The workspace path returned as sessionId from orchestrator-start.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "completion-note",
                                Description = "One sentence describing what you executed, e.g., 'Ran dotnet add package command' or 'Added UseAzureMonitor() to Program.cs'",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(OrchestratorNextCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "35f577d9-6378-4d34-b822-111ff6e8957c",
                        Name = "orchestrator-start",
                        Description = "START HERE for Azure Monitor instrumentation. Analyzes workspace and returns the first action to execute. After executing the action, call orchestrator-next to continue. DO NOT improvise. Execute EXACTLY what the 'instruction' field tells you.",
                        Title = "Start Azure Monitor Instrumentation",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "workspace-path",
                                Description = "Absolute path to the workspace folder.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(OrchestratorStartCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "8f69c45b-7e4f-4ea7-9a7d-58fa7fc0897e",
                        Name = "send-brownfield-analysis",
                        Description = "Send brownfield code analysis findings after orchestrator-start returned status 'analysis_needed'. You must have scanned the workspace source files and filled in the analysis template. For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false, hasCustomSampling: false) rather than null. After this call succeeds, continue with orchestrator-next as usual.",
                        Title = "Send Brownfield Analysis",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "session-id",
                                Description = "The workspace path returned as sessionId from orchestrator-start.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "findings-json",
                                Description = "JSON object with brownfield analysis findings. Required properties: - serviceOptions: Service options findings from analyzing AddApplicationInsightsTelemetry() call. Null if not found. - initializers: Telemetry initializer findings from analyzing ITelemetryInitializer or IConfigureOptions<TelemetryConfiguration> implementations. Null if none found. - processors: Telemetry processor findings from analyzing ITelemetryProcessor implementations. Null if none found. - clientUsage: TelemetryClient usage findings from analyzing direct TelemetryClient usage. Null if not found. - sampling: Custom sampling configuration findings. Null if no custom sampling. - telemetryPipeline: Custom ITelemetryChannel or TelemetrySinks usage findings. Null if not found. - logging: Explicit logger provider and filter findings. Null if not found. For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false, hasCustomSampling: false) rather than null.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(SendBrownfieldAnalysisCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "8fd4eb5f-14d1-450f-982c-82d761f0f7d6",
                        Name = "send-enhancement-select",
                        Description = "Submit the user's enhancement selection after orchestrator-start returned status 'enhancement_available'. Present the enhancement options to the user first, then call this tool with their chosen option key(s). Multiple enhancements can be selected by passing a comma-separated list (e.g. 'redis,processors'). After this call succeeds, continue with orchestrator-next as usual.",
                        Title = "Send Enhancement Selection",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "session-id",
                                Description = "The workspace path returned as sessionId from orchestrator-start.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "enhancement-keys",
                                Description = "One or more enhancement keys, comma-separated (e.g. 'redis', 'redis,processors', 'entityframework,otlp').",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(SendEnhancementSelectCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "metrics",
                Description = "Azure Monitor metrics operations - Commands for querying and analyzing Azure Monitor metrics.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "d3bf37ed-5f2e-448d-a16e-73140ef908c2",
                        Name = "definitions",
                        Description = "List available metric definitions for an Azure resource. Returns metadata about the metrics available for the resource.",
                        Title = "Definitions",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-type",
                                Description = "The Azure resource type (e.g., 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines'). If not specified, will attempt to infer from resource name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource",
                                Description = "The name of the Azure resource to query metrics for.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "metric-namespace",
                                Description = "The metric namespace to query. Obtain this value from the azmcp-monitor-metrics-definitions command.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "search-string",
                                Description = "A string to filter the metric definitions by. Helpful for reducing the number of records returned. Performs case-insensitive matching on metric name and description fields.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "limit",
                                Description = "The maximum number of metric definitions to return. Defaults to 10.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(MetricsDefinitionsCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "6e86ef31-04e1-4cec-8bda-5292d4bc3ad8",
                        Name = "query",
                        Description = "Query Azure Monitor metrics for a resource. Returns time series data for the specified metrics.",
                        Title = "Query",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-type",
                                Description = "The Azure resource type (e.g., 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines'). If not specified, will attempt to infer from resource name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource",
                                Description = "The name of the Azure resource to query metrics for.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "metric-names",
                                Description = "The names of metrics to query (comma-separated).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "start-time",
                                Description = "The start time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to 24 hours ago.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "end-time",
                                Description = "The end time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to now.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "interval",
                                Description = "The time interval for data points (e.g., PT1H for 1 hour, PT5M for 5 minutes).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "aggregation",
                                Description = "The aggregation type to use (Average, Maximum, Minimum, Total, Count).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "filter",
                                Description = "OData filter to apply to the metrics query.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "metric-namespace",
                                Description = "The metric namespace to query. Obtain this value from the azmcp-monitor-metrics-definitions command.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "max-buckets",
                                Description = "The maximum number of time buckets to return. Defaults to 50.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WorkspaceLogQueryCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "resource",
                Description = "Azure Monitor resource operations - Commands for resource-centric operations.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "log",
                        Description = "Azure Monitor resource logs operations - Commands for querying resource logs using KQL.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "02aaf533-0593-4e1d-bd87-f7c69d34c7ba",
                                Name = "query",
                                Description = "Query diagnostic and activity logs for a SPECIFIC Azure resource in a Log Analytics workspace using Kusto Query Language (KQL). Use this tool when the user mentions a specific resource name or Resource ID in their request (e.g., \"show logs for resource 'app-monitor'\"). This tool filters logs to only show data from the specified resource. When to use: User asks for logs from a specific resource by name or ID. When NOT to use: User asks for general workspace-wide logs without mentioning a specific resource. Required arguments: resource ID or resource name, table name, KQL query Optional: hours, limit",
                                Title = "Query",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-id",
                                        Description = "The Azure Resource ID to query logs. Example: /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.OperationalInsights/workspaces/<ws>",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "table",
                                        Description = "The name of the table to query. This is the specific table within the workspace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "query",
                                        Description = "The KQL query to execute against the Log Analytics workspace. You can use predefined queries by name: - 'recent': Shows most recent logs ordered by TimeGenerated - 'errors': Shows error-level logs ordered by TimeGenerated Otherwise, provide a custom KQL query.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "hours",
                                        Description = "The number of hours to query back from now.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "limit",
                                        Description = "The maximum number of results to return.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(WorkspaceLogQueryCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "table",
                Description = "Log Analytics workspace table operations - Commands for listing tables in Log Analytics workspaces.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "2b1ae0be-d6dd-4db9-9c58-fc4fcb3bf8e6",
                        Name = "list",
                        Description = "List all tables in a Log Analytics workspace. Requires workspace. Returns table names and schemas that can be used for constructing KQL queries.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "workspace",
                                Description = "The Log Analytics workspace ID or name. This can be either the unique identifier (GUID) or the display name of your workspace.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "table-type",
                                Description = "The type of table to query. Options: 'CustomLog', 'AzureMetrics', etc.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WorkspaceListCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "type",
                        Description = "Log Analytics workspace table type operations - Commands for listing table types in Log Analytics workspaces.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "17928c13-3907-428c-8232-74f7aec1d76d",
                                Name = "list",
                                Description = "List available table types in a Log Analytics workspace. Returns table type names.",
                                Title = "List",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "workspace",
                                        Description = "The Log Analytics workspace ID or name. This can be either the unique identifier (GUID) or the display name of your workspace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(WorkspaceListCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "webtests",
                Description = "Azure Monitor Web Test operations - Commands for working with Azure Availability/Web Tests.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "aa5a22bc-6a04-4bc0-a963-b6e462b5cdc4",
                        Name = "createorupdate",
                        Description = "Create or update a standard web test in Azure Monitor to monitor endpoint availability. Use this to set up new web tests or modify existing ones with monitoring configurations like URL, frequency, locations, and expected responses. Automatically creates a new test if it doesn't exist, or updates an existing test with new settings.",
                        Title = "Createorupdate",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "webtest-resource",
                                Description = "The name of the Web Test resource to operate on.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "appinsights-component",
                                Description = "The resource id of the Application Insights component to associate with the web test.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The location where the web test resource is created. This should be the same as the AppInsights component location.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "webtest-locations",
                                Description = "List of locations to run the test from (comma-separated values). Location refers to the geo-location population tag specific to Availability Tests.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "request-url",
                                Description = "The absolute URL to test",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "webtest",
                                Description = "The name of the test in web test resource",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "description",
                                Description = "The description of the web test",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "enabled",
                                Description = "Whether the web test is enabled",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "expected-status-code",
                                Description = "Expected HTTP status code",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "follow-redirects",
                                Description = "Whether to follow redirects",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "frequency",
                                Description = "Test frequency in seconds. Supported values 300, 600, 900 seconds.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "headers",
                                Description = "HTTP headers to include in the request. Comma-separated KEY=VALUE",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "http-verb",
                                Description = "HTTP method (get, post, etc.)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "ignore-status-code",
                                Description = "Whether to ignore the status code validation",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "parse-requests",
                                Description = "Whether to parse dependent requests",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "request-body",
                                Description = "The body of the request",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "retry-enabled",
                                Description = "Whether retries are enabled",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "ssl-check",
                                Description = "Whether to check SSL certificates",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "ssl-lifetime-check",
                                Description = "Number of days to check SSL certificate lifetime",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "timeout",
                                Description = "Request timeout in seconds (max 2 minutes). Supported values: 30, 60, 90, 120 seconds",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WebTestsCreateOrUpdateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c9897ba5-445c-43dc-9902-e8454dbdc243",
                        Name = "get",
                        Description = "Gets details for a specific web test or lists all web tests. When --webtest-resource is provided, returns detailed information about a single web test. When --webtest-resource is omitted, returns a list of all web tests in the subscription (optionally filtered by resource group).",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "webtest-resource",
                                Description = "The name of the Web Test resource to operate on.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WebTestsGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "workspace",
                Description = "Log Analytics workspace operations - Commands for managing Log Analytics workspaces.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "0c76b74e-14bf-4e0c-ab10-4bbeeb53347b",
                        Name = "list",
                        Description = "List Log Analytics workspaces in a subscription. This command retrieves all Log Analytics workspaces available in the specified Azure subscription, displaying their names, IDs, and other key properties. Use this command to identify workspaces before querying their logs or tables.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options = [],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WorkspaceListCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "log",
                        Description = "Azure Monitor resource logs operations - Commands for querying resource logs using KQL.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "3f513aea-b6fc-4ad0-8f7d-9fbaa1056ac6",
                                Name = "query",
                                Description = "Query logs across an ENTIRE Log Analytics workspace using Kusto Query Language (KQL). Use this tool when the user wants to query all resources in a workspace or doesn't specify a particular resource name/ID (e.g., \"show all errors in workspace\", \"query workspace logs\", \"what happened in my workspace\"). This tool queries across all resources and tables in the workspace. When to use: User asks for workspace-wide logs, all resources, or doesn't mention a specific resource. When NOT to use: User mentions a specific resource name or Resource ID - use resource log query instead. Requires workspace and resource group. Optional: hours and limit. query accepts KQL syntax.",
                                Title = "Query",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "workspace",
                                        Description = "The Log Analytics workspace ID or name. This can be either the unique identifier (GUID) or the display name of your workspace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "table",
                                        Description = "The name of the table to query. This is the specific table within the workspace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "query",
                                        Description = "The KQL query to execute against the Log Analytics workspace. You can use predefined queries by name: - 'recent': Shows most recent logs ordered by TimeGenerated - 'errors': Shows error-level logs ordered by TimeGenerated Otherwise, provide a custom KQL query.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "hours",
                                        Description = "The number of hours to query back from now.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "limit",
                                        Description = "The maximum number of results to return.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(WorkspaceLogQueryCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(WorkspaceListCommand) => serviceProvider.GetRequiredService<WorkspaceListCommand>(),
            nameof(WebTestsGetCommand) => serviceProvider.GetRequiredService<WebTestsGetCommand>(),
            nameof(GetLearningResourceCommand) => serviceProvider.GetRequiredService<GetLearningResourceCommand>(),
            nameof(OrchestratorNextCommand) => serviceProvider.GetRequiredService<OrchestratorNextCommand>(),
            nameof(OrchestratorStartCommand) => serviceProvider.GetRequiredService<OrchestratorStartCommand>(),
            nameof(SendBrownfieldAnalysisCommand) => serviceProvider.GetRequiredService<SendBrownfieldAnalysisCommand>(),
            nameof(SendEnhancementSelectCommand) => serviceProvider.GetRequiredService<SendEnhancementSelectCommand>(),
            nameof(MetricsDefinitionsCommand) => serviceProvider.GetRequiredService<MetricsDefinitionsCommand>(),
            nameof(WorkspaceLogQueryCommand) => serviceProvider.GetRequiredService<WorkspaceLogQueryCommand>(),
            nameof(WebTestsCreateOrUpdateCommand) => serviceProvider.GetRequiredService<WebTestsCreateOrUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in monitor area.")
        };
}
