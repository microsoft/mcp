// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "2b68f7b2-fcaa-44a5-9968-d4a3ec3950d4",
    Name = "validateforoperation",
    Title = "Validate Resilience Recovery Plan for Operation",
    Description = "Validates a customer-selected or unspecified recovery operation for a resilience recovery plan before execution. Use for requests to validate an operation, an intended recovery operation, or operation-specific pre-validation, including when earlier context mentions failover but the current operation is unspecified. Ask the customer to choose Failover, FailoverCommit, Reprotect, TestFailover, or TestFailoverCleanup; do not assume the operation. Checks the current state, readiness, and permissions. Not for general readiness or per-resource failover or reprotect qualification. Does not execute recovery operations.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanValidateForOperationCommand(
    ILogger<RecoveryPlanValidateForOperationCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanValidateForOperationOptions, RecoveryPlanValidateForOperationResult>
{
    private readonly ILogger<RecoveryPlanValidateForOperationCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanValidateForOperationOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);

        if (!TryGetOperationName(options.OperationName, out _))
        {
            validationResult.Errors.Add("--operation-name must be Failover, FailoverCommit, Reprotect, TestFailover, or TestFailoverCleanup.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanValidateForOperationOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _ = TryGetOperationName(options.OperationName, out RecoveryOperationNames operationName);
            RecoveryPlanValidateForOperationResult result = await _resilienceManagementService.ValidateRecoveryPlanForOperationAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                operationName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanValidateForOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating recovery plan for operation. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, OperationName: {OperationName}.",
                options.ServiceGroup, options.RecoveryPlan, options.OperationName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool TryGetOperationName(string operationName, out RecoveryOperationNames value)
    {
        if (string.Equals(operationName, "Failover", StringComparison.OrdinalIgnoreCase))
        {
            value = RecoveryOperationNames.Failover;
            return true;
        }

        if (string.Equals(operationName, "FailoverCommit", StringComparison.OrdinalIgnoreCase))
        {
            value = RecoveryOperationNames.FailoverCommit;
            return true;
        }

        if (string.Equals(operationName, "Reprotect", StringComparison.OrdinalIgnoreCase))
        {
            value = RecoveryOperationNames.Reprotect;
            return true;
        }

        if (string.Equals(operationName, "TestFailover", StringComparison.OrdinalIgnoreCase))
        {
            value = RecoveryOperationNames.TestFailover;
            return true;
        }

        if (string.Equals(operationName, "TestFailoverCleanup", StringComparison.OrdinalIgnoreCase))
        {
            value = RecoveryOperationNames.TestFailoverCleanup;
            return true;
        }

        value = default;
        return false;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed validating the recovery plan operation. Verify you have access to the recovery plan and service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The recovery plan operation validation request failed. Verify the recovery plan, operation name, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };
}
