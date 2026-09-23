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
    Id = "74e5bf03-9e8c-447a-b71b-0ac831e90bbc",
    Name = "refresh-resources",
    Title = "Refresh Resilience Goal Assignment Resources",
    Description = "Refreshes or rediscovers resources for an existing resilience goal assignment in a service group. Scans service group membership again to add newly discovered goal resources, update tracked resources, and remove stale goal resources. Returns Accepted and the service operation ID when supplied; rediscovery continues asynchronously. Does not recommend capacity or explicitly include or exclude resources from goals.",
    OperationPlane = ToolOperationPlane.Control, Destructive = true, Idempotent = false,
    OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentRefreshResourcesCommand(
    ILogger<GoalAssignmentRefreshResourcesCommand> logger,
    IResilienceManagementService service)
    : AuthenticatedCommand<GoalAssignmentRefreshResourcesOptions, GoalAssignmentOperationResult>
{
    public override void ValidateOptions(GoalAssignmentRefreshResourcesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        GoalAssignmentResourceValidation.ValidateNames(options.ServiceGroup, options.GoalAssignment, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentRefreshResourcesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.RefreshGoalAssignmentResourcesAsync(options.ServiceGroup, options.GoalAssignment, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.GoalAssignmentOperationResult);
        }
        catch (Exception ex)
        {
            logger.LogError("Goal assignment resource refresh failed.");
            HandleException(context, ex);
            context.Response.Results = null;
        }
        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => GoalAssignmentResourceValidation.GetErrorMessage(ex);
}
