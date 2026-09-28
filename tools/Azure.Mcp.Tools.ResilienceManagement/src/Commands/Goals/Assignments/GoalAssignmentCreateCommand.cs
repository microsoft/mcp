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
    Id = "8f0f263b-c2a4-47ba-93e7-c1ed571f362d",
    Name = "create",
    Title = "Create or Update Resilience Goal Assignment",
    Description = """
        Creates or updates a resilience goal assignment in the specified service group using a legacy goal
        template supported by the current Azure SDK. Waits for provisioning to complete and returns the hydrated
        assignment. The goal template must already exist in the same service group.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalAssignmentCreateCommand(ILogger<GoalAssignmentCreateCommand> logger, IGoalAssignmentCreateService goalAssignmentCreateService)
    : AuthenticatedCommand<GoalAssignmentCreateOptions, GoalAssignmentCreateCommand.GoalAssignmentCreateCommandResult>
{
    private readonly ILogger<GoalAssignmentCreateCommand> _logger = logger;
    private readonly IGoalAssignmentCreateService _goalAssignmentCreateService = goalAssignmentCreateService;

    public override void ValidateOptions(GoalAssignmentCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.GoalAssignment, "goal assignment", validationResult);
        ValidatePathSegment(options.GoalTemplate, "goal template", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            GoalAssignmentInfo goalAssignment = await _goalAssignmentCreateService.CreateGoalAssignmentAsync(
                options.ServiceGroup,
                options.GoalAssignment,
                options.GoalTemplate,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentCreateCommandResult(goalAssignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}, GoalTemplate: {GoalTemplate}.",
                options.ServiceGroup, options.GoalAssignment, options.GoalTemplate);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The goal assignment create or update request timed out. Check the assignment state before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The goal assignment could not be created or updated because it conflicts with the current resource state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating or updating the goal assignment. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The service group or goal template was not found. Verify both resources exist and you have access.",
        RequestFailedException =>
            "The goal assignment request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException
        ? HttpStatusCode.GatewayTimeout
        : base.GetStatusCode(ex);

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    public sealed record GoalAssignmentCreateCommandResult(GoalAssignmentInfo GoalAssignment);
}
