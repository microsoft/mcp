// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;

[CommandMetadata(
    Id = "9092d701-68c4-49ac-9096-dbd4d8aa4a03",
    Name = "create",
    Title = "Create Scheduled Task",
    Description = "Create an SRE Agent scheduled task.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ScheduledTasksCreateCommand(ILogger<ScheduledTasksCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ScheduledTasksCreateOptions, ScheduledTasksGetCommand.ScheduledTasksGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ScheduledTasksCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ScheduledTasksCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var request = new SreAgentScheduledTaskCreateRequest(options.Name, options.Agent!, options.CronExpression!, options.Message!, options.Description ?? options.Name);
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
