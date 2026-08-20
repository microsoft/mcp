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
        Updates an existing resilience drill in an Azure service group. Use for requests such as "Update
        resilience drill <drill_name> in service group <service_group> to use manual RBAC setup" and
        "Associate recovery plan <recovery_plan_name> with resilience drill <drill_name> in service group
        <service_group>". Changes the drill's RBAC setup mode, associates or links a recovery plan with the
        drill, or changes the supporting-resource subscription and region together. This tool modifies the drill;
        it does not get drill details or get a recovery plan. Only supplied properties are changed.
        """,
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

        if (options.RecoveryPlan is { } recoveryPlan && (string.IsNullOrWhiteSpace(recoveryPlan) || recoveryPlan.Contains('/')))
        {
            validationResult.Errors.Add("The recovery plan name must be a non-empty single path segment.");
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

        if (string.IsNullOrWhiteSpace(options.Subscription) && options.RbacSetupMode is null && string.IsNullOrWhiteSpace(options.RecoveryPlan))
        {
            validationResult.Errors.Add("Specify at least one property to update: subscription and region, RBAC setup mode, or recovery plan.");
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
                options.RecoveryPlan,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

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
            "The drill, service group, subscription, or recovery plan was not found.",
        RequestFailedException =>
            "The drill update request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillUpdateCommandResult(DrillInfo Drill);
}
