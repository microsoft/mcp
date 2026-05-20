// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ContainerApps.Options.ContainerApp;
using Azure.Mcp.Tools.ContainerApps.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;

[CommandMetadata(
    Id = "d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f90",
    Name = "list",
    Title = "List Container Apps",
    Description = """
        List Azure Container Apps in a subscription. Optionally filter by resource group. Each container app result
        includes: name, location, resourceGroup, managedEnvironmentId, provisioningState. If no container apps are
        found the tool returns an empty list of results (consistent with other list commands).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerAppListCommand(
    ILogger<ContainerAppListCommand> logger,
    IContainerAppsService containerAppsService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ContainerAppListOptions, ContainerAppListCommand.ContainerAppListCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerAppListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var containerApps = await containerAppsService.ListContainerApps(
                options.Subscription!,
                options.ResourceGroup,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new ContainerAppListCommandResult(containerApps?.Results ?? [], containerApps?.AreResultsTruncated ?? false), ContainerAppsJsonContext.Default.ContainerAppListCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error listing container apps. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ContainerAppListCommandResult(List<Models.ContainerAppInfo> ContainerApps, bool AreResultsTruncated);
}
