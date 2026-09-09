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
    Id = "2458b55c-e911-4d4e-91bc-9003a7cbefb0",
    Name = "create",
    Title = "Create or Update Resilience Drill",
    Description = """
        Creates, configures, or updates a resilience drill in an Azure service group. Use this tool when asked
        to create or provision a resilience drill, including when required drill details must first be clarified.
        Supports zonal and regional drills, system-assigned managed identity, RBAC setup, and an optional recovery
        plan in the same service group. Returns the created drill definition and provisioning state.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
// Drills are service-group-scoped; subscription identifies supporting resources rather than the ARM command scope.
public sealed class DrillCreateCommand(ILogger<DrillCreateCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillCreateOptions, DrillCreateCommand.DrillCreateCommandResult>
{
    private readonly ILogger<DrillCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.Drill.Contains('/'))
        {
            validationResult.Errors.Add("The drill name must be a single path segment.");
        }

        if (options.ServiceGroup.Contains('/'))
        {
            validationResult.Errors.Add("The service group name must be a single path segment.");
        }

        if (options.ResourceGroup is { } resourceGroup && resourceGroup.Contains('/'))
        {
            validationResult.Errors.Add("The resource group name must be a single path segment.");
        }

        if (options.RecoveryPlan is { } recoveryPlan && recoveryPlan.Contains('/'))
        {
            validationResult.Errors.Add("The recoveryplan name must be a single path segment.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var drill = await _resilienceManagementService.CreateDrillAsync(
                options.ServiceGroup,
                options.Drill,
                options.Subscription,
                options.Region,
                options.ResourceGroup,
                options.DrillType,
                options.RbacSetupMode,
                options.RecoveryPlan,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillCreateCommandResult(drill),
                ResilienceManagementJsonContext.Default.DrillCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating drill. ServiceGroup: {ServiceGroup}, Drill: {Drill}, Subscription: {Subscription}, Region: {Region}.",
                options.ServiceGroup, options.Drill, options.Subscription, options.Region);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill could not be created or updated because it conflicts with the current resource state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating or updating the drill. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The service group, subscription, resource group, or recoveryplan was not found.",
        RequestFailedException =>
            "The drill request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillCreateCommandResult(DrillInfo Drill);
}
