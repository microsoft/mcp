// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;

[CommandMetadata(
    Id = "35dd524f-6888-40a6-b07f-283e7990d601",
    Name = "pause",
    Title = "Pause Scheduled Task",
    Description = "Pause an SRE Agent scheduled task.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ScheduledTasksPauseCommand(ILogger<ScheduledTasksPauseCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ScheduledTasksPauseOptions>
{
    private readonly ILogger<ScheduledTasksPauseCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.TaskId);
    }

    protected override ScheduledTasksPauseOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TaskId = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.TaskId);
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
            await _sreAgentService.PauseScheduledTaskAsync(endpoint, options.TaskId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ScheduledTasksDeleteCommand.ScheduledTaskOperationResult(options.TaskId, "paused"), SreAgentJsonContext.Default.ScheduledTaskOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
