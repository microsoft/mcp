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

[CommandMetadata(Id = "1c2d3e4f-5a6b-4c7d-8e9f-0a1b2c3d4e5f", Name = "delete", Title = "Delete Common Prompt", Description = "Delete a common prompt after explicit confirmation.", Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class CommonPromptsDeleteCommand(ILogger<CommonPromptsDeleteCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<CommonPromptsDeleteOptions>
{
    private readonly ILogger<CommonPromptsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override CommonPromptsDeleteOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name) ?? string.Empty;
        o.Confirm = parseResult.GetValueOrDefault<bool>(SreAgentOptionDefinitions.Confirm.Name);
        return o;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            if (!o.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete common prompt '{o.Name}': destructive operation requires --confirm true.");
            }
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            await _sreAgentService.DeleteCommonPromptAsync(endpoint, o.Name, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Common prompt '{o.Name}' deleted.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error deleting common prompt"); HandleException(context, ex); }
        return context.Response;
    }
}
