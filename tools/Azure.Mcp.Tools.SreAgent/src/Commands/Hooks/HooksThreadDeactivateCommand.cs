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
    Id = "f5e93db8-f78a-4409-b625-176611ee0b0c",
    Name = "deactivate-thread-hook",
    Title = "Deactivate SRE Agent Thread Hook",
    Description = "Deactivate an on-demand hook for a thread on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksThreadDeactivateCommand(ILogger<HooksThreadDeactivateCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<HooksThreadDeactivateOptions>
{
    private readonly ILogger<HooksThreadDeactivateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.ThreadId.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.HookName.AsRequired());
    }

    protected override HooksThreadDeactivateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.ThreadId = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.ThreadId);
        options.HookName = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.HookName);
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
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(_sreAgentService, options, cancellationToken);
            await _sreAgentService.DeactivateThreadHookAsync(endpoint, options.ThreadId!, options.HookName!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new HooksThreadDeactivateCommandResult(true, options.ThreadId!, options.HookName!), SreAgentJsonContext.Default.HooksThreadDeactivateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating SRE Agent thread hook.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record HooksThreadDeactivateCommandResult(bool Deactivated, string ThreadId, string HookName);
}

