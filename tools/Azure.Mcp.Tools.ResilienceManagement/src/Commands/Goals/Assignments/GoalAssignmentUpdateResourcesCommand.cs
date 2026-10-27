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
    Id = "f70b3679-dc78-4750-a7fb-2c9107a97eb1",
    Name = "update-resources",
    Title = "Update Resilience Goal Assignment Resources",
    Description = "Updates discovered resources under a resilience goal assignment: include or exclude resources from high availability or disaster recovery goals, set attestation or user confirmations. Supply a JSON array of existing goal resource IDs and their properties, including underlying Azure resourceArmId. This is per-resource replacement, not PATCH: read current properties first and include all desired disaster recovery fields. Returns Accepted and the service operation ID when supplied. Does not change Azure resources or service group membership, rediscover resources, or recommend capacity.",
    OperationPlane = ToolOperationPlane.Control, Destructive = true, Idempotent = false,
    OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentUpdateResourcesCommand(
    ILogger<GoalAssignmentUpdateResourcesCommand> logger,
    IResilienceManagementService service)
    : AuthenticatedCommand<GoalAssignmentUpdateResourcesOptions, GoalAssignmentOperationResult>
{
    public override void ValidateOptions(GoalAssignmentUpdateResourcesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        GoalAssignmentResourceValidation.ValidateNames(options.ServiceGroup, options.GoalAssignment, validationResult);
        GoalAssignmentResourceValidation.ValidateResources(options.Resources, options.ServiceGroup, options.GoalAssignment, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentUpdateResourcesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateGoalAssignmentResourcesAsync(options.ServiceGroup, options.GoalAssignment, options.Resources, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.GoalAssignmentOperationResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Goal assignment resource update failed.");
            HandleException(context, ex);
            context.Response.Results = null;
        }
        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => GoalAssignmentResourceValidation.GetErrorMessage(ex);
}
