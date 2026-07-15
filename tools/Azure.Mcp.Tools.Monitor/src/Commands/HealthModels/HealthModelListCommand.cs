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
    Id = "9d9d2f3a-6f9d-4a3a-8d7f-2c0b6a1f5e4c",
    Name = "list",
    Title = "List Azure Monitor Health Models",
    Description = """
        List (find/show/enumerate) Azure Monitor Health Models (Microsoft.CloudHealth/healthmodels) in a subscription, or scoped to a specific resource group.
        Returns a summary of each health model, including its name, resource group, and location.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HealthModelListCommand(IMonitorHealthModelService healthModelService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HealthModelListOptions, List<HealthModelSummary>>(subscriptionResolver)
{
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

            context.Response.Results = ResponseResult.Create(models, MonitorJsonContext.Default.ListHealthModelSummary);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
