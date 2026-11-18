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
        Get (show/view) a single, specific Azure Monitor Health Model by name, within a resource group and subscription.
        Returns the model's metadata and if available, the health state (e.g. Healthy, Degraded, Unhealthy, Unknown) or null if it cannot be provided.
        """,
    OperationPlane = ToolOperationPlane.Control,
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
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(model), MonitorJsonContext.Default.HealthModelGetCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException { Status: 404, ErrorCode: "ResourceGroupNotFound" } =>
            "Resource group not found. Verify the resource group name and that you have access",
        RequestFailedException { Status: 404 } =>
            "Health model not found. Verify the health model name, resource group and that you have access",
        RequestFailedException { Status: 403 } reqEx =>
            $"Authorization failed reading the health model. Details: {reqEx.Message.Split('\n')[0].Trim()}",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record HealthModelGetCommandResult(HealthModelDetail HealthModel);
}
