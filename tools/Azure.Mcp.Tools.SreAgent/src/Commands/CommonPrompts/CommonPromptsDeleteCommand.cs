// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.CommonPrompts;

[CommandMetadata(
    Id = "1c2d3e4f-5a6b-4c7d-8e9f-0a1b2c3d4e5f",
    Name = "delete",
    Title = "Delete Common Prompt",
    Description = "Permanently remove and irreversibly delete a named common prompt from an SRE Agent. Erases the prompt definition after explicit user confirmation. This action cannot be undone.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CommonPromptsDeleteCommand(ILogger<CommonPromptsDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CommonPromptsDeleteOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<CommonPromptsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CommonPromptsDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete common prompt '{options.Name}': destructive operation requires --confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            await _sreAgentService.DeleteCommonPromptAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Common prompt '{options.Name}' deleted.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting common prompt");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
