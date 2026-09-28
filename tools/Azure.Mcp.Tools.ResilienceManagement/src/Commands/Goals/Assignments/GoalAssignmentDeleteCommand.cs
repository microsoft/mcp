// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

[CommandMetadata(
    Id = "12961012-071d-44e3-86ad-fbb6787dcb61",
    Name = "delete",
    Title = "Delete Resilience Goal Assignment",
    Description = "Deletes a resilience goal assignment from an Azure service group. This permanently removes the goal assignment configuration and cannot be undone. Use this only when the goal assignment is no longer needed.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalAssignmentDeleteCommand(
    ILogger<GoalAssignmentDeleteCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<GoalAssignmentDeleteOptions, GoalAssignmentDeleteCommand.GoalAssignmentDeleteCommandResult>
{
    private readonly ILogger<GoalAssignmentDeleteCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(GoalAssignmentDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.GoalAssignment, "goal assignment", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        GoalAssignmentDeleteOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await _resilienceManagementService.DeleteGoalAssignmentAsync(
                options.ServiceGroup,
                options.GoalAssignment,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentDeleteCommandResult(deleted, options.GoalAssignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup,
                options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The goal assignment delete request timed out. Check whether the goal assignment still exists before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The goal assignment cannot be deleted in its current state. Resolve dependent resources or active operations and try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed deleting the goal assignment. Verify you have permission to delete goal assignments in the service group.",
        RequestFailedException =>
            "The goal assignment delete request failed. Verify the goal assignment, service group, and request parameters, then try again.",
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

    public sealed record GoalAssignmentDeleteCommandResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] bool Deleted,
        string GoalAssignment);
}
