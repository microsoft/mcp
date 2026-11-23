// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Options.Inventory;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;

/// <summary>
/// POC: waits quietly for an inventory operation, keeping the agent's turn open until it completes. Each call checks the operation
/// like the status command, repeating for up to <see cref="WaitLimits.MaxWait"/>, and returns as soon as
/// it completes or the time is up. The agent writes nothing while it waits and replies once with the final result, so no progress
/// or "upload your file" text can reach the chat after the upload panel already shows the file arriving. Once it completes, the
/// agent calls the status tool once to show the inventory summary card and replies once.
/// </summary>
[CommandMetadata(
    Id = "8c3e1f7a-4b2d-4e9a-a6c5-7d1f0b2e9a41",
    Name = "wait",
    Title = "Wait For Azure Catalyst Inventory",
    Description = "Waits quietly, for up to a few minutes per call, for an Azure Catalyst inventory operation started by the " +
        "inventory create-or-update tool. " +
        "When: only when the 'instructions' of the latest inventory result say to call the inventory wait tool. " +
        "How: pass 'session-id' and 'operation-id' from the latest create-or-update result, unchanged. Call it directly yourself, " +
        "one call at a time; never write or run a script or code to call it. " +
        "Returns: the same fields as the inventory status tool: 'status', 'isCompleted', 'instructions' and, when finished, either " +
        "'result' or 'errorCode'/'errorMessage'. " +
        "Next: follow 'instructions': call this tool again while 'isCompleted' is false, without writing to the user; once it is " +
        "true, do what they say. Do not mention tool or parameter names to the user. " + ReplyFormat.ToolRule,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WaitInventoryCommand(GetInventoryStatusCommand statusCommand)
    : BaseCommand<WaitInventoryOptions, GetInventoryStatusCommand.GetInventoryStatusCommandResult>
{
    private readonly GetInventoryStatusCommand _statusCommand = statusCommand;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WaitInventoryOptions options, CancellationToken cancellationToken)
    {
        // One call covers the whole wait (up to WaitLimits.MaxWait), so the host shows a single wait step instead of "(N times)".
        Stopwatch elapsed = Stopwatch.StartNew();
        GetInventoryStatusCommand.GetInventoryStatusCommandResult result;
        do
        {
            result = await _statusCommand.RunAsync(
                DsBackdoorClient.BaseUrl,
                new GetInventoryStatusOptions { SessionId = options.SessionId, OperationId = options.OperationId, NoWait = false },
                cancellationToken);
        }
        while (result.Success && !result.IsCompleted && elapsed.Elapsed + TimeSpan.FromSeconds(GetInventoryStatusCommand.MaxWaitSeconds) <= WaitLimits.MaxWait);

        if (result.Success && result.IsCompleted && InventorySummaryWidget.IsEnabled)
        {
            // The agent shows the summary card with one status call, which returns the summary again; leave it out here.
            result = result with { Result = null, Instructions = GetInventoryStatusCommand.ShowSummaryInstructions };
        }

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.GetInventoryStatusCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }
}
