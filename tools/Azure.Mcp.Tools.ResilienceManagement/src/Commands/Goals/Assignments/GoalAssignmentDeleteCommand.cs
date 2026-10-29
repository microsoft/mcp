// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

[CommandMetadata(
    Id = "24572f80-b3d5-4877-a6e6-01c21c44fda8",
    Name = "delete",
    Title = "Delete Resilience Goal Assignment",
    Description = """
        Deletes (removes) a resilience goal assignment from its service group and waits for deletion to
        finish. Reports whether the assignment existed: deleted=true when it was removed, deleted=false when it was
        already absent. Removes the assignment and its tracked goal members, not the underlying Azure resources or
        the service group.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentDeleteCommand(
    ILogger<GoalAssignmentDeleteCommand> logger,
    IResilienceManagementService service)
    : BaseGoalAssignmentCommand<GoalAssignmentDeleteOptions, GoalAssignmentDeleteCommand.GoalAssignmentDeleteCommandResult>
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await service.DeleteGoalAssignmentAsync(
                options.ServiceGroup, options.GoalAssignment, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentDeleteCommandResult(options.ServiceGroup, options.GoalAssignment, deleted),
                ResilienceManagementJsonContext.Default.GoalAssignmentDeleteCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup, options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GoalAssignmentDeleteCommandResult(
        string ServiceGroup, string GoalAssignment,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] bool Deleted);
}
