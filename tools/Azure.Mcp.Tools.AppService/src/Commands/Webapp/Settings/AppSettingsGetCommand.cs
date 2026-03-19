// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;

public sealed class AppSettingsGetCommand(ILogger<AppSettingsGetCommand> logger, IAppServiceService appServiceService)
    : BaseAppServiceCommand<BaseAppServiceOptions>(resourceGroupRequired: true, appRequired: true)
{
    private const string CommandTitle = "Gets Azure App Service Web App Application Settings";
    private readonly ILogger<AppSettingsGetCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;
    public override string Id => "825ef21f-392f-4cd4-8272-7e7dce12e293";
    public override string Name => "get-appsettings";

    public override string Description =>
        """
        Retrieves the application settings for an App Service web app, returning key-value pairs that represent the
        setting. Application settings may contain sensitive information.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = true,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command) => base.RegisterOptions(command);

    protected override BaseAppServiceOptions BindOptions(ParseResult parseResult) => base.BindOptions(parseResult);

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

            var appSettings = await _appServiceService.GetAppSettingsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(appSettings), AppServiceJsonContext.Default.AppSettingsGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get application settings for Web App details for '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.AppName, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AppSettingsGetResult(IDictionary<string, string> AppSettings);
}
