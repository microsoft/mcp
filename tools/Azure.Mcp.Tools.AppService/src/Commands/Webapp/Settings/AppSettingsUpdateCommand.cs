// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;

public sealed class AppSettingsUpdateCommand(ILogger<AppSettingsUpdateCommand> logger)
    : BaseAppServiceCommand<AppSettingsUpdateCommandOptions>(resourceGroupRequired: true)
{
    private const string CommandTitle = "Updates Azure App Service Web App Application Settings";
    private readonly ILogger<AppSettingsUpdateCommand> _logger = logger;
    public override string Id => "08ca52a3-f766-4c62-9597-702f629efaf6";
    public override string Name => "update-appsettings";

    public override string Description =>
        """
        Updates the application setting for an App Service web app. Three types of updating are available:
        
        - Add: adds a new application setting with the specified name and value. If the application setting already exists, the operation will fail and return an error message.
        - Set: sets the value of an application setting. If the application setting does not exist, this is equivalent to add. If the application setting already exists, the value will be overwritten.
        - Delete: deletes an application setting with the specified name. If the application setting does not exist, nothing happens.

        For add and set update types, both the application setting name and value are required. For delete update type, only the application setting name is required.
        """;

    public override string Title => CommandTitle;

    private static readonly HashSet<string> ValidUpdateTypes = ["add", "set", "delete"];

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppServiceOptionDefinitions.AppServiceName);
        command.Options.Add(AppServiceOptionDefinitions.AppSettingName);
        command.Options.Add(AppServiceOptionDefinitions.AppSettingValue);
        command.Options.Add(AppServiceOptionDefinitions.AppSettingUpdateType);
        command.Validators.Add(commandResult =>
        {
            var updateType = commandResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppSettingUpdateType.Name);
            var settingValue = commandResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppSettingValue.Name);

            if (!ValidateUpdateType(updateType, out var errorMessage))
            {
                commandResult.AddError(errorMessage);
            }

            if (!ValidateSettingValue(updateType, settingValue, out errorMessage))
            {
                commandResult.AddError(errorMessage);
            }
        });
    }

    internal static bool ValidateUpdateType(string? settingUpdateType, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (!ValidUpdateTypes.Contains(settingUpdateType, StringComparer.OrdinalIgnoreCase))
        {
            errorMessage = $"'{AppServiceOptionDefinitions.AppSettingUpdateTypeName}' must be one of the following values: {string.Join(", ", ValidUpdateTypes)}.";
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

    protected override AppSettingsUpdateCommandOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AppName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppServiceName.Name);
        options.SettingName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppSettingName.Name);
        options.SettingValue = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppSettingValue.Name);
        options.SettingUpdateType = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppSettingUpdateType.Name);
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
            var updateResult = await appServiceService.UpdateAppSettingsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.SettingName!,
                options.SettingUpdateType!,
                options.SettingValue,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(updateResult), AppServiceJsonContext.Default.AppSettingsUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to '{SettingUpdateType}' application setting '{SettingName}' for Web App details for '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.SettingUpdateType, options.SettingName, options.AppName, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AppSettingsUpdateResult(string UpdateStatus);
}
