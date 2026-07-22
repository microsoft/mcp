// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Connectors;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Connectors;

[CommandMetadata(
    Id = "dfd7be5a-f0ef-43ea-97fa-4799167a7704",
    Name = "create_mcp",
    Title = "Create SRE Agent MCP Connector",
    Description = "Create or update an MCP connector on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class ConnectorsCreateMcpCommand(ILogger<ConnectorsCreateMcpCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConnectorsCreateMcpOptions, ConnectorsCreateMcpCommand.ConnectorsCreateMcpCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ConnectorsCreateMcpCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override void ValidateOptions(ConnectorsCreateMcpOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.Equals(options.Type, "stdio", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(options.Type, "http", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("The --type option must be 'stdio' or 'http'.");
        }

        if (string.Equals(options.Type, "stdio", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.Command))
        {
            validationResult.Errors.Add("The --command option is required for stdio MCP connectors.");
        }

        if (string.Equals(options.Type, "http", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.Endpoint))
        {
            validationResult.Errors.Add("The --endpoint option is required for http MCP connectors.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConnectorsCreateMcpOptions options, CancellationToken cancellationToken)
    {
        try
        {
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

            var dataSource = string.Equals(options.Type, "stdio", StringComparison.OrdinalIgnoreCase)
                ? options.Command
                : options.Endpoint;
            var connector = new AgentConnectorEnvelope
            {
                Name = options.Name,
                Properties = new AgentConnector
                {
                    Name = options.Name,
                    DataConnectorType = "Mcp",
                    DataSource = dataSource,
                    Identity = "system",
                    ExtendedProperties = extendedProperties
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

            context.Response.Results = ResponseResult.Create(new(created), SreAgentJsonContext.Default.ConnectorsCreateMcpCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent MCP connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConnectorsCreateMcpCommandResult(AgentConnector Connector);
}

