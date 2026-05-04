// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Log;

[CommandMetadata(
    Id = "3f513aea-b6fc-4ad0-8f7d-9fbaa1056ac6",
    Name = "query",
    Title = "Query Log Analytics Workspace",
    Description = """
        Query logs across an ENTIRE Log Analytics workspace using Kusto Query Language (KQL). 
        Use this tool when the user wants to query all resources in a workspace or doesn't specify a particular resource name/ID (e.g., "show all errors in workspace", "query workspace logs", "what happened in my workspace").
        This tool queries across all resources and tables in the workspace.

        When to use: User asks for workspace-wide logs, all resources, or doesn't mention a specific resource.
        When NOT to use: User mentions a specific resource name or Resource ID - use resource log query instead.

        Requires workspace and resource group.
        Optional: hours and limit.
        query accepts KQL syntax.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkspaceLogQueryCommand(ILogger<WorkspaceLogQueryCommand> logger, IMonitorService monitorService) : BaseWorkspaceMonitorCommand<WorkspaceLogQueryOptions>()
{
    private readonly ILogger<WorkspaceLogQueryCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MonitorOptionDefinitions.TableName);
        command.Options.Add(MonitorOptionDefinitions.Query);
        command.Options.Add(MonitorOptionDefinitions.Hours);
        command.Options.Add(MonitorOptionDefinitions.Limit);
    }

    protected override WorkspaceLogQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
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
            var results = await _monitorService.QueryWorkspaceLogs(
                options.Subscription!,
                options.Workspace!,
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
            _logger.LogError(ex, "Error executing log query command.");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
