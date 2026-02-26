// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

public sealed class WebappDetectorGetCommand(ILogger<WebappDetectorGetCommand> logger)
    : BaseWebappDiagnosticCommand<WebappDetectorGetOptions>(diagnosticCategoryRequired: true)
{
    private const string CommandTitle = "Gets the Diagnostic Detector for a Diagnostic Category";
    private readonly ILogger<WebappDetectorGetCommand> _logger = logger;
    public override string Id => "a8aa0966-4c0c-4e22-8854-cced583f0fb2";
    public override string Name => "get-detector";

    public override string Description =>
        """
        Retrieves detailed information about a diagnostic category's detectors, returning the name, kind, type,
        description, whether the detector is enabled, and the rank for the detector. If a detector name is provided the
        details for that specific detector is returned. Otherwise all detectors for the diagnostic category are
        returned.
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
        command.Options.Add(AppServiceOptionDefinitions.DetectorName);
    }

    protected override WebappDetectorGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DetectorName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.DetectorName.Name);
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
            var detectors = await appServiceService.GetWebAppDetectorsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.DiagnosticCategory!,
                options.DetectorName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(detectors), AppServiceJsonContext.Default.WebappDetectorGetResult);
        }
        catch (Exception ex)
        {
            if (options.DetectorName == null)
            {
                _logger.LogError(ex, "Failed to get diagnostic category detectors for diagnostic category '{DiagnosticCategory}' and Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.DiagnosticCategory, options.AppName, options.Subscription, options.ResourceGroup);
            }
            else
            {
                _logger.LogError(ex, "Failed to get diagnostic category detector '{Detector}' for diagnostic category '{DiagnosticCategory}' and Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.DetectorName, options.DiagnosticCategory, options.AppName, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WebappDetectorGetResult(List<WebappDetectorDetails> WebappDetectors);
}
