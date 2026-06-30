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
    Id = "9d9d2f3a-6f9d-4a3a-8d7f-2c0b6a1f5e4c",
    Name = "list",
    Title = "List Azure Monitor Health Models",
    Description = """
        List Azure Monitor Health Models (Microsoft.CloudHealth/healthmodels) in a subscription. Optionally scope the
        results to a single resource group with --resource-group. Returns the health model resources as a JSON array
        (id, name, location, properties, identity, tags). Use 'azmcp monitor healthmodels get' to retrieve a single model.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HealthModelListCommand(ILogger<HealthModelListCommand> logger, IMonitorHealthModelService healthModelService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HealthModelListOptions, List<JsonNode>>(subscriptionResolver)
{
    private readonly ILogger<HealthModelListCommand> _logger = logger;
    private readonly IMonitorHealthModelService _healthModelService = healthModelService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HealthModelListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var models = await _healthModelService.ListHealthModels(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(models, MonitorJsonContext.Default.ListJsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An exception occurred listing health models. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
