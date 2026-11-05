// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

[CommandMetadata(
    Id = "e891e78d-15bc-45a7-956e-0e1991e29d2f",
    Name = "update",
    Title = "Update Resilience Goal Assignment",
    Description = """
        Replaces or clears the service-level indicator associations of an existing resilience goal
        assignment in a service group. Preserves the assignment's zonal resiliency requirement and does not delete
        the indicator resources. Uses the GA assignment update API, waits for completion, and returns the updated
        assignment. This changes assignment configuration, not per-resource inclusion or manual attestation; use
        update-resources for those.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentUpdateCommand(
    ILogger<GoalAssignmentUpdateCommand> logger,
    IResilienceManagementService service)
    : BaseGoalAssignmentCommand<GoalAssignmentUpdateOptions, GoalAssignmentUpdateCommand.GoalAssignmentUpdateCommandResult>
{
    public override void ValidateOptions(GoalAssignmentUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if ((options.ServiceLevelResourceIds is not null) == options.ClearServiceLevelResources)
        {
            validationResult.Errors.Add("Specify service-level-resource-ids or clear-service-level-resources, but not both.");
        }

        if (options.ServiceLevelResourceIds is { Length: 0 })
        {
            validationResult.Errors.Add("Use clear-service-level-resources to explicitly clear the indicator associations.");
        }

        GoalAssignmentValidation.ValidateServiceLevelResourceIds(options.ServiceLevelResourceIds, options.ServiceGroup, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await service.UpdateGoalAssignmentAsync(
                options.ServiceGroup, options.GoalAssignment, options.ServiceLevelResourceIds ?? [],
                options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentUpdateCommandResult(assignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentUpdateCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup, options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GoalAssignmentUpdateCommandResult(GoalAssignmentInfo GoalAssignment);
}
