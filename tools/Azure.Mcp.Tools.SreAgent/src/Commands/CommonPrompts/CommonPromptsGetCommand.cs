// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
public sealed class CommonPromptsGetCommand(ILogger<CommonPromptsGetCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<CommonPromptsGetOptions>
{
    private readonly ILogger<CommonPromptsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Name);
    }

    protected override CommonPromptsGetOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name) ?? string.Empty;
        return o;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            var prompt = await _sreAgentService.GetCommonPromptAsync(endpoint, o.Name, o.Tenant, cancellationToken);
            if (prompt is null)
            { SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Common prompt '{o.Name}' not found."); return context.Response; }
            var body = prompt.Properties?.Prompt ?? string.Empty;
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"# {prompt.Name ?? o.Name}\n\n{body}");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting common prompt"); HandleException(context, ex); }
        return context.Response;
    }
}
