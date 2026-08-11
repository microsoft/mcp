// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "2fbfa9e6-0a5e-45e4-923d-0ef3706ef733",
    Name = "create",
    Title = "Create or Update Resilience Recovery Plan",
    Description = """
        Create or fully update a Zonal resilience recovery plan in my service group. New plans use a system-assigned
        managed identity by default, or an optional pre-provisioned user-assigned identity. Ensure the selected
        identity has the Azure RBAC roles required by the recovery resources; Recovery Orchestration role assignment
        is best effort. Updates preserve the existing managed identity, default recovery group ID, additional
        recovery groups, and omitted default group description.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanCreateCommand(ILogger<RecoveryPlanCreateCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanCreateOptions, RecoveryPlanCreateCommand.RecoveryPlanCreateCommandResult>
{
    private readonly ILogger<RecoveryPlanCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.PlanType != Models.RecoveryPlanKind.Zonal)
        {
            validationResult.Errors.Add("Only Zonal recovery plans are currently supported.");
        }

        if (options.RecoveryPlan.Length is < 5 or > 24 || !options.RecoveryPlan.All(IsValidRecoveryPlanNameCharacter))
        {
            validationResult.Errors.Add("The recovery plan name must be 5 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }

        if (options.PlanDescription.Length > 50)
        {
            validationResult.Errors.Add("The recovery plan description must not exceed 50 characters.");
        }

        if (!string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
        {
            try
            {
                _ = ResilienceManagementService.ParseUserAssignedIdentityResourceId(options.UserAssignedIdentity);
            }
            catch (ArgumentException ex)
            {
                validationResult.Errors.Add(ex.Message);
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var recoveryPlan = await _resilienceManagementService.CreateRecoveryPlanAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.PlanType,
                options.PlanDescription,
                options.UserAssignedIdentity,
                options.DefaultGroupDescription,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RecoveryPlanCreateCommandResult(recoveryPlan),
                ResilienceManagementJsonContext.Default.RecoveryPlanCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating recovery plan. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, PlanType: {PlanType}.",
                options.ServiceGroup, options.RecoveryPlan, options.PlanType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool IsValidRecoveryPlanNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argumentException => argumentException.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The recovery plan could not be created or updated because it conflicts with the current resource state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating or updating the recovery plan. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Service group not found. Verify the service group exists and you have access.",
        RequestFailedException =>
            "The recovery plan request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public record RecoveryPlanCreateCommandResult(System.Text.Json.JsonElement RecoveryPlan);
}