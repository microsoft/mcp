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

[CommandMetadata(
    Id = "6fe4a44c-b9c5-44b1-b985-12f9043b1051",
    Name = "memories_delete",
    Title = "Delete Memory",
    Description = "Delete a knowledge base document after explicit confirmation.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class MemoriesDeleteCommand(ILogger<MemoriesDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MemoriesDeleteOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<MemoriesDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MemoriesDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete memory '{options.Name}': destructive operation requires --confirm true.");
            }
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            await _sreAgentService.DeleteMemoryAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Document '{options.Name}' deleted from knowledge base.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting memory");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
