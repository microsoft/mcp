// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "7de91784-03ba-47e9-8cac-73172828a382",
    Name = "start",
    Title = "Start Resilience Drill",
    Description = "Starts, runs, or executes a resilience drill in Failover or TestFailover mode. Use this command to begin a new drill execution, not to get or list drill definitions. Returns the operation ID for the accepted asynchronous request.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillStartCommand(ILogger<DrillStartCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillStartOptions, DrillStartCommand.DrillStartCommandResult>
{
    private readonly ILogger<DrillStartCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillStartOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        DrillActionValidation.ValidateResourceNames(options.ServiceGroup, options.Drill, validationResult);
        DrillActionValidation.ValidateMode(options.Mode, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillStartOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string operationId = await _resilienceManagementService.StartDrillAsync(
                options.ServiceGroup,
                options.Drill,
                DrillActionValidation.NormalizeMode(options.Mode),
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillStartCommandResult(operationId, options.Drill, "Accepted"),
                ResilienceManagementJsonContext.Default.DrillStartCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting drill. ServiceGroup: {ServiceGroup}, Drill: {Drill}, Mode: {Mode}.",
                options.ServiceGroup, options.Drill, options.Mode);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill cannot be started in its current state. Verify no drill execution is already active.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed starting the drill. Verify you have permission to run drills in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The drill was not found. Verify the drill and service group exist and you have access.",
        RequestFailedException =>
            "The drill start request failed. Verify the drill, service group, and mode, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillStartCommandResult(string OperationId, string Drill, string Status);
}