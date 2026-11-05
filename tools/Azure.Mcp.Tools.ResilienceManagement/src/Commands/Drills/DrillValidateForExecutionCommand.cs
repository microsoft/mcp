// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "90da933a-bbf3-48ec-a03b-5db38e0c0a3b",
    Name = "validate-for-execution",
    Title = "Validate Resilience Drill For Execution",
    Description = "Validates a resilience drill for execution from source locations in its service group. Preflight drill readiness without starting fault injection, or validate Reprotect without sources. Otherwise source locations are required. Optionally select Failover, Reprotect, FailoverReverse, or ReprotectReverse. Returns a validation operation ID.",
    OperationPlane = ToolOperationPlane.Control,
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

        if (options.OperationName is { } operation && !Enum.IsDefined(operation))
        {
            validationResult.Errors.Add("--operation-name must be Failover, Reprotect, FailoverReverse, or ReprotectReverse.");
        }

        if (options.OperationName != DrillExecutionOperation.Reprotect && options.SourceLocations is not { Length: > 0 })
        {
            validationResult.Errors.Add("--source-locations is required unless --operation-name is Reprotect.");
        }
        else if (options.SourceLocations is { } locations && (locations.Length == 0 || locations.Any(string.IsNullOrWhiteSpace)))
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
            DrillRunTask? operationName = null;
            if (options.OperationName is { } operation)
            {
                operationName = new DrillRunTask(operation.ToString());
            }
            DrillValidateForExecutionResult validation = await _resilienceManagementService.ValidateDrillForExecutionAsync(
                options.ServiceGroup,
                options.Drill,
                options.SourceLocations,
                operationName,
                options.Tenant,
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
