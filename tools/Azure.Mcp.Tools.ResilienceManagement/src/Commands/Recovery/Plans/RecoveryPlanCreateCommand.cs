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
        Creates a new Zonal resilience recovery plan in an Azure service group or fully updates an existing recovery plan.
        Creation requires a plan description and a customer-selected SystemAssigned, UserAssigned, or SystemAndUserAssigned
        managed identity. Do not assume an identity type; ask the user to choose one when omitted. Updates can switch identity
        types, but cannot replace an existing user-assigned identity with a different one. Updates preserve the default recovery
        group ID, additional recovery groups, and omitted plan or default group descriptions. Plan descriptions must be 5 to 50
        characters. A user-assigned identity update must include the existing identity's full resource ID; ask for it when omitted.
        """,
    OperationPlane = ToolOperationPlane.Control,
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

        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);

        if (options.PlanDescription is not null && options.PlanDescription.Length is < 5 or > 50)
        {
            validationResult.Errors.Add("The recovery plan description must be 5 to 50 characters.");
        }

        if (options.DefaultGroupDescription is not null && options.DefaultGroupDescription.Length is < 5 or > 50)
        {
            validationResult.Errors.Add("The default recovery group description must be 5 to 50 characters when specified.");
        }

        if (options.IdentityType != Models.RecoveryPlanIdentityKind.SystemAssigned && string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
        {
            validationResult.Errors.Add("--user-assigned-identity is required when --identity-type is UserAssigned or SystemAndUserAssigned.");
        }
        else if (options.IdentityType == Models.RecoveryPlanIdentityKind.SystemAssigned && !string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
        {
            validationResult.Errors.Add("--user-assigned-identity is not allowed when --identity-type is SystemAssigned.");
        }
        else if (!string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
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
                options.IdentityType,
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

    public record RecoveryPlanCreateCommandResult(Models.RecoveryPlanInfo RecoveryPlan);
}
