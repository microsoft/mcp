// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "e694c9e4-f134-48b2-b6ed-5f6a617d7d8d",
    Name = "delete",
    Title = "Delete Resilience Recovery Plan",
    Description = "Deletes a resilience recovery plan from an Azure service group. This idempotent operation returns Deleted = false when the recovery plan does not exist.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanDeleteCommand(ILogger<RecoveryPlanDeleteCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanDeleteOptions, RecoveryPlanDeleteCommand.RecoveryPlanDeleteCommandResult>
{
    private readonly ILogger<RecoveryPlanDeleteCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.RecoveryPlan.Length is < 5 or > 24 || !options.RecoveryPlan.All(IsValidRecoveryPlanNameCharacter))
        {
            validationResult.Errors.Add("The recovery plan name must be 5 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await _resilienceManagementService.DeleteRecoveryPlanAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RecoveryPlanDeleteCommandResult(deleted, options.RecoveryPlan),
                ResilienceManagementJsonContext.Default.RecoveryPlanDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting recovery plan. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool IsValidRecoveryPlanNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The recovery plan cannot be deleted in its current state. Complete or cancel active recovery operations and try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed deleting the recovery plan. Verify you have permission to delete recovery plans in the service group.",
        RequestFailedException =>
            "The recovery plan delete request failed. Verify the recovery plan, service group, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecoveryPlanDeleteCommandResult(bool Deleted, string RecoveryPlan);
}