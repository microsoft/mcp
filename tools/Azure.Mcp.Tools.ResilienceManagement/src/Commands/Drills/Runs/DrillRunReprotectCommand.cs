// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "a331e69f-4669-40de-97ad-10a1cebe4b54",
    Name = "reprotect",
    Title = "Reprotect a Resilience Drill Run",
    Description = "Initiates reprotection for failed-over resources in a resilience drill run.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunReprotectCommand(ILogger<DrillRunReprotectCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunReprotectOptions, DrillRunReprotectCommand.DrillRunReprotectCommandResult>
{
    private readonly ILogger<DrillRunReprotectCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunReprotectOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunReprotectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _resilienceManagementService.ReprotectDrillRunAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new DrillRunReprotectCommandResult(options.DrillRun, Accepted: true);
            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunReprotectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error initiating drill run reprotection. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Reprotection cannot start while the drill run is in its current state. Complete the active operation or verify reprotection is available, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed initiating drill run reprotection. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException =>
            "The drill run reprotect request failed. Verify the drill run, drill, and service group, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    public sealed record DrillRunReprotectCommandResult(string DrillRun, bool Accepted);
}
