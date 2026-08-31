// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "9136d433-50df-4e08-bc6a-660881d39421",
    Name = "check-resync-readiness",
    Title = "Check Resilience Drill Resync Readiness",
    Description = "Checks whether a resilience drill is ready to resync in a service group. Starts a resync and readiness check that resyncs the drill's configuration and evaluates whether the drill is ready, then returns the operation ID for the started check. Use this to run a resync readiness check and confirm drill readiness before running the drill.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillCheckResyncReadinessCommand(
    ILogger<DrillCheckResyncReadinessCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillCheckResyncReadinessOptions, DrillCheckResyncReadinessCommand.DrillCheckResyncReadinessCommandResult>
{
    private readonly ILogger<DrillCheckResyncReadinessCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillCheckResyncReadinessOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "--service-group", validationResult);
        ValidatePathSegment(options.Drill, "--drill", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        DrillCheckResyncReadinessOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            DrillResyncReadinessResult readiness = await _resilienceManagementService.CheckDrillResyncReadinessAsync(
                options.ServiceGroup,
                options.Drill,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillCheckResyncReadinessCommandResult(readiness),
                ResilienceManagementJsonContext.Default.DrillCheckResyncReadinessCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking drill resync readiness. ServiceGroup: {ServiceGroup}, Drill: {Drill}.",
                options.ServiceGroup,
                options.Drill);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/'))
        {
            validationResult.Errors.Add($"{optionName} must be a single non-empty path segment.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill resync readiness check cannot start in its current state. Verify that no conflicting drill operation is active.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed starting the drill resync readiness check. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill not found. Verify the drill and service group exist and you have access.",
        RequestFailedException =>
            "The drill resync readiness request failed. Verify the drill and service group, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillCheckResyncReadinessCommandResult(DrillResyncReadinessResult Readiness);
}
