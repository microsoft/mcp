// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;

[CommandMetadata(
    Id = "ec36210d-35ee-40a8-bb61-f4681f816201",
    Name = "list",
    Title = "List Scheduled Tasks",
    Description = "List SRE Agent scheduled tasks.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ScheduledTasksListCommand(ILogger<ScheduledTasksListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, ScheduledTasksListCommand.ScheduledTasksListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ScheduledTasksListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var tasks = await _sreAgentService.ListScheduledTasksAsync(endpoint, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(tasks), SreAgentJsonContext.Default.ScheduledTasksListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent scheduled tasks.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record ScheduledTasksListCommandResult(List<SreAgentScheduledTask> Tasks);
}
