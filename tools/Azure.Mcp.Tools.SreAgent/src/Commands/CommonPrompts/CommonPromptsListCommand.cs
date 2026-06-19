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
    Id = "f6e7c1b2-2a4d-4f7c-9b73-7c6f7e7d3aa1",
    Name = "list",
    Title = "List Common Prompts",
    Description = "List all common prompts available on an SRE Agent. Returns a collection of all registered prompt names and descriptions.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CommonPromptsListCommand(ILogger<CommonPromptsListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CommonPromptsListOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<CommonPromptsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CommonPromptsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var prompts = await _sreAgentService.ListCommonPromptsAsync(endpoint, options.Search, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(prompts, options.Search));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing common prompts");
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static string Format(List<CommonPromptEnvelope> prompts, string? search)
    {
        if (prompts.Count == 0)
            return string.IsNullOrWhiteSpace(search) ? "No common prompts found." : $"No common prompts matched search \"{search}\".";
        var lines = new List<string> { "# Common Prompts", string.Empty, $"{prompts.Count} prompt(s)", string.Empty };
        foreach (var p in prompts)
        {
            lines.Add($"- **{p.Name ?? "(unnamed)"}**");
            var preview = p.Properties?.Prompt;
            if (!string.IsNullOrWhiteSpace(preview))
            {
                var snippet = preview.Length > 120 ? preview[..120] + "..." : preview;
                lines.Add($"  {snippet.Replace("\n", " ")}");
            }
        }
        return string.Join('\n', lines);
    }
}
