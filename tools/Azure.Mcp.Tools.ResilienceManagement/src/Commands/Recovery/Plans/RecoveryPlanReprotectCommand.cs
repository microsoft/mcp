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
    Id = "4de82b03-68f7-45b2-8a57-925411cc89d6",
    Name = "reprotect",
    Title = "Reprotect Resilience Recoveryplan",
    Description = "Starts reprotection after failover for all qualified resources in a recoveryplan or for explicitly selected recovery-resource IDs. This destructive operation changes protection state and returns operation and recovery job IDs for tracking. Use validateforreprotect first when qualification is unknown.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanReprotectCommand(
    ILogger<RecoveryPlanReprotectCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanReprotectOptions, RecoveryPlanReprotectResult>
{
    private readonly ILogger<RecoveryPlanReprotectCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanReprotectOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);
        RecoveryPlanValidation.ValidateSelectedResourceIds(options.SelectedResourceIds, options.ServiceGroup, options.RecoveryPlan, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanReprotectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanReprotectResult result = await _resilienceManagementService.ReprotectRecoveryPlanAsync(options.ServiceGroup, options.RecoveryPlan, options.SelectedResourceIds, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.RecoveryPlanReprotectResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recoveryplan reprotect. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.", options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The recoveryplan reprotect request timed out. Check recovery jobs before retrying to avoid starting the operation twice.",
        RequestFailedException reqEx when reqEx.Status is (int)HttpStatusCode.Conflict or (int)HttpStatusCode.PreconditionFailed => "Recoveryplan reprotection cannot start in its current state. Complete active operations and validate reprotect qualification before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => "Authorization failed starting recoveryplan reprotection. Verify you have permission to run recoveryplan actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => "Recoveryplan not found. Verify the recoveryplan and service group exist and you have access.",
        RequestFailedException => "The reprotect request failed. Verify the recoveryplan, selected resources, and qualification, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException ? HttpStatusCode.GatewayTimeout : base.GetStatusCode(ex);
}
