// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Agents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Agents;

[CommandMetadata(
    Id = "3e7e3a46-5f64-4cb6-90fa-98c845bc4f92",
    Name = "create",
    Title = "Create SRE Agent Tool",
    Description = "Creates or updates a custom tool on a targeted SRE Agent resource. Required: --subscription, --agent, --name, --tool-type.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsToolsCreateCommand(ILogger<AgentsToolsCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AgentsToolsCreateOptions, AgentsToolsCreateCommand.AgentsToolsCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsToolsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AgentsToolsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var properties = CreateProperties(options);
            var request = new SreAgentToolCreateRequest
            {
                Name = options.Name,
                Properties = properties
            };

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var tool = await _sreAgentService.CreateAgentToolAsync(endpoint, request, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(tool), SreAgentJsonContext.Default.AgentsToolsCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent tool {Name} on agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static SreAgentToolProperties CreateProperties(AgentsToolsCreateOptions options)
    {
        var properties = new SreAgentToolProperties
        {
            Type = options.ToolType,
            Description = options.Description
        };

        if (string.Equals(options.ToolType, "KustoTool", StringComparison.OrdinalIgnoreCase))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Connector, nameof(options.Connector));
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Database, nameof(options.Database));
            properties.Connector = options.Connector;
            properties.Database = options.Database;
            properties.Query = options.Query;
        }
        else if (string.Equals(options.ToolType, "LinkTool", StringComparison.OrdinalIgnoreCase))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(options.UrlTemplate, nameof(options.UrlTemplate));
            properties.Template = options.UrlTemplate;
        }

        if (!string.IsNullOrWhiteSpace(options.Parameters))
        {
            properties.Parameters = JsonSerializer.Deserialize(options.Parameters, SreAgentJsonContext.Default.ListSreAgentToolParameter);
        }

        return properties;
    }

    public sealed record AgentsToolsCreateCommandResult(SreAgentTool Tool);
}
