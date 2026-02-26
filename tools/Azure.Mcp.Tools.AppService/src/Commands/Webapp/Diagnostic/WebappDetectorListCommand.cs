// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

public sealed class WebappDetectorListCommand(ILogger<WebappDetectorListCommand> logger)
    : SubscriptionCommand<BaseAppServiceOptions>()
{
    private const string CommandTitle = "List the Diagnostic Detectors for an App Service Web App";
    private readonly ILogger<WebappDetectorListCommand> _logger = logger;
    public override string Id => "a8aa0966-4c0c-4e22-8854-cced583f0fb2";
    public override string Name => "list";

    public override string Description =>
        """
        Retrieves detailed information about detectors detector for the specified App Service Web App, returning the name,
        kind, type, description, and category for each detector.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(AppServiceOptionDefinitions.AppServiceName.AsRequired());
    }

    protected override BaseAppServiceOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var detectors = await appServiceService.ListWebAppDetectorsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(detectors), AppServiceJsonContext.Default.WebappDetectorListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get diagnostic detectors for Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.AppName, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WebappDetectorListResult(List<WebappDetectorDetails> WebappDetectors);
}
