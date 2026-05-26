// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(Id = "ff881ee4-ee6e-4fae-8114-c6c6f36745bb", Name = "memories-reindex", Title = "Reindex Memories", Description = "Trigger a knowledge base reindex.", Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class MemoriesReindexCommand(ILogger<MemoriesReindexCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MemoryRemoteOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<MemoriesReindexCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MemoryRemoteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(_sreAgentService, options, cancellationToken);
            await _sreAgentService.ReindexMemoriesAsync(endpoint, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, "Γ£à Knowledge base reindex triggered. This may take a few minutes to complete.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error reindexing memories"); HandleException(context, ex); }
        return context.Response;
    }
}
