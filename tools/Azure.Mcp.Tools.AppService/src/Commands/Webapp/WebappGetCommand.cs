// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp;

public sealed class WebappGetCommand(ILogger<WebappGetCommand> logger) : BaseAppServiceCommand<BaseAppServiceOptions>()
{
    private const string CommandTitle = "Gets Azure App Service Web App Details";
    private readonly ILogger<WebappGetCommand> _logger = logger;
    public override string Id => "4412f1af-16e7-46db-8305-33e3d7ae06de";
    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure App Service web apps, including app name, resource group, location,
        state, hostnames, etc. If a specific app name is not provided, the command will return details for all web apps
        in a subscription or resource group in a subscription. You can specify the app name, resource group name, and
        subscription to get details for a specific web app.
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
        command.Options.Add(AppServiceOptionDefinitions.AppServiceName.AsOptional());
        command.Validators.Add(commandResult =>
        {
            var appName = commandResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppServiceName.Name);
            var resourceGroup = commandResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
            if (!string.IsNullOrWhiteSpace(appName) && string.IsNullOrWhiteSpace(resourceGroup))
            {
                commandResult.AddError($"When specifying '{AppServiceOptionDefinitions.AppServiceName.Name}', you must also specify '{OptionDefinitions.Common.ResourceGroup.Name}'.");
            }
        });
    }

    protected override BaseAppServiceOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AppName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppServiceName.Name);
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
            var webapps = await appServiceService.GetWebAppsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.AppName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(webapps), AppServiceJsonContext.Default.WebappGetResult);
        }
        catch (Exception ex)
        {
            if (options.AppName == null)
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
                _logger.LogError(ex, "Failed to get Web App details for '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.AppName, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WebappGetResult(List<WebappDetails> Webapps);
}
