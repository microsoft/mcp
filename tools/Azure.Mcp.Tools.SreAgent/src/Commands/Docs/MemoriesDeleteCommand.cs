// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(Id = "6fe4a44c-b9c5-44b1-b985-12f9043b1051", Name = "memories-delete", Title = "Delete Memory", Description = "Delete a knowledge base document after explicit confirmation.", Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class MemoriesDeleteCommand(ILogger<MemoriesDeleteCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<MemoriesDeleteOptions>
{
    private readonly ILogger<MemoriesDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentPortedOptionDefinitions.Name);
        command.Options.Add(SreAgentPortedOptionDefinitions.Confirm);
    }

    protected override MemoriesDeleteOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Name = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.Name.Name);
        o.Confirm = parseResult.GetValueOrDefault<bool>(SreAgentPortedOptionDefinitions.Confirm.Name);
        return o;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            if (!o.Confirm)
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Error: Refusing to delete memory '{o.Name}': destructive operation requires 'confirm: true'. Ask the user to confirm, then retry with confirm=true.");
                return context.Response;
            }
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            await _sreAgentService.DeleteMemoryAsync(endpoint, o.Name!, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Document '{o.Name}' deleted from knowledge base.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error deleting memory"); HandleException(context, ex); }
        return context.Response;
    }
}
