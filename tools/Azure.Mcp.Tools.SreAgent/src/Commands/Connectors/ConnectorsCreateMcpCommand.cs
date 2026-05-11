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
    Id = "dfd7be5a-f0ef-43ea-97fa-4799167a7704",
    Name = "create-mcp",
    Title = "Create SRE Agent MCP Connector",
    Description = "Create or update an MCP connector on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class ConnectorsCreateMcpCommand(ILogger<ConnectorsCreateMcpCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<ConnectorsCreateMcpOptions>
{
    private readonly ILogger<ConnectorsCreateMcpCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Type.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Command);
        command.Options.Add(SreAgentOptionDefinitions.Args);
        command.Options.Add(SreAgentOptionDefinitions.EnvsJson);
        command.Options.Add(SreAgentOptionDefinitions.Endpoint);
        command.Options.Add(SreAgentOptionDefinitions.AuthType);
        command.Options.Add(SreAgentOptionDefinitions.BearerTokenEnv);
        command.Options.Add(SreAgentOptionDefinitions.HeadersJson);
    }

    protected override ConnectorsCreateMcpOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        options.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name);
        options.Type = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Type);
        options.Command = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Command);
        options.Args = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Args);
        options.EnvsJson = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.EnvsJson);
        options.Endpoint = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Endpoint);
        options.AuthType = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.AuthType);
        options.BearerTokenEnv = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.BearerTokenEnv);
        options.HeadersJson = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.HeadersJson);
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
            if (!string.Equals(options.Type, "stdio", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(options.Type, "http", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The --type option must be 'stdio' or 'http'.");
            }

            if (string.Equals(options.Type, "stdio", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.Command))
            {
                throw new ArgumentException("The --command option is required for stdio MCP connectors.");
            }

            if (string.Equals(options.Type, "http", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.Endpoint))
            {
                throw new ArgumentException("The --endpoint option is required for http MCP connectors.");
            }

            var extendedProperties = new Dictionary<string, object> { ["type"] = options.Type! };
            if (string.Equals(options.Type, "stdio", StringComparison.OrdinalIgnoreCase))
            {
                extendedProperties["command"] = options.Command!;
                if (options.Args is { Length: > 0 })
                {
                    extendedProperties["args"] = options.Args;
                }

                var envs = SreAgentCommandHelpers.ParseJsonStringMap(options.EnvsJson, SreAgentOptionDefinitions.EnvsJsonName);
                if (envs is not null)
                {
                    extendedProperties["envs"] = envs;
                }
            }
            else
            {
                extendedProperties["endpoint"] = options.Endpoint!;
                if (!string.IsNullOrWhiteSpace(options.AuthType))
                {
                    extendedProperties["authType"] = options.AuthType;
                }

                if (!string.IsNullOrWhiteSpace(options.BearerTokenEnv))
                {
                    var bearerToken = Environment.GetEnvironmentVariable(options.BearerTokenEnv)
                        ?? throw new ArgumentException($"Environment variable '{options.BearerTokenEnv}' was not found.");
                    extendedProperties["bearerToken"] = bearerToken;
                }

                var headers = SreAgentCommandHelpers.ParseJsonStringMap(options.HeadersJson, SreAgentOptionDefinitions.HeadersJsonName);
                if (headers is not null)
                {
                    extendedProperties["headers"] = headers;
                }
            }

            var connector = new AgentConnectorEnvelope
            {
                Name = options.Name,
                Properties = new AgentConnector
                {
                    Name = options.Name,
                    DataConnectorType = "Mcp",
                    DataSource = "placeholder",
                    Identity = string.Empty,
                    ExtendedProperties = extendedProperties
                }
            };

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(_sreAgentService, options, cancellationToken);
            var created = await _sreAgentService.CreateOrUpdateConnectorAsync(endpoint, options.Name!, connector, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ConnectorsCreateMcpCommandResult(created), SreAgentJsonContext.Default.ConnectorsCreateMcpCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent MCP connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConnectorsCreateMcpCommandResult(AgentConnector Connector);
}

