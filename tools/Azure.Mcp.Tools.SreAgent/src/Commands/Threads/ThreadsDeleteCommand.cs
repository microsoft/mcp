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
    Id = "7c86f73c-bd69-4bb9-908a-d4a02d9f6805",
    Name = "delete",
    Title = "Delete Thread",
    Description = "Delete an SRE Agent thread. Requires confirm=true.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ThreadsDeleteCommand(ILogger<ThreadsDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ThreadsDeleteOptions, SreAgentThreadOperationResult>(subscriptionResolver)
{
    private readonly ILogger<ThreadsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
                throw new InvalidOperationException("Deleting a thread requires --confirm true.");
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.DeleteThreadAsync(endpoint, options.ThreadId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.ThreadId, "deleted", []), SreAgentJsonContext.Default.SreAgentThreadOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent thread.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
