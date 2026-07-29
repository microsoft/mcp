// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
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
public sealed class ScheduledTasksPauseCommand(ILogger<ScheduledTasksPauseCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ScheduledTasksPauseOptions, ScheduledTasksDeleteCommand.ScheduledTaskOperationResult>(subscriptionResolver)
{
    private readonly ILogger<ScheduledTasksPauseCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ScheduledTasksPauseOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.PauseScheduledTaskAsync(endpoint, options.TaskId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.TaskId, "paused"), SreAgentJsonContext.Default.ScheduledTaskOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
