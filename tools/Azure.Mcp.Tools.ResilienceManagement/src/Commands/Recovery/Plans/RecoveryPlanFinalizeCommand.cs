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
    Id = "16cb8656-5370-40e0-912e-d4e82c36239c",
    Name = "finalize",
    Title = "Finalize Resilience Recovery Plan",
    Description = "Finalizes a configured editable resilience recovery plan and returns an operation ID for tracking. This destructive state-transition operation validates the plan configuration and moves it toward ready; it is not failover commit and does not execute failover. Use this tool only when the customer explicitly asks to finalize the named recovery plan.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanFinalizeCommand(
    ILogger<RecoveryPlanFinalizeCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanFinalizeOptions, RecoveryPlanFinalizeResult>
{
    private readonly ILogger<RecoveryPlanFinalizeCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanFinalizeOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanFinalizeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanFinalizeResult result = await _resilienceManagementService.FinalizeRecoveryPlanAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanFinalizeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error finalizing recovery plan. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException =>
            "The recovery plan finalize request timed out before it started. Check the recovery plan and recovery jobs before retrying to avoid starting the operation twice.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The recovery plan cannot be finalized in its current state. Complete its configuration or active recovery operations, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed finalizing the recovery plan. Verify you have permission to run recovery plan actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The finalize request failed. Verify the recovery plan is fully configured and editable, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.GatewayTimeout,
        _ => base.GetStatusCode(ex)
    };
}
