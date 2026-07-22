// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

[CommandMetadata(
    Id = "a8aa0966-4c0c-4e22-8854-cced583f0fb2",
    Name = "diagnose",
    Title = "Diagnose an App Service Web App",
    Description = """
        Runs a specific diagnostic detector on an App Service Web App to troubleshoot issues with performance, availability,
        configuration, or errors. Returns detailed analysis results including insights and recommendations. Use this to investigate
        why a web app is slow, failing, restarting, or unhealthy. Requires a detector ID from 'azmcp appservice webapp diagnostic list'.
        Supports optional time range filtering for historical analysis.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DetectorDiagnoseCommand(ILogger<DetectorDiagnoseCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DetectorDiagnoseOptions, DetectorDiagnoseCommand.DetectorDiagnoseResult>(subscriptionResolver)
{
    private readonly ILogger<DetectorDiagnoseCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

    public override void ValidateOptions(DetectorDiagnoseOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        options.StartTime = options.StartTime?.ToUniversalTime();
        options.EndTime = options.EndTime?.ToUniversalTime();

        if (options.StartTime != null && options.EndTime != null && options.StartTime > options.EndTime)
        {
            validationResult.Errors.Add($"Start time '{options.StartTime}' must be earlier than end time '{options.EndTime}'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DetectorDiagnoseOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var diagnoses = await _appServiceService.DiagnoseDetectorAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.App,
                options.DetectorId,
                options.StartTime,
                options.EndTime,
                options.Interval,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(diagnoses), AppServiceJsonContext.Default.DetectorDiagnoseResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get diagnostic detectors for Web App '{App}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.App, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DetectorDiagnoseResult(DiagnosisResults Diagnoses);
}
