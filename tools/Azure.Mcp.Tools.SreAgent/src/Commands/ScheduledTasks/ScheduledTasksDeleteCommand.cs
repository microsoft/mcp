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

[CommandMetadata(Id = "64680a1f-b076-460b-87fd-20fdc971a804", Name = "delete", Title = "Delete Scheduled Task", Description = "Delete an SRE Agent scheduled task. Requires confirm=true.", Destructive = true, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class ScheduledTasksDeleteCommand(ILogger<ScheduledTasksDeleteCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ScheduledTasksDeleteOptions>
{
    private readonly ILogger<ScheduledTasksDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.TaskId);
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override ScheduledTasksDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TaskId = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.TaskId);
        options.Confirm = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Confirm);
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
            if (!options.Confirm)
                throw new InvalidOperationException("Deleting a scheduled task requires --confirm true.");
            var endpoint = await ResolveEndpointAsync(_sreAgentService, options, cancellationToken);
            await _sreAgentService.DeleteScheduledTaskAsync(endpoint, options.TaskId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ScheduledTaskOperationResult(options.TaskId, "deleted"), SreAgentJsonContext.Default.ScheduledTaskOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ScheduledTaskOperationResult(string? TaskId, string Status);
}
