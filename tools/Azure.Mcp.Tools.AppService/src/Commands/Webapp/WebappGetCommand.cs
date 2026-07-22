// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp;

[CommandMetadata(
    Id = "4412f1af-16e7-46db-8305-33e3d7ae06de",
    Name = "get",
    Title = "Gets Azure App Service Web App Details",
    Description = """
        Retrieves detailed information about Azure App Service web apps, including app name, resource group, location,
        state, hostnames, etc. If a specific app name is not provided, the command will return details for all web apps
        in a subscription or resource group in a subscription. You can specify the app name, resource group name, and
        subscription to get details for a specific web app.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WebappGetCommand(ILogger<WebappGetCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WebappGetOptions, WebappGetCommand.WebappGetResult>(subscriptionResolver)
{
    private readonly ILogger<WebappGetCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

    public override void ValidateOptions(WebappGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrWhiteSpace(options.App) && string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            validationResult.Errors.Add($"When specifying '{AppServiceOptionDefinitions.AppName}', you must also specify '{OptionDefinitions.Common.ResourceGroupName}'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WebappGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var webapps = await _appServiceService.GetWebAppsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.App,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(webapps), AppServiceJsonContext.Default.WebappGetResult);
        }
        catch (Exception ex)
        {
            if (options.App == null)
            {
                if (options.ResourceGroup == null)
                {
                    _logger.LogError(ex, "Failed to list Web Apps in subscription {Subscription}", options.Subscription);
                }
                else
                {
                    _logger.LogError(ex, "Failed to list Web Apps in resource group {ResourceGroup} and subscription {Subscription}",
                        options.ResourceGroup, options.Subscription);
                }
            }
            else
            {
                _logger.LogError(ex, "Failed to get Web App details for '{App}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.App, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record WebappGetResult(List<WebappDetails> Webapps);
}
