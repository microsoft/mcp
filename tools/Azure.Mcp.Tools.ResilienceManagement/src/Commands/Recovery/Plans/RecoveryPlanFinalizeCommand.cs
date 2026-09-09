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
    Title = "Finalize Resilience Recoveryplan",
    Description = "Completes or finalizes the current recoveryplan operation by validating resource permissions and updating the recoveryplan state. Returns an operation ID for tracking. This does not commit a completed failover.",
    Destructive = false,
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
            RecoveryPlanFinalizeResult result = await _resilienceManagementService.FinalizeRecoveryPlanAsync(options.ServiceGroup, options.RecoveryPlan, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.RecoveryPlanFinalizeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing recoveryplan. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.", options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The recoveryplan finalize request timed out. Check the recoveryplan state before retrying.",
        RequestFailedException reqEx when reqEx.Status is (int)HttpStatusCode.Conflict or (int)HttpStatusCode.PreconditionFailed => "The recoveryplan cannot be finalized in its current state. Complete active operations and try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => "Authorization failed finalizing the recoveryplan. Verify you have permission to run recoveryplan actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => "Recoveryplan not found. Verify the recoveryplan and service group exist and you have access.",
        RequestFailedException => "The recoveryplan finalize request failed. Verify the recoveryplan and its resource permissions, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException ? HttpStatusCode.GatewayTimeout : base.GetStatusCode(ex);
}
