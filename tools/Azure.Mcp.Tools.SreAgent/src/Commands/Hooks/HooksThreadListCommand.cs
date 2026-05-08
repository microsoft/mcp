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
    Id = "b2413e2f-e121-4d63-860c-28eebf4fd00a",
    Name = "list-thread-hooks",
    Title = "List SRE Agent Thread Hooks",
    Description = "List hook activation state for a thread on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksThreadListCommand(ILogger<HooksThreadListCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<HooksThreadListOptions>
{
    private readonly ILogger<HooksThreadListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.ThreadId.AsRequired());
    }

    protected override HooksThreadListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.ThreadId = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.ThreadId);
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
            var hooks = await _sreAgentService.ListThreadHooksAsync(endpoint, options.ThreadId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new HooksThreadListCommandResult(hooks), SreAgentJsonContext.Default.HooksThreadListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent thread hooks.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record HooksThreadListCommandResult(ThreadHooksResponse ThreadHooks);
}

