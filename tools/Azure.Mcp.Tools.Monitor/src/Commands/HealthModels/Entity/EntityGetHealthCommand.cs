// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;

[CommandMetadata(
    Id = "80b23546-a6ac-4f0c-ad70-f51d6dff5543",
    Name = "get",
    Title = "Get the health of an entity in a health model",
    Description = """
        Retrieve the health status of an entity for a given Azure Monitor Health Model. Use this tool ONLY when the user mentions a specific health model name and asks for health status, health events. This provides application-level health monitoring with custom health models, not basic Azure resource availability.
        For basic Azure resource availability status, use Resource Health tool instead `azmcp_resourcehealth_availability-status_get`.  
        For querying logs from a Log Analystics workspace, use `azmcp_monitor_workspace_log_query`.  
        For querying logs of a specific Azure resource, use `azmcp_monitor_resource_log_query`. 
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class EntityGetHealthCommand(ILogger<EntityGetHealthCommand> logger, IMonitorHealthModelService healthModelService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseMonitorHealthModelsOptions, JsonNode>(subscriptionResolver)
{
    private readonly ILogger<EntityGetHealthCommand> _logger = logger;
    private readonly IMonitorHealthModelService _healthModelService = healthModelService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseMonitorHealthModelsOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _healthModelService.GetEntityHealth(
                options.Entity,
                options.HealthModel,
                options.ResourceGroup,
                options.Subscription!,
                options.AuthMethod,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, MonitorJsonContext.Default.JsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An exception occurred getting health for entity: {Entity} in healthModel: {HealthModelName}, resourceGroup: {ResourceGroup}, subscription: {Subscription}, authMethod: {AuthMethod}"
                + ", tenant: {Tenant}.",
                options.Entity,
                options.HealthModel,
                options.ResourceGroup,
                options.Subscription,
                options.AuthMethod,
                options.Tenant);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => $"Entity or health model not found. Please check the entity ID, health model name, and resource group.",
        ArgumentException argEx => $"Invalid argument: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };
}
