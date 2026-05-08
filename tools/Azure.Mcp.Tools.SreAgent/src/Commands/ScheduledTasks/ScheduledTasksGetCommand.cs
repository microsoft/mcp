// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;

[CommandMetadata(Id = "7e984a24-f5b6-4631-8bfb-58f1d31e8502", Name = "get", Title = "Get Scheduled Task", Description = "Get an SRE Agent scheduled task.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class ScheduledTasksGetCommand(ILogger<ScheduledTasksGetCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ScheduledTasksGetOptions>
{
    private readonly ILogger<ScheduledTasksGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.TaskId);
    }

    protected override ScheduledTasksGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TaskId = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.TaskId.Name);
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
            var endpoint = await ResolveEndpointAsync(_sreAgentService, options, cancellationToken);
            var task = await _sreAgentService.GetScheduledTaskAsync(endpoint, options.TaskId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ScheduledTasksGetCommandResult(task), SreAgentJsonContext.Default.ScheduledTasksGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ScheduledTasksGetCommandResult(SreAgentScheduledTask? Task);
}
