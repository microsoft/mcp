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
    Id = "dd2962f2-1ba5-4cf6-a975-ed8b1ddab107",
    Name = "list",
    Title = "List SRE Agent Hooks",
    Description = "List hooks configured on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksListCommand(ILogger<HooksListCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<HooksListOptions>
{
    private readonly ILogger<HooksListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
    }

    protected override HooksListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
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
            var hooks = await _sreAgentService.ListHooksAsync(endpoint, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new HooksListCommandResult(hooks), SreAgentJsonContext.Default.HooksListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent hooks.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record HooksListCommandResult(List<HookEnvelope> Hooks);
}

