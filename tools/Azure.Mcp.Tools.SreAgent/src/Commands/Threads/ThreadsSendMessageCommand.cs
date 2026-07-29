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
    Id = "35c72f68-e2b3-4e7b-bb89-4f1d4a6f2104",
    Name = "send_message",
    Title = "Send Thread Message",
    Description = "Send a message to an existing SRE Agent thread.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ThreadsSendMessageCommand(ILogger<ThreadsSendMessageCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ThreadsSendMessageOptions, SreAgentThreadOperationResult>(subscriptionResolver)
{
    private readonly ILogger<ThreadsSendMessageCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsSendMessageOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.SendThreadMessageAsync(endpoint, options.ThreadId, SreAgentCommandHelpers.CreateMessageRequest(options.Message), options.Tenant, cancellationToken);
            var messages = await _sreAgentService.PollThreadForCompletionAsync(endpoint, options.ThreadId, options.Tenant, TimeSpan.FromMinutes(2), false, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.ThreadId, "sent", messages), SreAgentJsonContext.Default.SreAgentThreadOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SRE Agent thread message.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
