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
    Id = "adf88fc6-d765-48c4-9c54-97713ad65306",
    Name = "test",
    Title = "Test SRE Agent Connector",
    Description = "Test a connector and list the tools it exposes.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsTestCommand(ILogger<ConnectorsTestCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConnectorsTestOptions, ConnectorsTestCommand.ConnectorsTestCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ConnectorsTestCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConnectorsTestOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var result = await _sreAgentService.TestConnectorAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), SreAgentJsonContext.Default.ConnectorsTestCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SRE Agent connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConnectorsTestCommandResult(ConnectorTestResult TestResult);
}

