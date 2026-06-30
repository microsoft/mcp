// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Options.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels;

[CommandMetadata(
    Id = "1e5a6b2c-0d4e-4f8a-9b3c-7a2d8e1f6b0d",
    Name = "get",
    Title = "Get an Azure Monitor Health Model",
    Description = """
        Get (show/view) a single Azure Monitor Health Model (Microsoft.CloudHealth/healthmodels) by name. Requires
        --subscription, --resource-group, and --health-model. Returns the full health model resource as JSON
        (id, name, location, properties, identity, tags, systemData).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HealthModelGetCommand(ILogger<HealthModelGetCommand> logger, IMonitorHealthModelService healthModelService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HealthModelGetOptions, JsonNode>(subscriptionResolver)
{
    private readonly ILogger<HealthModelGetCommand> _logger = logger;
    private readonly IMonitorHealthModelService _healthModelService = healthModelService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HealthModelGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _healthModelService.GetHealthModel(
                options.Subscription!,
                options.ResourceGroup,
                options.HealthModel,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(model, MonitorJsonContext.Default.JsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An exception occurred getting health model: {HealthModel}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.HealthModel,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
