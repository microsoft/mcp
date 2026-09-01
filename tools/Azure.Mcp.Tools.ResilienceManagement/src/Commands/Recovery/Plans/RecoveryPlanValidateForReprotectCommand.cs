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
    Id = "a58a9d25-b69a-4a8e-9ad0-04fb693352de",
    Name = "validateforreprotect",
    Title = "Validate Resilience Recovery Plan for Reprotect",
    Description = "Validates whether a resilience recovery plan and its resources are qualified for reprotect after failover. Optionally validates customer-selected recovery-resource IDs; when no IDs are provided, validates all qualified resources in the plan. Use this validation-only tool to identify per-resource reprotect eligibility and blocking reasons. It does not execute reprotect or update recovery resources.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanValidateForReprotectCommand(
    ILogger<RecoveryPlanValidateForReprotectCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanValidateForReprotectOptions, RecoveryPlanValidateForReprotectResult>
{
    private readonly ILogger<RecoveryPlanValidateForReprotectCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanValidateForReprotectOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);
        RecoveryPlanValidation.ValidateSelectedResourceIds(
            options.SelectedResourceIds,
            options.ServiceGroup,
            options.RecoveryPlan,
            validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanValidateForReprotectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanValidateForReprotectResult result = await _resilienceManagementService.ValidateRecoveryPlanForReprotectAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.SelectedResourceIds,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanValidateForReprotectResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating recovery plan for reprotect. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException =>
            "The recovery plan reprotect validation timed out before it completed. Retry the operation.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed validating the recovery plan for reprotect. Verify you have access to the recovery plan and service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The reprotect validation request failed. Verify the recovery plan, selected resources, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.GatewayTimeout,
        _ => base.GetStatusCode(ex)
    };
}
