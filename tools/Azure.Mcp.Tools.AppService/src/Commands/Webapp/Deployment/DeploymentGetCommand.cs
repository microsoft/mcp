// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;

[CommandMetadata(
    Id = "17c59409-5382-4419-aef4-0058ffe2c6ec",
    Name = "get",
    Title = "Gets Azure App Service Web App Deployment Details",
    Description = """
        Retrieves detailed information about Azure App Service web app deployments, including deployment name,
        if deployment is actively happening, when the deployment started and ended, who authored and deployed the
        deployment, and the type of deployment. If a specific deployment ID is not provided, the command will return
        details for all deployments in the web app. You can specify a deployment ID to get details for a specific
        deployment.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DeploymentGetCommand(ILogger<DeploymentGetCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DeploymentGetOptions, DeploymentGetCommand.DeploymentGetResult>(subscriptionResolver)
{
    private readonly ILogger<DeploymentGetCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DeploymentGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var deployments = await _appServiceService.GetDeploymentsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.App,
                options.DeploymentId,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(deployments), AppServiceJsonContext.Default.DeploymentGetResult);
        }
        catch (Exception ex)
        {
            if (options.DeploymentId == null)
            {
                _logger.LogError(ex, "Failed to list deployments for Web App '{App}' in resource group {ResourceGroup} and subscription {Subscription}",
                    options.App, options.ResourceGroup, options.Subscription);
            }
            else
            {
                _logger.LogError(ex, "Failed to get deployment '{DeploymentId}' for Web App '{App}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.DeploymentId, options.App, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DeploymentGetResult(List<DeploymentDetails> Deployments);
}
