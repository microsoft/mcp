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
    Id = "96622339-b89f-4764-b15f-793bd52d11bf",
    Name = "validateforfailover",
    Title = "Validate Resilience Recovery Plan for Failover",
    Description = "Validates a resilience recovery plan for failover using customer-provided source locations, selected recovery-resource IDs, or both. If the customer provides neither selector, ask which source locations or recovery-resource IDs to validate; never infer them from prior context or resource metadata. Use this tool to check failover qualification or readiness, identify blocking reasons per recovery resource, or supply user consent. This validation-only operation does not execute failover or update recovery resources.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanValidateForFailoverCommand(
    ILogger<RecoveryPlanValidateForFailoverCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanValidateForFailoverOptions, RecoveryPlanValidateForFailoverResult>
{
    private readonly ILogger<RecoveryPlanValidateForFailoverCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanValidateForFailoverOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanValidateForFailoverOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanValidateForFailoverResult result = await _resilienceManagementService.ValidateRecoveryPlanForFailoverAsync(
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
                ResilienceManagementJsonContext.Default.RecoveryPlanValidateForFailoverResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating recovery plan for failover. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed validating the recovery plan for failover. Verify you have access to the recovery plan and service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The failover validation request failed. Verify the recovery plan, source locations, selected resources, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };
}
