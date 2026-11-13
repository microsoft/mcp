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
        Creates or updates a resilience goal assignment with explicit plan-type intent. Regional plans must send
        require-zonal-resiliency=false and require-regional-resiliency=true with both regional recovery objectives.
        Zonal plans must send the inverse flags. Waits for provisioning and returns the authoritative assignment.
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
        if (options.RequireZonalResiliency == options.RequireRegionalResiliency)
        {
            validationResult.Errors.Add(
                "Exactly one of require-zonal-resiliency and require-regional-resiliency must be true.");
        }

        if (options.RequireRegionalResiliency &&
            (string.IsNullOrWhiteSpace(options.RegionalRecoveryPointObjective) ||
             string.IsNullOrWhiteSpace(options.RegionalRecoveryTimeObjective)))
        {
            validationResult.Errors.Add(
                "Regional goals require regional-recovery-point-objective and regional-recovery-time-objective.");
        }

        if (!options.RequireRegionalResiliency &&
            (options.RegionalRecoveryPointObjective is not null ||
             options.RegionalRecoveryTimeObjective is not null))
        {
            validationResult.Errors.Add(
                "Regional recovery objectives are valid only when require-regional-resiliency is true.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            GoalAssignmentInfo goalAssignment = await _goalAssignmentCreateService.CreateGoalAssignmentAsync(
                options.ServiceGroup,
                options.GoalAssignment,
                options.RequireZonalResiliency,
                options.RequireRegionalResiliency,
                options.RegionalRecoveryPointObjective,
                options.RegionalRecoveryTimeObjective,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentCreateCommandResult(goalAssignment),
                ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating goal assignment. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}, RequireZonalResiliency: {RequireZonalResiliency}, RequireRegionalResiliency: {RequireRegionalResiliency}.",
                options.ServiceGroup,
                options.GoalAssignment,
                options.RequireZonalResiliency,
                options.RequireRegionalResiliency);
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
            "The service group was not found. Verify it exists and you have access.",
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
