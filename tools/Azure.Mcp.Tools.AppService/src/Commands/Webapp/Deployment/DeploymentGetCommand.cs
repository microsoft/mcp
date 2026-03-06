// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;

public sealed class DeploymentGetCommand(ILogger<DeploymentGetCommand> logger)
    : BaseAppServiceCommand<DeploymentGetOptions>(resourceGroupRequired: true, appRequired: true)
{
    private const string CommandTitle = "Gets Azure App Service Web App Deployment Details";
    private readonly ILogger<DeploymentGetCommand> _logger = logger;
    public override string Id => "17c59409-5382-4419-aef4-0058ffe2c6ec";
    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure App Service web app deployments, including deployment name,
        if deployment is actively happening, when the deployment started and ended, who authored and deployed the
        deployment, and the type of deployment. If a specific deployment ID is not provided, the command will return
        details for all deployments in the web app. You can specify a deployment ID to get details for a specific
        deployment.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppServiceOptionDefinitions.DeploymentIdOption);
    }

    protected override DeploymentGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DeploymentId = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.DeploymentIdOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        // Validate first, then bind
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var appServiceService = context.GetService<IAppServiceService>();
            var deployments = await appServiceService.GetDeploymentsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
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
                _logger.LogError(ex, "Failed to list deployments for Web App '{AppName}' in resource group {ResourceGroup} and subscription {Subscription}",
                    options.AppName, options.ResourceGroup, options.Subscription);
            }
            else
            {
                _logger.LogError(ex, "Failed to get deployment '{DeploymentId}' for Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.DeploymentId, options.AppName, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DeploymentGetResult(List<DeploymentDetails> Deployments);
}
