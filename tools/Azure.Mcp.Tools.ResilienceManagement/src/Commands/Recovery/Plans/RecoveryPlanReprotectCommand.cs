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
    Title = "Reprotect Resilience Recovery Plan",
    Description = "Starts reprotection for explicitly selected resources in a qualified resilience recovery plan after failover and returns an operation ID for tracking. This destructive operation changes recovery protection state. Ask which recovery-resource IDs to reprotect when they are omitted; never infer them from prior context or resource metadata. Use validateforreprotect before execution when qualification is unknown.",
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

        if (options.SelectedResourceIds is not { Length: > 0 })
        {
            validationResult.Errors.Add("Provide at least one --selected-resource-ids value.");
        }

        RecoveryPlanValidation.ValidateSelectedResourceIds(
            options.SelectedResourceIds,
            options.ServiceGroup,
            options.RecoveryPlan,
            validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanReprotectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanReprotectResult result = await _resilienceManagementService.ReprotectRecoveryPlanAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.SelectedResourceIds,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanReprotectResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting recovery plan reprotect. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException =>
            "The recovery plan reprotect request timed out before it started. Check recovery jobs before retrying to avoid starting the operation twice.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The recovery plan reprotect operation cannot start in its current state. Complete active recovery operations and validate reprotect qualification before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed starting recovery plan reprotect. Verify you have permission to run recovery plan actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The reprotect request failed. Verify the recovery plan, selected resources, and qualification, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.GatewayTimeout,
        _ => base.GetStatusCode(ex)
    };
}