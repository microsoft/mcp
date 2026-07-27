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
    Id = "5d6e7f80-1a2b-4c3d-9e8f-7a6b5c4d3e2f",
    Name = "create",
    Title = "Create or Update Common Prompt",
    Description = "Create or update a named common prompt on the SRE Agent.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CommonPromptsCreateCommand(ILogger<CommonPromptsCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CommonPromptsCreateOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<CommonPromptsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CommonPromptsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            await _sreAgentService.CreateOrUpdateCommonPromptAsync(
                endpoint,
                options.Name,
                options.Content,
                options.Tenant,
                cancellationToken);

            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Common prompt '{options.Name}' saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating common prompt");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
