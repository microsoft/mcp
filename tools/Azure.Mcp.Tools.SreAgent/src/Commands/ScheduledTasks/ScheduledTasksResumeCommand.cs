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

[CommandMetadata(Id = "ef6f210c-846f-4506-8543-a9969b00ed01", Name = "resume", Title = "Resume Scheduled Task", Description = "Resume an SRE Agent scheduled task.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class ScheduledTasksResumeCommand(ILogger<ScheduledTasksResumeCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ScheduledTasksResumeOptions>
{
    private readonly ILogger<ScheduledTasksResumeCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.TaskId);
    }

    protected override ScheduledTasksResumeOptions BindOptions(ParseResult parseResult)
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
            await _sreAgentService.ResumeScheduledTaskAsync(endpoint, options.TaskId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ScheduledTasksDeleteCommand.ScheduledTaskOperationResult(options.TaskId, "resumed"), SreAgentJsonContext.Default.ScheduledTaskOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
