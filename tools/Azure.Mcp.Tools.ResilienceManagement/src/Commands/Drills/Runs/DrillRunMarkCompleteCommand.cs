// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "b7e2c9a4-3d51-4c8e-9f6a-1e0d2b7c5a48",
    Name = "mark-complete",
    Title = "Mark a Resilience Drill Run Stage Complete",
    Description = """
        Marks a stage of a resilience drill run as complete, disabling further retries on that stage so the drill run can
        proceed. Provide the drill run stage to complete (for example FaultInjection). It starts the operation and returns
        the operation ID.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunMarkCompleteCommand(ILogger<DrillRunMarkCompleteCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunMarkCompleteOptions, DrillRunMarkCompleteCommand.DrillRunMarkCompleteCommandResult>
{
    private static readonly string[] AllowedStages =
    {
        "FaultInjection", "Failover", "Reprotect", "FailoverReverse", "ReprotectReverse"
    };

    private readonly ILogger<DrillRunMarkCompleteCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunMarkCompleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "--service-group", validationResult);
        ValidatePathSegment(options.Drill, "--drill", validationResult);
        ValidatePathSegment(options.DrillRun, "--drill-run", validationResult);

        if (!TryResolveStage(options.Stage, out _))
        {
            validationResult.Errors.Add(
                $"--stage must be one of: {string.Join(", ", AllowedStages)}.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunMarkCompleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _ = TryResolveStage(options.Stage, out string stage);
            DrillRunMarkCompleteResult result = await _resilienceManagementService.MarkDrillRunCompleteAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                stage,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillRunMarkCompleteCommandResult(result),
                ResilienceManagementJsonContext.Default.DrillRunMarkCompleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error marking drill run stage complete. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}, Stage: {Stage}.",
                options.ServiceGroup, options.Drill, options.DrillRun, options.Stage);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool TryResolveStage(string? value, out string stage)
    {
        stage = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (string allowed in AllowedStages)
        {
            if (allowed.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                stage = allowed;
                return true;
            }
        }

        return false;
    }

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/'))
        {
            validationResult.Errors.Add($"{optionName} must be a single non-empty path segment.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill run stage cannot be marked complete in its current state. Verify the stage is active, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed marking the drill run stage complete. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException =>
            "The drill run mark complete request failed. Verify the drill run, drill, service group, and stage, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillRunMarkCompleteCommandResult(DrillRunMarkCompleteResult Result);
}
