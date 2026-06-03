// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
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
    Id = "d6b89855-d6ec-4211-809e-5909e459c208",
    Name = "get",
    Title = "Get SRE Agent Hook",
    Description = "Get details for a hook configured on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksGetCommand(ILogger<HooksGetCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<HooksGetOptions>
{
    private readonly ILogger<HooksGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name.AsRequired());
    }

    protected override HooksGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name) ?? string.Empty;
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
            var hook = await _sreAgentService.GetHookAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new HooksGetCommandResult(hook), SreAgentJsonContext.Default.HooksGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent hook.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record HooksGetCommandResult(HookEnvelope Hook);
}

