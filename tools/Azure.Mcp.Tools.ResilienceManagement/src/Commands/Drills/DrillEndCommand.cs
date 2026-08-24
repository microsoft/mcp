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
    Id = "d0c5ad0f-e4e8-423a-92cb-faa0af84a599",
    Name = "end",
    Title = "End Resilience Drill",
    Description = "Ends the currently running execution of a resilience drill and records its Success or Failed attestation. Returns the operation ID for the accepted asynchronous request.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillEndCommand(ILogger<DrillEndCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillEndOptions, DrillEndCommand.DrillEndCommandResult>
{
    private readonly ILogger<DrillEndCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillEndOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        DrillActionValidation.ValidateResourceNames(options.ServiceGroup, options.Drill, validationResult);
        DrillActionValidation.ValidateAttestation(options.Attestation, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillEndOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string operationId = await _resilienceManagementService.EndDrillAsync(
                options.ServiceGroup,
                options.Drill,
                DrillActionValidation.NormalizeAttestation(options.Attestation),
                options.AttestationNotes,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillEndCommandResult(operationId, options.Drill, "Accepted"),
                ResilienceManagementJsonContext.Default.DrillEndCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error ending drill. ServiceGroup: {ServiceGroup}, Drill: {Drill}, Attestation: {Attestation}.",
                options.ServiceGroup, options.Drill, options.Attestation);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill cannot be ended in its current state. Verify a drill execution is active.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed ending the drill. Verify you have permission to run drills in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The drill was not found. Verify the drill and service group exist and you have access.",
        RequestFailedException =>
            "The drill end request failed. Verify the drill, service group, and attestation, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillEndCommandResult(string OperationId, string Drill, string Status);
}