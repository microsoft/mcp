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
    Id = "8b1d2f3c-4a5b-4c6d-8e7f-9a0b1c2d3e4f",
    Name = "get",
    Title = "Get Common Prompt",
    Description = "Show the content of a specific named common prompt on an SRE Agent. Returns the full prompt text for a single prompt identified by name.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CommonPromptsGetCommand(ILogger<CommonPromptsGetCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CommonPromptsGetOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<CommonPromptsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CommonPromptsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var prompt = await _sreAgentService.GetCommonPromptAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            if (prompt is null)
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Common prompt '{options.Name}' not found.");
                return context.Response;
            }
            var body = prompt.Properties?.Prompt ?? string.Empty;
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"# {prompt.Name ?? options.Name}\n\n{body}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting common prompt");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
