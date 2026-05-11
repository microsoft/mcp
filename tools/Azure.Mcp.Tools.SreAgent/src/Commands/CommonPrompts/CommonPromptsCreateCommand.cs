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

[CommandMetadata(Id = "5d6e7f80-1a2b-4c3d-9e8f-7a6b5c4d3e2f", Name = "create", Title = "Create or Update Common Prompt", Description = "Create or update a named common prompt on the SRE Agent.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class CommonPromptsCreateCommand(ILogger<CommonPromptsCreateCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<CommonPromptsCreateOptions>
{
    private readonly ILogger<CommonPromptsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentPortedOptionDefinitions.Name);
        command.Options.Add(SreAgentPortedOptionDefinitions.Content);
    }

    protected override CommonPromptsCreateOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Name = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.Name.Name);
        o.Content = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.Content.Name);
        return o;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            await _sreAgentService.CreateOrUpdateCommonPromptAsync(endpoint, o.Name!, o.Content!, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Common prompt '{o.Name}' saved.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error creating common prompt"); HandleException(context, ex); }
        return context.Response;
    }
}
