// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
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
    Id = "069f5411-fae7-4446-a7fc-53d7dc4b3c03",
    Name = "create-kusto",
    Title = "Create SRE Agent Kusto Connector",
    Description = "Create or update a Kusto connector on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsCreateKustoCommand(ILogger<ConnectorsCreateKustoCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<ConnectorsCreateKustoOptions>
{
    private readonly ILogger<ConnectorsCreateKustoCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.ClusterUrl.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Database);
    }

    protected override ConnectorsCreateKustoOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name);
        options.ClusterUrl = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.ClusterUrl);
        options.Database = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Database);
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
            // SRE Agent requires the Kusto data source to be of the form
            // https://<cluster>.kusto.windows.net/<database>. Concatenate database into
            // the URL path when --database is supplied so server-side validation passes.
            var dataSource = string.IsNullOrWhiteSpace(options.Database)
                ? options.ClusterUrl
                : $"{options.ClusterUrl!.TrimEnd('/')}/{Uri.EscapeDataString(options.Database!)}";
            var connector = new AgentConnectorEnvelope
            {
                Name = options.Name,
                Properties = new AgentConnector
                {
                    Name = options.Name,
                    DataConnectorType = "Kusto",
                    DataSource = dataSource,
                    Identity = "system",
                    ExtendedProperties = null
                }
            };

            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(_sreAgentService, options, cancellationToken);
            var created = await _sreAgentService.CreateOrUpdateConnectorAsync(options.Subscription!, resourceGroup, options.Agent!, options.Name!, connector, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ConnectorsCreateKustoCommandResult(created), SreAgentJsonContext.Default.ConnectorsCreateKustoCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent Kusto connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConnectorsCreateKustoCommandResult(AgentConnector Connector);
}

