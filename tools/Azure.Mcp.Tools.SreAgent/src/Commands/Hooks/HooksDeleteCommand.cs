// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Hooks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.SreAgent.Commands.Hooks;

[CommandMetadata(
    Id = "290f14b5-720e-4036-82b1-9fd9f577e009",
    Name = "delete",
    Title = "Delete SRE Agent Hook",
    Description = "Delete a hook from an Azure SRE Agent resource. Requires confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksDeleteCommand(ILogger<HooksDeleteCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<HooksDeleteOptions>
{
    private readonly ILogger<HooksDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override HooksDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name) ?? string.Empty;
        options.Confirm = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Confirm);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete hook '{options.Name}': destructive operation requires confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(_sreAgentService, options, cancellationToken);
            await _sreAgentService.DeleteHookAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new HooksDeleteCommandResult(true, options.Name), SreAgentJsonContext.Default.HooksDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent hook.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record HooksDeleteCommandResult(bool Deleted, string Name);
}

