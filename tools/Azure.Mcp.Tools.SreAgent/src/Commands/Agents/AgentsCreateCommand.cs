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
    Id = "7b8619c2-11e2-4fa6-bff1-a925ad7ca4bb",
    Name = "create",
    Title = "Create SRE Sub-Agent",
    Description = "Creates or updates a sub-agent on a targeted SRE Agent resource. Required: --subscription, --agent, --name.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsCreateCommand(ILogger<AgentsCreateCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<AgentsCreateOptions>
{
    private readonly ILogger<AgentsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Description);
        command.Options.Add(SreAgentOptionDefinitions.Instructions);
        command.Options.Add(SreAgentOptionDefinitions.Tools);
        command.Options.Add(SreAgentOptionDefinitions.Handoffs);
    }

    protected override AgentsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Agent.Name);
        options.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name);
        options.Description = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Description.Name);
        options.Instructions = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Instructions.Name);
        options.Tools = parseResult.GetValueOrDefault<string[]>(SreAgentOptionDefinitions.Tools.Name);
        options.Handoffs = parseResult.GetValueOrDefault<string[]>(SreAgentOptionDefinitions.Handoffs.Name);
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
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options.Subscription!,
                options.ResourceGroup,
                options.Agent!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var request = new SreSubAgentCreateRequest
            {
                Name = options.Name!,
                Properties = new SreSubAgentProperties
                {
                    Description = options.Description,
                    Instructions = options.Instructions,
                    Tools = ToList(options.Tools),
                    Handoffs = ToList(options.Handoffs)
                }
            };

            var agent = await _sreAgentService.CreateSubAgentAsync(endpoint, request, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(agent), SreAgentJsonContext.Default.AgentsCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE sub-agent {Name} on agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static List<string>? ToList(string[]? values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();

    internal record AgentsCreateCommandResult(SreSubAgent Agent);
}
