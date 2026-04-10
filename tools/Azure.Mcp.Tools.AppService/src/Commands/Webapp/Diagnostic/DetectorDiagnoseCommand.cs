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
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

public sealed class DetectorDiagnoseCommand(ILogger<DetectorDiagnoseCommand> logger)
    : BaseAppServiceCommand<DetectorDiagnoseOptions>(resourceGroupRequired: true, appRequired: true)
{
    private const string CommandTitle = "Diagnose an App Service Web App";
    private readonly ILogger<DetectorDiagnoseCommand> _logger = logger;
    public override string Id => "a8aa0966-4c0c-4e22-8854-cced583f0fb2";
    public override string Name => "diagnose";

    public override string Description =>
        """
        Diagnoses an App Service Web App with the specified detector, returning the diagnostic results of the detector.
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
        command.Options.Add(AppServiceOptionDefinitions.DetectorName.AsRequired());
        command.Options.Add(AppServiceOptionDefinitions.StartTime);
        command.Options.Add(AppServiceOptionDefinitions.EndTime);
        command.Options.Add(AppServiceOptionDefinitions.Interval);
        command.Validators.Add(result =>
        {
            var startTime = result.GetValueOrDefault<string?>(AppServiceOptionDefinitions.StartTime.Name);
            var endTime = result.GetValueOrDefault<string?>(AppServiceOptionDefinitions.EndTime.Name);

            bool hasStartTime = !string.IsNullOrEmpty(startTime);
            bool hasEndTime = !string.IsNullOrEmpty(endTime);

            if (hasStartTime && !DateTimeOffset.TryParse(startTime, out _))
            {
                result.AddError($"Invalid start time format: {startTime}. Please provide a valid ISO format date time string.");
            }

            if (hasEndTime && !DateTimeOffset.TryParse(endTime, out _))
            {
                result.AddError($"Invalid end time format: {endTime}. Please provide a valid ISO format date time string.");
            }

            if (hasStartTime && hasEndTime
                && DateTimeOffset.TryParse(startTime, out var start)
                && DateTimeOffset.TryParse(endTime, out var end)
                && start > end)
            {
                result.AddError($"Start time '{startTime}' must be earlier than end time '{endTime}'.");
            }
        });
    }

    protected override DetectorDiagnoseOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DetectorName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.DetectorName.Name);
        if (DateTimeOffset.TryParse(parseResult.GetValueOrDefault<string?>(AppServiceOptionDefinitions.StartTime.Name), out var startTime))
        {
            options.StartTime = startTime.ToUniversalTime();
        }
        if (DateTimeOffset.TryParse(parseResult.GetValueOrDefault<string?>(AppServiceOptionDefinitions.EndTime.Name), out var endTime))
        {
            options.EndTime = endTime.ToUniversalTime();
        }
        options.Interval = parseResult.GetValueOrDefault<string?>(AppServiceOptionDefinitions.Interval.Name);
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
            var diagnoses = await appServiceService.DiagnoseDetectorAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.DetectorName!,
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
            _logger.LogError(ex, "Failed to get diagnostic detectors for Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                options.AppName, options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DetectorDiagnoseResult(DiagnosisResults Diagnoses);
}
