// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

[CommandMetadata(
    Id = "c1f4a82e-6d39-4b57-9e08-2a7c5b9d4f61",
    Name = "get",
    Title = "Get or List Resilience Goal Assignments",
    Description = """
        Gets resilience goal assignments in the specified service group. Provide a goal assignment name to get
        the full details of that assignment (id, name, goal assignment type, goal template id, and provisioning
        state). Omit the name to list all goal assignments in the service group, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalAssignmentGetCommand(ILogger<GoalAssignmentGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<GoalAssignmentGetOptions, GoalAssignmentGetCommand.GoalAssignmentGetCommandResult>
{
    private readonly ILogger<GoalAssignmentGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            GoalAssignmentGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var goalAssignments = await _resilienceManagementService.ListGoalAssignmentsAsync(
                    options.ServiceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new GoalAssignmentGetCommandResult(GoalAssignments: goalAssignments.ToList());
            }
            else
            {
                var goalAssignment = await _resilienceManagementService.GetGoalAssignmentAsync(
                    options.ServiceGroup,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new GoalAssignmentGetCommandResult(GoalAssignment: goalAssignment);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.GoalAssignmentGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting goal assignment(s). ServiceGroup: {ServiceGroup}, Name: {Name}.",
                options.ServiceGroup, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Goal assignment not found. Verify the goal assignment name, service group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the goal assignment. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Goal assignment not found. Verify the goal assignment and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record GoalAssignmentGetCommandResult(List<ResourceSummary>? GoalAssignments = null, GoalAssignmentInfo? GoalAssignment = null);
}
