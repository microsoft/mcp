// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

[CommandMetadata(
    Id = "b8e4d1c6-3a07-4f29-9d85-2c6f0a7b1e94",
    Name = "create",
    Title = "Create or Update Resilience Goal Assignment",
    Description = """
        Creates or updates a resilience goal assignment in the specified service group that assigns the given
        goal template, and returns the goal assignment information including id, name, goal assignment type,
        goal template id, and provisioning state.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalAssignmentCreateCommand(ILogger<GoalAssignmentCreateCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<GoalAssignmentCreateOptions, GoalAssignmentCreateCommand.GoalAssignmentCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<GoalAssignmentCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var goalAssignment = await _resilienceManagementService.CreateGoalAssignmentAsync(
                options.ServiceGroup,
                options.GoalAssignment,
                options.GoalTemplate,
                options.GoalTemplateServiceGroup,
                options.GoalAssignmentType,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentCreateCommandResult(goalAssignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}, GoalTemplate: {GoalTemplate}, Subscription: {Subscription}.",
                options.ServiceGroup, options.GoalAssignment, options.GoalTemplate, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the goal assignment. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Service group or goal template not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record GoalAssignmentCreateCommandResult(GoalAssignmentInfo GoalAssignment);
}
