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
    Id = "7e984a24-f5b6-4631-8bfb-58f1d31e8502",
    Name = "get",
    Title = "Get Scheduled Task",
    Description = "Get an SRE Agent scheduled task.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ScheduledTasksGetCommand(ILogger<ScheduledTasksGetCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ScheduledTasksGetOptions, ScheduledTasksGetCommand.ScheduledTasksGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ScheduledTasksGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ScheduledTasksGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var task = await _sreAgentService.GetScheduledTaskAsync(endpoint, options.TaskId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(task), SreAgentJsonContext.Default.ScheduledTasksGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent scheduled task.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record ScheduledTasksGetCommandResult(SreAgentScheduledTask? Task);
}
