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
    Id = "ef6f210c-846f-4506-8543-a9969b00ed01",
    Name = "resume",
    Title = "Resume Scheduled Task",
    Description = "Resume an SRE Agent scheduled task.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ScheduledTasksResumeCommand(ILogger<ScheduledTasksResumeCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ScheduledTasksResumeOptions, ScheduledTasksDeleteCommand.ScheduledTaskOperationResult>(subscriptionResolver)
{
    private readonly ILogger<ScheduledTasksResumeCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ScheduledTasksResumeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.ResumeScheduledTaskAsync(endpoint, options.TaskId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.TaskId, "resumed"), SreAgentJsonContext.Default.ScheduledTaskOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
