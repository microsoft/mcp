// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(Id = "ab73d6fa-d53e-446c-9d4c-9d8cf41a3106", Name = "investigate-with-agent", Title = "Investigate With Agent", Description = "Start an investigation thread and automatically answer direction follow-ups.", Destructive = false, Idempotent = false, OpenWorld = true, ReadOnly = false, Secret = false, LocalRequired = false)]
public class ThreadsInvestigateCommand(ILogger<ThreadsInvestigateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : ThreadsCommandBase<ThreadsInvestigateOptions, SreAgentInvestigationResult>(subscriptionResolver)
{
    private readonly ILogger<ThreadsInvestigateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsInvestigateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await RunInvestigationAsync(options, autoApprove: false, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, SreAgentJsonContext.Default.SreAgentInvestigationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running SRE Agent investigation.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal async Task<SreAgentInvestigationResult> RunInvestigationAsync(ThreadsInvestigateOptions options, bool autoApprove, CancellationToken cancellationToken)
    {
        var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(_sreAgentService, options, cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)));

        var thread = await _sreAgentService.CreateThreadAsync(endpoint, CreateThreadRequest(options.Message!, options.Agent!), options.Tenant, timeout.Token);
        var threadId = thread?.Id;
        if (string.IsNullOrWhiteSpace(threadId))
        {
            return new(null, "failed", 0, true, "Thread created but no ID was returned.", []);
        }

        var messages = await PollForCompletionAsync(_sreAgentService, endpoint, threadId, options.Tenant, TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)), autoApprove, timeout.Token);
        var followUps = 0;
        while (followUps < Math.Max(0, options.MaxIterations))
        {
            var action = ClassifyFollowUp(messages);
            if (action == FollowUpAction.None)
            {
                break;
            }

            if (action == FollowUpAction.NeedsData && !autoApprove)
            {
                return new(threadId, "needs-data", followUps, true, LastAgentText(messages), messages);
            }

            if (action == FollowUpAction.NeedsData && autoApprove)
            {
                await ApprovePendingApprovalsAsync(_sreAgentService, endpoint, messages, options.Tenant, timeout.Token);
            }

            await _sreAgentService.SendThreadMessageAsync(endpoint, threadId, CreateMessageRequest(FollowUpPrompt), options.Tenant, timeout.Token);
            messages = await PollForCompletionAsync(_sreAgentService, endpoint, threadId, options.Tenant, TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)), autoApprove, timeout.Token);
            followUps++;
        }

        var status = followUps >= Math.Max(0, options.MaxIterations) ? "max-iterations-reached" : "completed";
        return new(threadId, status, followUps, false, null, messages);
    }
}
