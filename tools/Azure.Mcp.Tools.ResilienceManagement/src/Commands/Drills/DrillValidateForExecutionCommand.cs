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
    Name = "validate-for-execution",
    Title = "Validate Resilience Drill For Execution",
    Description = "Validates a named resilience drill for execution from specified physical source locations. Use this command to validate, preflight, or check drill execution readiness before running the drill. It starts validation and returns the operation ID; it does not get or list drill definitions or resources.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillValidateForExecutionCommand(
    ILogger<DrillValidateForExecutionCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillValidateForExecutionOptions, DrillValidateForExecutionCommand.DrillValidateForExecutionCommandResult>
{
    private readonly ILogger<DrillValidateForExecutionCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillValidateForExecutionOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "--service-group", validationResult);
        ValidatePathSegment(options.Drill, "--drill", validationResult);

        if (options.SourceLocations.Length == 0 || options.SourceLocations.Any(string.IsNullOrWhiteSpace))
        {
            validationResult.Errors.Add("--source-locations must contain at least one non-empty location.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        DrillValidateForExecutionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            DrillValidateForExecutionResult validation = await _resilienceManagementService.ValidateDrillForExecutionAsync(
                options.ServiceGroup,
                options.Drill,
                options.SourceLocations,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillValidateForExecutionCommandResult(validation),
                ResilienceManagementJsonContext.Default.DrillValidateForExecutionCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating drill for execution. ServiceGroup: {ServiceGroup}, Drill: {Drill}.",
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
            "The drill cannot be validated in its current state. Verify that no conflicting drill operation is active.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed validating the drill. Verify you have permission to execute drills in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill not found. Verify the drill and service group exist and you have access.",
        RequestFailedException =>
            "The drill validation request failed. Verify the drill, service group, and source locations, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillValidateForExecutionCommandResult(DrillValidateForExecutionResult Validation);
}
