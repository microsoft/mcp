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
    Id = "95a7a71c-337e-4e73-ab77-0429fd46faba",
    Name = "refresh-resources",
    Title = "Rediscover Resilience Goal Resources",
    Description = """
        Starts resource rediscovery for an Azure resilience goal assignment using current service-group
        membership. Use after adding or removing service-group resources to refresh the assignment's tracked
        members. Does not change resource inclusion or attestation settings directly; use update-resources
        for those. Returns the service operation ID and Accepted status unless already completed.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentRefreshResourcesCommand(
    ILogger<GoalAssignmentRefreshResourcesCommand> logger,
    IResilienceManagementService service)
    : BaseGoalAssignmentCommand<GoalAssignmentRefreshResourcesOptions, GoalAssignmentRefreshResourcesCommand.GoalAssignmentRefreshResourcesCommandResult>
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentRefreshResourcesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var operation = await service.RefreshGoalAssignmentResourcesAsync(
                options.ServiceGroup, options.GoalAssignment, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentRefreshResourcesCommandResult(operation),
                ResilienceManagementJsonContext.Default.GoalAssignmentRefreshResourcesCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing goal resources. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup, options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GoalAssignmentRefreshResourcesCommandResult(GoalAssignmentOperationResult Operation);
}
