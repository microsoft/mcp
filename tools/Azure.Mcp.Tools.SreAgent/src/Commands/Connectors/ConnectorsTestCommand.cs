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
public sealed class ConnectorsTestCommand(ILogger<ConnectorsTestCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<ConnectorsTestOptions>
{
    private readonly ILogger<ConnectorsTestCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name.AsRequired());
    }

    protected override ConnectorsTestOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name);
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
            var result = await _sreAgentService.TestConnectorAsync(endpoint, options.Name!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ConnectorsTestCommandResult(result), SreAgentJsonContext.Default.ConnectorsTestCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SRE Agent connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConnectorsTestCommandResult(ConnectorTestResult TestResult);
}

