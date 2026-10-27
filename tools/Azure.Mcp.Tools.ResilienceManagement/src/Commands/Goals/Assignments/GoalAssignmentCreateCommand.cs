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
    Id = "8f0f263b-c2a4-47ba-93e7-c1ed571f362d",
    Name = "create",
    Title = "Create Resilience Goal Assignment",
    Description = """
        Creates a new resilience goal assignment in a service group, with zonal resiliency required
        (require-zonal-resiliency=true) and optional service-level indicator target resource IDs. Uses the GA goal
        schema; no goal template is needed. Waits for provisioning and returns the created assignment. May retry
        failed provisioning, but does not replace a successfully provisioned assignment. To change an existing
        assignment's SLI associations, use update.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentCreateCommand(
    ILogger<GoalAssignmentCreateCommand> logger,
    IResilienceManagementService service)
    : BaseGoalAssignmentCommand<GoalAssignmentCreateOptions, GoalAssignmentCreateCommand.GoalAssignmentCreateCommandResult>
{
    public override void ValidateOptions(GoalAssignmentCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (options.RequireZonalResiliency is not true)
        {
            validationResult.Errors.Add("The Goals service requires --require-zonal-resiliency true when creating an assignment.");
        }

        GoalAssignmentValidation.ValidateServiceLevelResourceIds(options.ServiceLevelResourceIds, options.ServiceGroup, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await service.CreateGoalAssignmentAsync(
                options.ServiceGroup, options.GoalAssignment, options.RequireZonalResiliency!.Value,
                options.ServiceLevelResourceIds, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentCreateCommandResult(assignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup, options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GoalAssignmentCreateCommandResult(GoalAssignmentInfo GoalAssignment);
}
