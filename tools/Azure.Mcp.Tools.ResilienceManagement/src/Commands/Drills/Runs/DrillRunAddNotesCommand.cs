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
    Id = "c1d39a5c-7993-4c29-a81f-6a9abf5a9487",
    Name = "add-notes",
    Title = "Add Notes to a Resilience Drill Run",
    Description = "Adds notes to a run of a resilience drill in an Azure service group.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunAddNotesCommand(ILogger<DrillRunAddNotesCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunAddNotesOptions, DrillRunAddNotesCommand.DrillRunAddNotesCommandResult>
{
    private readonly ILogger<DrillRunAddNotesCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunAddNotesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);

        if (string.IsNullOrWhiteSpace(options.Notes))
        {
            validationResult.Errors.Add("Notes must contain at least one non-whitespace character.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunAddNotesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _resilienceManagementService.AddDrillRunNotesAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                options.Notes,
                options.Tenant,
                cancellationToken);

            var result = new DrillRunAddNotesCommandResult(options.DrillRun, Accepted: true);
            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunAddNotesCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding notes to drill run. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Notes cannot be added to the drill run in its current state. Verify the drill run state and try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed adding notes to the drill run. Verify you have permission to update drill runs in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException =>
            "The add-notes request failed. Verify the drill run, drill, service group, and notes, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    public sealed record DrillRunAddNotesCommandResult(string DrillRun, bool Accepted);
}
