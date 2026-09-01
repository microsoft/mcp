// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;

[CommandMetadata(
    Id = "08ca52a3-f766-4c62-9597-702f629efaf6",
    Name = "update-appsettings",
    Title = "Updates Azure App Service Web App Application Settings",
    Description = """
        Updates an application setting for an App Service web app. Updates that create or replace a setting require both the
        setting name and value. Removing a setting requires only its name.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AppSettingsUpdateCommand(ILogger<AppSettingsUpdateCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AppSettingsUpdateOptions, AppSettingsUpdateCommand.AppSettingsUpdateResult>(subscriptionResolver)
{
    private readonly ILogger<AppSettingsUpdateCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

    public override void ValidateOptions(AppSettingsUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!ValidateSettingValue(options.SettingUpdateType, options.SettingValue, out var errorMessage))
        {
            validationResult.Errors.Add(errorMessage);
        }
    }

    internal static bool ValidateSettingValue(AppSettingUpdateType settingUpdateType, string? settingValue, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (settingUpdateType is AppSettingUpdateType.Add or AppSettingUpdateType.Set
            && string.IsNullOrWhiteSpace(settingValue))
        {
            errorMessage = "--setting-value is required for the selected update operation.";
            return false;
        }
        return true;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AppSettingsUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var updateResult = await _appServiceService.UpdateAppSettingsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.App,
                options.SettingName,
                options.SettingUpdateType,
                options.SettingValue,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(updateResult), AppServiceJsonContext.Default.AppSettingsUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to '{SettingUpdateType}' application setting '{SettingName}' for Web App details for '{App}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.SettingUpdateType, options.SettingName, options.App, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AppSettingsUpdateResult(string UpdateStatus);
}
