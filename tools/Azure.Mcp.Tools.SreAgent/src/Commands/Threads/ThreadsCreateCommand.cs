// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(
    Id = "c81d2450-f262-40a4-a9b0-5b2e1c3d0e03",
    Name = "create",
    Title = "Create Thread",
    Description = "Create a new thread on an SRE Agent and start a conversation by sending the opening message. Returns the initial agent response.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ThreadsCreateCommand(ILogger<ThreadsCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ThreadsCreateOptions, SreAgentThreadOperationResult>(subscriptionResolver)
{
    private readonly ILogger<ThreadsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var thread = await _sreAgentService.CreateThreadAsync(endpoint, SreAgentCommandHelpers.CreateThreadRequest(options.Message, options.Agent), options.Tenant, cancellationToken);
            var threadId = thread?.Id;
            var messages = string.IsNullOrWhiteSpace(threadId) ? [] : await _sreAgentService.PollThreadForCompletionAsync(endpoint, threadId, options.Tenant, TimeSpan.FromMinutes(2), false, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(threadId, "created", messages), SreAgentJsonContext.Default.SreAgentThreadOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent thread.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
