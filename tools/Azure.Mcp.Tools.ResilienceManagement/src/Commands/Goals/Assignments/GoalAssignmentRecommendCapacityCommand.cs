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
    Id = "a552c452-dccd-4580-b80c-7d1b1e905a22",
    Name = "recommend-capacity",
    Title = "Recommend Resilience Goal Assignment Capacity",
    Description = "Starts capacity recommendations for a resilience goal assignment in a service group. Assess zonal resiliency eligibility and capacity recommendations for selected Azure resource ARM IDs, or omit resource-ids to assess all eligible service group resources. Returns Accepted and the service operation ID when supplied, not completed recommendations. Does not rediscover resources or change goal participation.",
    OperationPlane = ToolOperationPlane.Control, Destructive = false, Idempotent = false,
    OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentRecommendCapacityCommand(
    ILogger<GoalAssignmentRecommendCapacityCommand> logger,
    IResilienceManagementService service)
    : AuthenticatedCommand<GoalAssignmentRecommendCapacityOptions, GoalAssignmentOperationResult>
{
    public override void ValidateOptions(GoalAssignmentRecommendCapacityOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        GoalAssignmentResourceValidation.ValidateNames(options.ServiceGroup, options.GoalAssignment, validationResult);
        GoalAssignmentResourceValidation.ValidateResourceIds(options.ResourceIds, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentRecommendCapacityOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.RecommendGoalAssignmentCapacityAsync(options.ServiceGroup, options.GoalAssignment, options.ResourceIds, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.GoalAssignmentOperationResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Goal assignment capacity recommendation failed.");
            HandleException(context, ex);
            context.Response.Results = null;
        }
        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => GoalAssignmentResourceValidation.GetErrorMessage(ex);
}
