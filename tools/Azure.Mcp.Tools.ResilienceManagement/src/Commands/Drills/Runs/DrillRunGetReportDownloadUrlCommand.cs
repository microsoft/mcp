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
    Id = "2ff347c8-5d54-41a6-839b-e30599b61432",
    Name = "get-report-download-url",
    Title = "Get a Resilience Drill Run Report Download URL",
    Description = """
        Gets a short-lived, read-only download URL for the generated report of a resilience drill run, with the report
        format and the time the URL expires. The report must already have been generated. Treat the returned URL as a
        secret and do not share it.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true,
    LocalRequired = false)]
public sealed class DrillRunGetReportDownloadUrlCommand(ILogger<DrillRunGetReportDownloadUrlCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunGetReportDownloadUrlOptions, DrillRunGetReportDownloadUrlCommand.DrillRunGetReportDownloadUrlCommandResult>
{
    private static readonly string[] AllowedFormats = ["Html"];

    private readonly ILogger<DrillRunGetReportDownloadUrlCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunGetReportDownloadUrlOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);

        if (options.Format is not null && !TryResolveFormat(options.Format, out _))
        {
            validationResult.Errors.Add($"--format must be one of: {string.Join(", ", AllowedFormats)}.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunGetReportDownloadUrlOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string? format = options.Format is null ? null : (TryResolveFormat(options.Format, out string resolved) ? resolved : null);
            DrillRunReportDownloadInfo result = await _resilienceManagementService.GetDrillRunReportDownloadUrlAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                format,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillRunGetReportDownloadUrlCommandResult(result),
                ResilienceManagementJsonContext.Default.DrillRunGetReportDownloadUrlCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting drill run report download URL. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool TryResolveFormat(string value, out string format)
    {
        format = string.Empty;
        foreach (string allowed in AllowedFormats)
        {
            if (allowed.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                format = allowed;
                return true;
            }
        }

        return false;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.GatewayTimeout,
        // A failed long-running operation surfaces the status of its successful status poll; report the failure instead.
        RequestFailedException reqEx when reqEx.Status < 400 => HttpStatusCode.BadGateway,
        _ => base.GetStatusCode(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        // The base handler copies the raw exception message into Results; only the sanitized Message may reach callers.
        context.Response.Results = null;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status < 400 && !string.IsNullOrEmpty(reqEx.ErrorCode) =>
            $"The service could not create the report download URL ({reqEx.ErrorCode}). Verify the report was generated successfully, then try again.",
        RequestFailedException reqEx when reqEx.Status < 400 =>
            "The service could not create the report download URL. Verify the report was generated successfully, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the drill run report download URL. Verify you have permission to read drill runs in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "No report is available for this drill run. Verify the drill run, drill, and service group exist, generate the report with generate-report and wait for it to succeed, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotImplemented =>
            "Report download is not available because report storage is not configured in this environment.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.ServiceUnavailable =>
            "The service could not create the report download URL because a dependency did not respond. Try again.",
        RequestFailedException =>
            "The drill run report download URL request failed. Verify the drill run, drill, and service group and that the report was generated, then try again.",
        TimeoutException => "Getting the drill run report download URL timed out. Try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    public sealed record DrillRunGetReportDownloadUrlCommandResult(DrillRunReportDownloadInfo Result);
}
