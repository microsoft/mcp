// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Connectors;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Connectors;

[CommandMetadata(
    Id = "069f5411-fae7-4446-a7fc-53d7dc4b3c03",
    Name = "create_kusto",
    Title = "Create SRE Agent Kusto Connector",
    Description = "Create or update a Kusto connector on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsCreateKustoCommand(ILogger<ConnectorsCreateKustoCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConnectorsCreateKustoOptions, ConnectorsCreateKustoCommand.ConnectorsCreateKustoCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ConnectorsCreateKustoCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConnectorsCreateKustoOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // SRE Agent requires the Kusto data source to be of the form
            // https://<cluster>.kusto.windows.net/<database>. Concatenate database into
            // the URL path when --database is supplied so server-side validation passes.
            var dataSource = string.IsNullOrWhiteSpace(options.Database)
                ? options.ClusterUrl
                : $"{options.ClusterUrl.TrimEnd('/')}/{Uri.EscapeDataString(options.Database!)}";
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

            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var created = await _sreAgentService.CreateOrUpdateConnectorAsync(
                options.Subscription!,
                resourceGroup,
                options.Agent,
                options.Name,
                connector,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(created), SreAgentJsonContext.Default.ConnectorsCreateKustoCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent Kusto connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConnectorsCreateKustoCommandResult(AgentConnector Connector);
}

