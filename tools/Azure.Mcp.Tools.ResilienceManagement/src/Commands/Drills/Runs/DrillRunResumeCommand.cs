// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "e04ee466-d3ec-4641-844a-8f3204499e1b",
    Name = "resume",
    Title = "Resume a Resilience Drill Run",
    Description = "Resumes a failover drill run paused after fault injection so it can proceed to the failover stage.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunResumeCommand(ILogger<DrillRunResumeCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunResumeOptions, DrillRunResumeCommand.DrillRunResumeCommandResult>
{
    private readonly ILogger<DrillRunResumeCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunResumeOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunResumeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _resilienceManagementService.ResumeDrillRunAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                options.Tenant,
                cancellationToken);

            var result = new DrillRunResumeCommandResult(options.DrillRun, Accepted: true);
            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunResumeCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error resuming drill run. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill run cannot be resumed in its current state. Verify it is paused after fault injection, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed resuming the drill run. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException =>
            "The drill run resume request failed. Verify the drill run is paused and the drill and service group are correct, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    public sealed record DrillRunResumeCommandResult(string DrillRun, bool Accepted);
}
