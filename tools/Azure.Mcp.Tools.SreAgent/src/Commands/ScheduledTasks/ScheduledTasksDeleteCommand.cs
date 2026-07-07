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
    Id = "64680a1f-b076-460b-87fd-20fdc971a804",
    Name = "delete",
    Title = "Delete Scheduled Task",
    Description = "Delete an SRE Agent scheduled task. Requires confirm=true.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ScheduledTasksDeleteCommand(ILogger<ScheduledTasksDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ScheduledTasksDeleteOptions, ScheduledTasksDeleteCommand.ScheduledTaskOperationResult>(subscriptionResolver)
{
    private readonly ILogger<ScheduledTasksDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ScheduledTasksDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException("Deleting a scheduled task requires --confirm true.");
            }
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.DeleteScheduledTaskAsync(endpoint, options.TaskId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.TaskId, "deleted"), SreAgentJsonContext.Default.ScheduledTaskOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record ScheduledTaskOperationResult(string? TaskId, string Status);
}
