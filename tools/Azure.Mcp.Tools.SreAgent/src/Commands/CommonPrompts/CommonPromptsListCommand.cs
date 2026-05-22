// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.CommonPrompts;

[CommandMetadata(Id = "f6e7c1b2-2a4d-4f7c-9b73-7c6f7e7d3aa1", Name = "list", Title = "List Common Prompts", Description = "List common prompts on the SRE Agent.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class CommonPromptsListCommand(ILogger<CommonPromptsListCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<CommonPromptsListOptions>
{
    private readonly ILogger<CommonPromptsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Search);
    }

    protected override CommonPromptsListOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Search = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Search);
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
            var prompts = await _sreAgentService.ListCommonPromptsAsync(endpoint, o.Search, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(prompts, o.Search));
        }
        catch (Exception ex) { _logger.LogError(ex, "Error listing common prompts"); HandleException(context, ex); }
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
