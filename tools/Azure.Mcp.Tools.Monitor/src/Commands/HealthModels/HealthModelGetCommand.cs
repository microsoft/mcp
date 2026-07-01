// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Options.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels;

[CommandMetadata(
    Id = "1e5a6b2c-0d4e-4f8a-9b3c-7a2d8e1f6b0d",
    Name = "get",
    Title = "Get an Azure Monitor Health Model",
    Description = """
        Get (show/view) a single Azure Monitor Health Model by name, resource group, and subscription.
        Returns the model's metadata and if available, the health state (e.g. Healthy, Degraded, Unhealthy, Unknown) or null if it cannot be provided.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HealthModelGetCommand(IMonitorHealthModelService healthModelService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HealthModelGetOptions, HealthModelGetCommand.HealthModelGetCommandResult>(subscriptionResolver)
{
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

            context.Response.Results = ResponseResult.Create(new(model), MonitorJsonContext.Default.HealthModelGetCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record HealthModelGetCommandResult(HealthModelDetail HealthModel);
}
