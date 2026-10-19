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
    Id = "f8b190a0-9034-4366-a51e-759243644e32",
    Name = "recommend-capacity",
    Title = "Recommend Resilience Goal Capacity",
    Description = """
        Starts a zonal resiliency capacity assessment for an Azure goal assignment. Specify up to 50
        underlying resource ARM IDs, or omit them to discover non-resilient resources automatically.
        This generates resilience capacity recommendations; it does not provision or resize resources.
        Returns the service operation ID and Accepted status unless already completed.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentRecommendCapacityCommand(
    ILogger<GoalAssignmentRecommendCapacityCommand> logger,
    IResilienceManagementService service)
    : BaseGoalAssignmentCommand<GoalAssignmentRecommendCapacityOptions, GoalAssignmentRecommendCapacityCommand.GoalAssignmentRecommendCapacityCommandResult>
{
    public override void ValidateOptions(GoalAssignmentRecommendCapacityOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (options.ResourceIds?.Length > 50)
        {
            validationResult.Errors.Add("At most 50 resource IDs may be assessed per request.");
        }

        GoalAssignmentValidation.ValidateResourceIds(options.ResourceIds, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentRecommendCapacityOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var operation = await service.RecommendGoalAssignmentCapacityAsync(
                options.ServiceGroup, options.GoalAssignment, options.ResourceIds ?? [], options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentRecommendCapacityCommandResult(operation),
                ResilienceManagementJsonContext.Default.GoalAssignmentRecommendCapacityCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error recommending goal capacity. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup, options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GoalAssignmentRecommendCapacityCommandResult(GoalAssignmentOperationResult Operation);
}
