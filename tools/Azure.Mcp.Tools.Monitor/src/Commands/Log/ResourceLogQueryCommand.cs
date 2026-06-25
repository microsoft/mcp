// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Log;

[CommandMetadata(
    Id = "02aaf533-0593-4e1d-bd87-f7c69d34c7ba",
    Name = "query",
    Title = "Query Logs for Azure Resource",
    Description = """
        Query diagnostic and activity logs for a SPECIFIC Azure resource in a Log Analytics workspace using Kusto Query Language (KQL). 
        Use this tool when the user mentions a specific resource name or Resource ID in their request (e.g., "show logs for resource 'app-monitor'"). 
        This tool filters logs to only show data from the specified resource.

        When to use: User asks for logs from a specific resource by name or ID.
        When NOT to use: User asks for general workspace-wide logs without mentioning a specific resource.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceLogQueryCommand(ILogger<ResourceLogQueryCommand> logger, IMonitorService monitorService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceLogQueryOptions, List<JsonNode>>(subscriptionResolver)
{
    private readonly ILogger<ResourceLogQueryCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceLogQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _monitorService.QueryResourceLogs(
                options.Subscription!,
                options.ResourceId,
                options.Query,
                options.Table,
                options.Hours,
                options.Limit,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(results, MonitorJsonContext.Default.ListJsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing log query resource command.");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
