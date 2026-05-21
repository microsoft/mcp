// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Connectors;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.SreAgent.Commands.Connectors;

[CommandMetadata(
    Id = "50f58038-1258-48cc-a7d2-bc6c29614405",
    Name = "delete",
    Title = "Delete SRE Agent Connector",
    Description = "Delete a connector from an Azure SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsDeleteCommand(ILogger<ConnectorsDeleteCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<ConnectorsDeleteOptions>
{
    private readonly ILogger<ConnectorsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override ConnectorsDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name) ?? string.Empty;
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
                throw new InvalidOperationException($"Refusing to delete connector '{options.Name}': destructive operation requires --confirm true.");
            }

            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(_sreAgentService, options, cancellationToken);
            await _sreAgentService.DeleteConnectorAsync(options.Subscription!, resourceGroup, options.Agent!, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ConnectorsDeleteCommandResult(true, options.Name), SreAgentJsonContext.Default.ConnectorsDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConnectorsDeleteCommandResult(bool Deleted, string Name);
}

