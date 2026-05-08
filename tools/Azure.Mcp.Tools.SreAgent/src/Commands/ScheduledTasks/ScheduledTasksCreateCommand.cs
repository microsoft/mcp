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

[CommandMetadata(Id = "9092d701-68c4-49ac-9096-dbd4d8aa4a03", Name = "create", Title = "Create Scheduled Task", Description = "Create an SRE Agent scheduled task.", Destructive = false, Idempotent = false, OpenWorld = true, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class ScheduledTasksCreateCommand(ILogger<ScheduledTasksCreateCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ScheduledTasksCreateOptions>
{
    private readonly ILogger<ScheduledTasksCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.CronExpression);
        command.Options.Add(SreAgentOptionDefinitions.Message);
        command.Options.Add(SreAgentOptionDefinitions.Description);
    }

    protected override ScheduledTasksCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name);
        options.CronExpression = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.CronExpression.Name);
        options.Message = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Message.Name);
        options.Description = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Description.Name);
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
            var request = new SreAgentScheduledTaskCreateRequest(options.Name!, options.Agent!, options.CronExpression!, options.Message!, options.Description ?? options.Name!);
            var task = await _sreAgentService.CreateScheduledTaskAsync(endpoint, request, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ScheduledTasksGetCommand.ScheduledTasksGetCommandResult(task), SreAgentJsonContext.Default.ScheduledTasksGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
