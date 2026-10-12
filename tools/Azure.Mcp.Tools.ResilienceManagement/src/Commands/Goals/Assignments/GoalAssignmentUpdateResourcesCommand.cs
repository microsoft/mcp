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
    Id = "8a384d69-274b-40eb-ac91-bb8906041e8a",
    Name = "update-resources",
    Title = "Update Resilience Goal Resource Participation",
    Description = """
        Updates tracked resources in an Azure resilience goal assignment: include or exclude resources from
        zonal goals, set or clear manual attestation, or record user confirmation of resilience solutions.
        Accepts a GA JSON resources array with properties.resourceArmId and nested zonalResiliency settings.
        Does not delete underlying Azure resources. Returns the service operation ID and Accepted status
        unless already completed. Attest only resilience that has actually been verified.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class GoalAssignmentUpdateResourcesCommand(
    ILogger<GoalAssignmentUpdateResourcesCommand> logger,
    IResilienceManagementService service)
    : BaseGoalAssignmentCommand<GoalAssignmentUpdateResourcesOptions, GoalAssignmentUpdateResourcesCommand.GoalAssignmentUpdateResourcesCommandResult>
{
    public override void ValidateOptions(GoalAssignmentUpdateResourcesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        try
        {
            _ = GoalAssignmentValidation.CreateResourcesContent(options.Resources);
        }
        catch (ArgumentException ex)
        {
            validationResult.AddError(ex.Message, "Invalid goal resource update.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalAssignmentUpdateResourcesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var content = GoalAssignmentValidation.CreateResourcesContent(options.Resources);
            var operation = await service.UpdateGoalAssignmentResourcesAsync(
                options.ServiceGroup, options.GoalAssignment, content, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new GoalAssignmentUpdateResourcesCommandResult(operation),
                ResilienceManagementJsonContext.Default.GoalAssignmentUpdateResourcesCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating goal resources. ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}.",
                options.ServiceGroup, options.GoalAssignment);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GoalAssignmentUpdateResourcesCommandResult(GoalAssignmentOperationResult Operation);
}
