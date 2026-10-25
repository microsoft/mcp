// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "9c4ad84e-7056-46ad-a857-8a81dce0c676",
    Name = "update",
    Title = "Update Resilience Drill",
    Description = """
        Updates an existing resilience drill in a service group. Changes only supplied properties, including
        supporting-resource subscription and region, RBAC setup, managed identity, Chaos, recovery-plan, goal,
        Health Model, SLI, and monitoring configuration. Monitoring-source updates require an explicit existing
        Azure Health Model or service-group-scoped Microsoft.Monitor SLI and exact read identity; the tool
        validates those resources before mutation and verifies the selection in Drill read-back.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillUpdateCommand(ILogger<DrillUpdateCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillUpdateOptions, DrillUpdateCommand.DrillUpdateCommandResult>
{
    private readonly ILogger<DrillUpdateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.Drill) || options.Drill.Contains('/'))
        {
            validationResult.Errors.Add("The drill name must be a non-empty single path segment.");
        }

        if (string.IsNullOrWhiteSpace(options.ServiceGroup) || options.ServiceGroup.Contains('/'))
        {
            validationResult.Errors.Add("The service group name must be a non-empty single path segment.");
        }

        if (options.Subscription is not null && string.IsNullOrWhiteSpace(options.Subscription))
        {
            validationResult.Errors.Add("The subscription must not be empty.");
        }

        if (options.Region is not null && string.IsNullOrWhiteSpace(options.Region))
        {
            validationResult.Errors.Add("The region must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Subscription) != string.IsNullOrWhiteSpace(options.Region))
        {
            validationResult.Errors.Add("Subscription and region must be specified together.");
        }

        if (options.ResourceGroup is not null &&
            (string.IsNullOrWhiteSpace(options.ResourceGroup) || options.ResourceGroup.Contains('/') || string.IsNullOrWhiteSpace(options.Subscription)))
        {
            validationResult.Errors.Add("The resource group must be a single non-empty path segment specified together with subscription and region.");
        }

        try
        {
            _ = DrillConfiguration.Update(options.GoalAssignmentProperties, options.HealthModelMonitoringProperties, options.SliMonitoringProperties,
                options.ChaosResourceProperties, options.RecoveryPlanProperties, options.MonitoringProperties);
            _ = DrillConfiguration.ParseManagedIdentity(options.Identity);
        }
        catch (ArgumentException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }

        DrillCreateCommand.ValidateMonitoringOptions(
            options.MonitoringSourceType,
            options.MonitoringSourceId,
            options.MonitoringIdentityType,
            options.MonitoringUserAssignedIdentityId,
            options.MonitoringSliType,
            validationResult,
            required: false);

        if (string.IsNullOrWhiteSpace(options.Subscription) &&
            options.RbacSetupMode is null &&
            options.GoalAssignmentProperties is null &&
            options.HealthModelMonitoringProperties is null &&
            options.SliMonitoringProperties is null &&
            options.Identity is null &&
            options.ChaosResourceProperties is null &&
            options.RecoveryPlanProperties is null &&
            options.MonitoringProperties is null &&
            options.MonitoringSourceType is null)
        {
            validationResult.Errors.Add("Specify at least one drill property to update.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var drill = await _resilienceManagementService.UpdateDrillAsync(
                options.ServiceGroup,
                options.Drill,
                options.Subscription,
                options.Region,
                options.RbacSetupMode,
                DrillConfiguration.Update(options.GoalAssignmentProperties, options.HealthModelMonitoringProperties, options.SliMonitoringProperties,
                    options.ChaosResourceProperties, options.RecoveryPlanProperties, options.MonitoringProperties),
                DrillConfiguration.ParseManagedIdentity(options.Identity),
                options.ResourceGroup,
                options.Tenant,
                cancellationToken,
                monitoringSource: options.MonitoringSourceType is null
                    ? null
                    : new DrillMonitoringSource(
                        options.MonitoringSourceType.Value,
                        options.MonitoringSourceId!,
                        options.MonitoringIdentityType!.Value,
                        options.MonitoringUserAssignedIdentityId,
                        options.MonitoringSliType));

            context.Response.Results = ResponseResult.Create(
                new DrillUpdateCommandResult(drill),
                ResilienceManagementJsonContext.Default.DrillUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating drill. ServiceGroup: {ServiceGroup}, Drill: {Drill}, Subscription: {Subscription}, Region: {Region}.",
                options.ServiceGroup, options.Drill, options.Subscription, options.Region);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill could not be updated because it conflicts with the current resource state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the drill. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The drill, service group, subscription, or recoveryplan was not found.",
        RequestFailedException =>
            "The drill update request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillUpdateCommandResult(DrillInfo Drill);
}
