// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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

        Required arguments: resource ID or resource name, table name, KQL query
        Optional: hours, limit
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceLogQueryCommand(ILogger<ResourceLogQueryCommand> logger, IMonitorService monitorService) : SubscriptionCommand<ResourceLogQueryOptions>()
{
    private readonly ILogger<ResourceLogQueryCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ResourceLogQueryOptionDefinitions.ResourceId);
        command.Options.Add(MonitorOptionDefinitions.TableName);
        command.Options.Add(MonitorOptionDefinitions.Query);
        command.Options.Add(MonitorOptionDefinitions.Hours);
        command.Options.Add(MonitorOptionDefinitions.Limit);
    }

    protected override ResourceLogQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceId = parseResult.GetValueOrDefault<string>(ResourceLogQueryOptionDefinitions.ResourceId.Name);
        options.TableName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.TableName.Name);
        options.Query = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Query.Name);
        options.Hours = parseResult.GetValueOrDefault<int>(MonitorOptionDefinitions.Hours.Name);
        options.Limit = parseResult.GetValueOrDefault<int>(MonitorOptionDefinitions.Limit.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var results = await _monitorService.QueryResourceLogs(
                options.Subscription!,
                options.ResourceId!,
                options.Query!,
                options.TableName!,
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
