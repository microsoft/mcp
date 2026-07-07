// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Options;
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
        Updates the application setting for an App Service web app. Three types of updating are available:

        - Add: adds a new application setting with the specified name and value. If the application setting already exists, the operation will fail and return an error message.
        - Set: sets the value of an application setting. If the application setting does not exist, this is equivalent to add. If the application setting already exists, the value will be overwritten.
        - Delete: deletes an application setting with the specified name. If the application setting does not exist, nothing happens.

        For add and set update types, both the application setting name and value are required. For delete update type, only the application setting name is required.
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

    private static readonly HashSet<string> s_validUpdateTypes = ["add", "set", "delete"];

    public override void ValidateOptions(AppSettingsUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!ValidateUpdateType(options.SettingUpdateType, out var errorMessage))
        {
            validationResult.Errors.Add(errorMessage);
        }

        if (!ValidateSettingValue(options.SettingUpdateType, options.SettingValue, out errorMessage))
        {
            validationResult.Errors.Add(errorMessage);
        }
    }

    internal static bool ValidateUpdateType(string? settingUpdateType, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (!s_validUpdateTypes.Contains(settingUpdateType, StringComparer.OrdinalIgnoreCase))
        {
            errorMessage = $"'{AppServiceOptionDefinitions.AppSettingUpdateTypeName}' must be one of the following values: {string.Join(", ", s_validUpdateTypes)}.";
            return false;
        }
        return true;
    }

    internal static bool ValidateSettingValue(string? settingUpdateType, string? settingValue, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (("add".Equals(settingUpdateType, StringComparison.OrdinalIgnoreCase) || "set".Equals(settingUpdateType, StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrWhiteSpace(settingValue))
        {
            errorMessage = $"'{AppServiceOptionDefinitions.AppSettingValueName}' is required when '{AppServiceOptionDefinitions.AppSettingUpdateTypeName}' is 'add' or 'set'.";
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
                options.RetryPolicy,
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
