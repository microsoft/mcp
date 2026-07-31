// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;

[CommandMetadata(
    Id = "825ef21f-392f-4cd4-8272-7e7dce12e293",
    Name = "get-appsettings",
    Title = "Gets Azure App Service Web App Application Settings",
    Description = """
        Retrieves the application settings for an App Service web app, returning key-value pairs that represent the
        setting. Application settings may contain sensitive information.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true,
    LocalRequired = false)]
public sealed class AppSettingsGetCommand(ILogger<AppSettingsGetCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseAppServiceOptions, AppSettingsGetCommand.AppSettingsGetResult>(subscriptionResolver)
{
    private readonly ILogger<AppSettingsGetCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseAppServiceOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var appSettings = await _appServiceService.GetAppSettingsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.App,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(appSettings), AppServiceJsonContext.Default.AppSettingsGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get application settings for Web App details for '{App}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.App, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AppSettingsGetResult(IDictionary<string, string> AppSettings);
}
