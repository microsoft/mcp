// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "b438c217-b579-41a3-96a2-393a6c6f4513",
    Name = "generate-report",
    Title = "Generate a Resilience Drill Run Report",
    Description = """
        Generates or refreshes the report for a resilience drill run; a finalized report is left unchanged. It starts the
        report generation operation and returns the operation ID, whether it has completed, and the report summary when
        available: generation status, per-stage statuses, available formats, and last generated time.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunGenerateReportCommand(ILogger<DrillRunGenerateReportCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunGenerateReportOptions, DrillRunGenerateReportCommand.DrillRunGenerateReportCommandResult>
{
    private readonly ILogger<DrillRunGenerateReportCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunGenerateReportOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunGenerateReportOptions options, CancellationToken cancellationToken)
    {
        try
        {
            DrillRunGenerateReportResult result = await _resilienceManagementService.GenerateDrillRunReportAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillRunGenerateReportCommandResult(result),
                ResilienceManagementJsonContext.Default.DrillRunGenerateReportCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating drill run report. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        // The base handler copies the raw exception message into Results; only the sanitized Message may reach callers.
        context.Response.Results = null;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed generating the drill run report. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotImplemented =>
            "Report generation is not available because report storage is not configured in this environment.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.ServiceUnavailable =>
            "The service could not generate the drill run report because a dependency did not respond. Try again.",
        RequestFailedException =>
            "The drill run report generation request failed. Verify the drill run, drill, and service group, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    public sealed record DrillRunGenerateReportCommandResult(DrillRunGenerateReportResult Result);
}
