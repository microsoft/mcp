// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "6f991f5e-0218-46b5-8d6d-8b59defb1143",
    Name = "checkreadiness",
    Title = "Check Resilience Recovery Plan Readiness",
    Description = "Checks whether a resilience recovery plan and its protected resources are ready for recovery operations in an Azure service group.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanCheckReadinessCommand(ILogger<RecoveryPlanCheckReadinessCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanCheckReadinessOptions, RecoveryPlanReadinessResult>
{
    private readonly ILogger<RecoveryPlanCheckReadinessCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanCheckReadinessOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanCheckReadinessOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanReadinessResult result = await _resilienceManagementService.CheckRecoveryPlanReadinessAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanReadinessResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking recovery plan readiness. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException =>
            "The recovery plan readiness check timed out before it completed. Retry the operation.",
        InvalidOperationException =>
            "The recovery plan readiness check completed without returning a valid recovery job response. Retry the operation. If the problem persists, contact support.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The recovery plan readiness check cannot start in its current state. Complete or cancel active recovery operations and try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed checking recovery plan readiness. Verify you have permission to run recovery plan actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The recovery plan readiness request failed. Verify the recovery plan, service group, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.GatewayTimeout,
        InvalidOperationException => HttpStatusCode.BadGateway,
        _ => base.GetStatusCode(ex)
    };
}
