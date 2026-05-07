// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

[CommandMetadata(
    Id = "7807fdb6-4b92-4361-8042-be61dd342e17",
    Name = "list",
    Title = "List the Diagnostic Detectors for an App Service Web App",
    Description = """
        Lists all available diagnostic detectors for an App Service Web App. Use this to discover which diagnostics
        are available before running a specific detector. Returns the detector ID, name, type, description, category,
        and analysis types for each detector. Useful for troubleshooting app service issues, checking available
        health checks, and finding the right detector for performance, availability, or configuration analysis.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DetectorListCommand(ILogger<DetectorListCommand> logger, IAppServiceService appServiceService)
    : BaseAppServiceCommand<BaseAppServiceOptions>(resourceGroupRequired: true, appRequired: true)
{
    private readonly ILogger<DetectorListCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

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

            var detectors = await _appServiceService.ListDetectorsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(detectors), AppServiceJsonContext.Default.DetectorListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get diagnostic detectors for Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.AppName, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DetectorListResult(List<DetectorDetails> Detectors);
}
