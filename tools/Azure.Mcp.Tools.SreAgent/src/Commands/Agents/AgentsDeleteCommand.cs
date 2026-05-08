// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Agents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.SreAgent.Commands.Agents;

[CommandMetadata(
    Id = "53dbfc5d-95f3-4b68-94d0-f7fc5bd390ba",
    Name = "delete",
    Title = "Delete SRE Sub-Agent",
    Description = "Deletes a sub-agent from a targeted SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsDeleteCommand(ILogger<AgentsDeleteCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<AgentsDeleteOptions>
{
    private readonly ILogger<AgentsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override AgentsDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Agent.Name);
        options.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name);
        options.Confirm = parseResult.GetValueOrDefault<bool>(SreAgentOptionDefinitions.Confirm.Name);
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
                throw new InvalidOperationException($"Refusing to delete sub-agent '{options.Name}': destructive operation requires --confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options.Subscription!,
                options.ResourceGroup,
                options.Agent!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = await _sreAgentService.DeleteSubAgentAsync(endpoint, options.Name!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), SreAgentJsonContext.Default.AgentsDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE sub-agent {Name} from agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsDeleteCommandResult(SreAgentDeleteResult Agent);
}
