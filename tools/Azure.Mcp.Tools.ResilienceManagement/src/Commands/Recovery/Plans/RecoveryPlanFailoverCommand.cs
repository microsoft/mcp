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
    Id = "207d19cd-06fb-4aab-b3a9-d233935c6229",
    Name = "failover",
    Title = "Fail Over Resilience Recovery Plan",
    Description = "Starts failover for a qualified resilience recovery plan using customer-provided source locations, selected recovery-resource IDs, or both, and returns an operation ID for tracking. This destructive operation starts recovery workloads in their target locations. If the customer provides neither selector, ask which source locations or recovery-resource IDs to fail over; never infer them from prior context or resource metadata. Use validateforfailover and checkreadiness before execution when qualification or readiness is unknown.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanFailoverCommand(
    ILogger<RecoveryPlanFailoverCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanFailoverOptions, RecoveryPlanFailoverResult>
{
    private readonly ILogger<RecoveryPlanFailoverCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanFailoverOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);

        bool hasSourceLocations = options.SourceLocations is { Length: > 0 };
        bool hasSelectedResourceIds = options.SelectedResourceIds is { Length: > 0 };
        if (!hasSourceLocations && !hasSelectedResourceIds)
        {
            validationResult.Errors.Add("Provide at least one --source-locations or --selected-resource-ids value.");
        }

        if (hasSourceLocations && options.SourceLocations!.Any(string.IsNullOrWhiteSpace))
        {
            validationResult.Errors.Add("Each --source-locations value must be a non-empty Azure location.");
        }

        if (options.UserConsent is not null &&
            !string.Equals(options.UserConsent, "Unspecified", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(options.UserConsent, "Allowed", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--user-consent must be Unspecified or Allowed when specified.");
        }

        RecoveryPlanValidation.ValidateSelectedResourceIds(
            options.SelectedResourceIds,
            options.ServiceGroup,
            options.RecoveryPlan,
            validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanFailoverOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanFailoverResult result = await _resilienceManagementService.FailoverRecoveryPlanAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.SourceLocations ?? [],
                options.SelectedResourceIds,
                options.UserConsent,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanFailoverResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting recovery plan failover. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException =>
            "The recovery plan failover request timed out before it started. Check recovery jobs before retrying to avoid starting the operation twice.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The recovery plan failover cannot start in its current state. Complete active recovery operations, validate failover qualification and readiness, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed starting recovery plan failover. Verify you have permission to run recovery plan actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The failover request failed. Verify the recovery plan, source locations, selected resources, consent, and readiness, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.GatewayTimeout,
        _ => base.GetStatusCode(ex)
    };
}