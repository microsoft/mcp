// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

public abstract class ThreadsCommandBase<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : SreAgentDataPlaneCommand<TOptions>
    where TOptions : BaseSreAgentOptions, new()
{
    protected const string FollowUpPrompt = "Please proceed with the investigation using all available tools and information. Use your best judgment and provide your complete findings including root cause analysis and recommended next steps.";

    private static readonly string[] DirectionPatterns =
    [
        "do you want me to", "would you like me to", "shall i", "should i proceed", "should i continue",
        "do you prefer", "would you prefer", "what would you like me to", "how would you like me to",
        "do you want to proceed", "would you like to proceed"
    ];

    private static readonly string[] DataRequestPatterns =
    [
        "could you provide", "can you provide", "please provide", "could you share", "can you share", "please share",
        "could you clarify", "can you clarify", "please clarify", "please specify", "i need more information",
        "i need additional", "what is the", "what are the", "do you have", "do you know", "subscription id",
        "resource group", "tenant id", "cluster name", "connection string", "credentials", "access key"
    ];

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
    }

    protected static SreAgentThreadCreateRequest CreateThreadRequest(string message, string agentName)
    {
        var (userId, displayName) = GetUser();
        return new(new(message, userId, displayName, agentName));
    }

    protected static SreAgentThreadMessageRequest CreateMessageRequest(string message, string? agentName = null)
    {
        var (userId, displayName) = GetUser();
        return new(message, userId, displayName, agentName);
    }

    protected static async Task<List<SreAgentThreadMessage>> PollForCompletionAsync(
        ISreAgentService sreAgentService,
        string endpoint,
        string threadId,
        string? tenant,
        TimeSpan timeout,
        bool autoApprove,
        CancellationToken cancellationToken)
    {
        var endTime = DateTimeOffset.UtcNow + timeout;
        List<SreAgentThreadMessage> messages = [];
        while (DateTimeOffset.UtcNow < endTime)
        {
            cancellationToken.ThrowIfCancellationRequested();
            messages = await sreAgentService.GetThreadMessagesAsync(endpoint, threadId, tenant, cancellationToken);

            if (autoApprove)
            {
                await ApprovePendingApprovalsAsync(sreAgentService, endpoint, messages, tenant, cancellationToken);
            }
            else if (HasPendingInteractiveRequest(messages))
            {
                return messages;
            }

            var thread = await sreAgentService.GetThreadAsync(endpoint, threadId, tenant, cancellationToken);
            var last = thread?.LastMessage;
            var isAgentMessage = !string.Equals(last?.Author?.Role, "user", StringComparison.OrdinalIgnoreCase);
            if (isAgentMessage && last?.IsComplete == true)
            {
                return messages.Count > 0
                    ? messages
                    : await sreAgentService.GetThreadMessagesAsync(endpoint, threadId, tenant, cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }

        return messages;
    }

    protected static FollowUpAction ClassifyFollowUp(List<SreAgentThreadMessage> messages)
    {
        var agentMessages = messages
            .Where(m => !string.Equals(m.Author?.Role, "user", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (agentMessages.Count == 0)
        {
            return FollowUpAction.None;
        }

        var last = agentMessages[^1];
        if (string.Equals(last.MessageType, "UserQuestion", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(last.UserQuestion?.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return FollowUpAction.NeedsData;
        }

        if (string.Equals(last.MessageType, "Approval", StringComparison.OrdinalIgnoreCase) && IsPendingApproval(last.Approval?.Status))
        {
            return FollowUpAction.NeedsData;
        }

        var text = (last.Text ?? string.Empty).ToLowerInvariant().Trim();
        if (DataRequestPatterns.Any(text.Contains))
        {
            return FollowUpAction.NeedsData;
        }

        return DirectionPatterns.Any(text.Contains) ? FollowUpAction.Auto : FollowUpAction.None;
    }

    protected static string? LastAgentText(List<SreAgentThreadMessage> messages) => messages
        .Where(m => !string.Equals(m.Author?.Role, "user", StringComparison.OrdinalIgnoreCase))
        .LastOrDefault()?.Text;

    protected static async Task ApprovePendingApprovalsAsync(
        ISreAgentService sreAgentService,
        string endpoint,
        List<SreAgentThreadMessage> messages,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var (userId, _) = GetUser("mcp-yolo");
        var approvals = messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Approval?.Id) && IsPendingApproval(m.Approval.Status))
            .Select(m => m.Approval!.Id!)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var approvalId in approvals)
        {
            await sreAgentService.ApproveApprovalAsync(endpoint, approvalId, new(userId), tenant, cancellationToken);
        }
    }

    private static bool HasPendingInteractiveRequest(List<SreAgentThreadMessage> messages) =>
        messages.Any(m =>
            (string.Equals(m.MessageType, "UserQuestion", StringComparison.OrdinalIgnoreCase) && string.Equals(m.UserQuestion?.Status, "Pending", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(m.MessageType, "Approval", StringComparison.OrdinalIgnoreCase) && IsPendingApproval(m.Approval?.Status)));

    private static bool IsPendingApproval(string? status) =>
        string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "PendingAuthorization", StringComparison.OrdinalIgnoreCase);

    private static (string UserId, string DisplayName) GetUser(string fallback = "mcp-user")
    {
        var user = Environment.GetEnvironmentVariable("USER") ?? Environment.GetEnvironmentVariable("USERNAME") ?? fallback;
        return (user, user == fallback ? "MCP User" : user);
    }

    protected static async Task<SreAgentInvestigationResult> RunInvestigationAsync(
        ISreAgentService sreAgentService,
        ThreadsInvestigateOptions options,
        bool autoApprove,
        CancellationToken cancellationToken)
    {
        var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(sreAgentService, options, cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)));

        var thread = await sreAgentService.CreateThreadAsync(endpoint, CreateThreadRequest(options.Message!, options.Agent!), options.Tenant, timeout.Token);
        var threadId = thread?.Id;
        if (string.IsNullOrWhiteSpace(threadId))
        {
            return new(null, "failed", 0, true, "Thread created but no ID was returned.", []);
        }

        var messages = await PollForCompletionAsync(sreAgentService, endpoint, threadId, options.Tenant, TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)), autoApprove, timeout.Token);
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
                await ApprovePendingApprovalsAsync(sreAgentService, endpoint, messages, options.Tenant, timeout.Token);
            }

            await sreAgentService.SendThreadMessageAsync(endpoint, threadId, CreateMessageRequest(FollowUpPrompt), options.Tenant, timeout.Token);
            messages = await PollForCompletionAsync(sreAgentService, endpoint, threadId, options.Tenant, TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)), autoApprove, timeout.Token);
            followUps++;
        }

        var status = followUps >= Math.Max(0, options.MaxIterations) ? "max-iterations-reached" : "completed";
        return new(threadId, status, followUps, false, null, messages);
    }

    protected enum FollowUpAction
    {
        None,
        Auto,
        NeedsData
    }
}
