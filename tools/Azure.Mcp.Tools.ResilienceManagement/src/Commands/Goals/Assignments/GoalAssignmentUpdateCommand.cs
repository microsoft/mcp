// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

[CommandMetadata(
    Id = "c7216a25-47b1-4823-83ad-064b5bed786f",
    Name = "update",
    Title = "Update Resilience Goal Assignment",
    Description = "Updates an existing resilience goal assignment in an Azure service group with a service-level indicator and objective resource mapping. The assignment must already exist. Returns the updated assignment after provisioning completes.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalAssignmentUpdateCommand(
    ILogger<GoalAssignmentUpdateCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<GoalAssignmentUpdateOptions, GoalAssignmentUpdateCommand.GoalAssignmentUpdateCommandResult>
{
    private readonly ILogger<GoalAssignmentUpdateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(GoalAssignmentUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.GoalAssignment, "goal assignment", validationResult);
        ValidateResourceId(options.ServiceLevelIndicatorResourceId, "service-level indicator resource", validationResult);
        ValidateResourceId(options.ServiceLevelObjectiveResourceId, "service-level objective resource", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        GoalAssignmentUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            GoalAssignmentInfo goalAssignment = await _resilienceManagementService.UpdateGoalAssignmentAsync(
                options.ServiceGroup,
                options.GoalAssignment,
                options.ServiceLevelIndicatorResourceId,
                options.ServiceLevelObjectiveResourceId,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentUpdateCommandResult(goalAssignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup,
                options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The goal assignment update request timed out. Check the assignment state before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The goal assignment cannot be updated in its current state. Resolve dependent resources or active operations and try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the goal assignment. Verify you have permission to update goal assignments in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The goal assignment, service group, or service-level resource was not found. Verify the resources exist and you have access.",
        RequestFailedException =>
            "The goal assignment update request failed. Verify the assignment, service group, service-level resources, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException
        ? HttpStatusCode.GatewayTimeout
        : base.GetStatusCode(ex);

    private static void ValidatePathSegment(string value, string resourceName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {resourceName} name must be a single non-empty path segment.");
        }
    }

    private static void ValidateResourceId(string value, string resourceName, ValidationResult validationResult)
    {
        if (!ResourceIdentifier.TryParse(value, out ResourceIdentifier? resourceId) || string.IsNullOrEmpty(resourceId?.SubscriptionId))
        {
            validationResult.Errors.Add($"The {resourceName} ID must be a valid Azure resource ID.");
        }
    }

    public sealed record GoalAssignmentUpdateCommandResult(GoalAssignmentInfo GoalAssignment);
}
