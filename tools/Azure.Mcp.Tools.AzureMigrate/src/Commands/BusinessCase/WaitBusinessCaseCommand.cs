// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// POC: waits quietly for a business case, keeping the agent's turn open until it finishes. Each call checks the business case like
/// the status command, repeating for up to <see cref="MaxWait"/>, and returns as soon as it finishes or the time is up. It has no card,
/// so the agent can call it repeatedly without adding cards to the conversation; once the business case succeeds, the agent calls the
/// status tool once to show the ROI report card and replies once with the ROI summary.
/// </summary>
[CommandMetadata(
    Id = "5b0f3c2e-8d1a-4f6b-9c7e-2a4d6e8f1b3c",
    Name = "wait",
    Title = "Wait For Azure Catalyst Business Case",
    Description = "Waits quietly, for up to a few minutes per call, for an Azure Catalyst business case started by the business " +
        "case create tool, moving it to its next stage. " +
        "When: only when the 'instructions' of the latest business case result say to call the business case wait tool. " +
        "How: pass exactly the values in 'nextStatusParameters' of the latest business case result, unchanged and without inventing " +
        "any. Call it directly yourself, one call at a time; never write or run a script or code to call it. " +
        "Returns: the same fields as the business case status tool: 'status', 'stage', 'isCompleted', 'nextStatusParameters' while it " +
        "runs, 'result' and 'report' once it succeeded, and 'instructions'. " +
        "Next: follow 'instructions': call this tool again while it runs, without writing to the user; once 'isCompleted' is true, " +
        "do what they say. Do not mention tool or parameter names to the user. " + ReplyFormat.ToolRule,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WaitBusinessCaseCommand(GetBusinessCaseStatusCommand statusCommand)
    : BaseCommand<GetBusinessCaseStatusOptions, BusinessCaseCommandResult>
{
    /// <summary>Longest time one call waits, below the time a client allows a tool call.</summary>
    internal static TimeSpan MaxWait => WaitLimits.MaxWait;

    /// <summary>Time between two checks within one call.</summary>
    internal static TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(8);

    private readonly GetBusinessCaseStatusCommand _statusCommand = statusCommand;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GetBusinessCaseStatusOptions options, CancellationToken cancellationToken)
    {
        BusinessCaseCommandResult result = await RunAsync(options, MaxWait, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.BusinessCaseCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<BusinessCaseCommandResult> RunAsync(GetBusinessCaseStatusOptions options, TimeSpan maxWait, CancellationToken cancellationToken)
    {
        Stopwatch elapsed = Stopwatch.StartNew();
        while (true)
        {
            BusinessCaseCommandResult result = await _statusCommand.RunAsync(options, cancellationToken);
            if (result.Success && result.Status == BusinessCaseStages.Succeeded && BusinessCaseReportWidget.IsEnabled)
            {
                // The agent shows the report card with one status call, which returns the report again; leave it out here.
                return result with { Report = null, Artifacts = null, Instructions = BusinessCaseStages.ShowReportInstructions };
            }

            if (!result.Success || result.IsCompleted || result.NextStatusParameters is null)
            {
                return result;
            }

            TimeSpan remaining = maxWait - elapsed.Elapsed;
            if (remaining <= CheckInterval)
            {
                return result with { Instructions = BusinessCaseStages.WaitAgainInstructions };
            }

            // Carry the final stage's session and operation into the next check once it has started.
            options.DsSessionId = result.NextStatusParameters.DsSessionId;
            options.DsOperationId = result.NextStatusParameters.DsOperationId;
            await Task.Delay(CheckInterval, cancellationToken);
        }
    }
}
