// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Options.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;

/// <summary>
/// POC: tracks an inventory operation (upload and transformation) on the Discovery Service
/// (GET /mcprestapi/sessions/{id}/operationstatus/{operationId}), forwarding the caller's bearer token unchanged.
/// Each call waits up to about <see cref="MaxWaitSeconds"/> seconds for the operation to complete before returning,
/// so the agent can call it repeatedly with the same session and operation until it completes.
/// </summary>
/// <remarks>
/// The description's "When" and "Next" are what keep the agent tracking. The server cannot call tools itself, so the agent calls
/// this again after each result whose 'isCompleted' is false. Each call waits server-side (see <see cref="s_pollDelays"/>), which
/// spaces the checks without a client timer.
/// </remarks>
[CommandMetadata(
    Id = CommandId,
    Name = "status",
    Title = "Get Azure Catalyst Inventory Status",
    Description = "Tracks an Azure Catalyst inventory operation (file upload and transformation, or a text-only update) started by " +
        "the inventory create-or-update tool. " +
        "When: right after create-or-update succeeds, and again while the operation has not completed. " +
        "How: pass 'session-id' and 'operation-id' from the latest create-or-update result in the conversation; ask the user to " +
        "start an inventory if there is none. " +
        "Returns: 'status' (for example Running, Succeeded or Failed), 'isCompleted', 'instructions' and, when finished, either " +
        "'result' or 'errorCode'/'errorMessage'. Each call waits up to about 25 seconds for the operation to finish. " +
        "Next: while 'isCompleted' is false, call it again quietly with the same 'session-id' and 'operation-id'; the inventory " +
        "must be complete before it is exported or used for a business case. Call it directly yourself, one call at a time; never " +
        "write or run a script or code to call it. When it is done, reply in the format 'instructions' give. " +
        "Follow 'instructions'. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GetInventoryStatusCommand(ILogger<GetInventoryStatusCommand> logger, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<GetInventoryStatusOptions, GetInventoryStatusCommand.GetInventoryStatusCommandResult>
{
    internal const string CommandId = "6e866a7d-d20d-4e98-9737-8240b7ad9957";

    /// <summary>
    /// Instructions the wait tool returns once the inventory completes: the agent calls the status tool once, which shows the
    /// inventory summary card, and replies once with that result's instructions.
    /// </summary>
    internal const string ShowSummaryInstructions =
        "The inventory has completed. Write nothing yet: call the inventory status tool once with the same sessionId and " +
        "operationId (this shows the inventory summary card), then reply once, following that result's instructions.";

    private static readonly string[] s_terminalStatuses = ["Succeeded", "Failed", "Canceled", "Cancelled"];

    // Re-checks within one call while the operation runs: 2, 4, 8 and 11 seconds apart (about 25 seconds in total).
    // Why a long poll: the agent has no timer and calls again only after a result, so a short call would cause many round trips
    // and chat noise. 25 seconds keeps each call under typical host tool-call timeouts (the Cowork widget proxy cuts off at about
    // 27 seconds). The short first gaps catch fast text-only updates; the call returns as soon as the operation completes. On
    // OneBox a five-check call took about 35 seconds, because each check itself takes a few seconds.
    internal static readonly TimeSpan[] s_pollDelays =
        [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(11)];

    internal const int MaxWaitSeconds = 25;

    private readonly ILogger<GetInventoryStatusCommand> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GetInventoryStatusOptions options, CancellationToken cancellationToken)
    {
        GetInventoryStatusCommandResult result = await RunAsync(DsBackdoorClient.BaseUrl, options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.GetInventoryStatusCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<GetInventoryStatusCommandResult> RunAsync(string baseUrl, GetInventoryStatusOptions options, CancellationToken cancellationToken)
    {
        string? sessionId = options.SessionId?.Trim();
        string? operationIdText = options.OperationId?.Trim();
        if (!DsBackdoorClient.IsValidSessionId(sessionId))
        {
            return Fail(sessionId, operationIdText, null, "'session-id' is required: 1-20 characters using only letters, digits, '-' or '_'.");
        }

        if (!Guid.TryParse(operationIdText, out Guid operationId))
        {
            return Fail(sessionId, operationIdText, null, "'operation-id' is required and must be a GUID.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(sessionId, operationIdText, null, authError);
        }

        string url = DsBackdoorClient.OperationStatusUrl(baseUrl, sessionId!, operationId);
        if (options.NoWait == true)
        {
            // The upload panel calls this tool through the host, which cuts off widget tools/call after about 27 seconds, so the
            // panel asks for a single check (no wait). The panel only displays progress; the agent's waiting calls do the tracking.
            return Parse(sessionId!, operationId.ToString("D"),
                await DsBackdoorClient.SendAsync(_logger, HttpMethod.Get, url, token, null, Guid.NewGuid().ToString(), "get operation status", cancellationToken));
        }

        return await WaitForCompletionAsync(
            sessionId!,
            operationId.ToString("D"),
            ct => DsBackdoorClient.SendAsync(_logger, HttpMethod.Get, url, token, null, Guid.NewGuid().ToString(), "get operation status", ct),
            (delay, ct) => Task.Delay(delay, ct),
            cancellationToken);
    }

    /// <summary>
    /// Checks the operation status and, while it is still running, checks again after each of <see cref="s_pollDelays"/>.
    /// A missing operation or a failure on the first check is returned straight away; later transient failures are retried.
    /// </summary>
    internal static async Task<GetInventoryStatusCommandResult> WaitForCompletionAsync(
        string sessionId,
        string operationId,
        Func<CancellationToken, Task<DsBackdoorClient.DsResponse>> getStatusAsync,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
        CancellationToken cancellationToken)
    {
        GetInventoryStatusCommandResult result = Parse(sessionId, operationId, await getStatusAsync(cancellationToken));
        foreach (TimeSpan delay in s_pollDelays)
        {
            if (!result.Success || result.IsCompleted)
            {
                break;
            }

            await delayAsync(delay, cancellationToken);
            GetInventoryStatusCommandResult polled = Parse(sessionId, operationId, await getStatusAsync(cancellationToken));
            if (polled.Success || polled.HttpStatus == (int)HttpStatusCode.NotFound)
            {
                result = polled;
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a Discovery Service operation status response. <paramref name="subject"/> names what the session runs (for example
    /// 'inventory' or 'HA assessment') in the error and instructions.
    /// </summary>
    internal static GetInventoryStatusCommandResult Parse(
        string sessionId, string operationId, DsBackdoorClient.DsResponse response, string subject = InventorySubject)
    {
        if (response.Status == (int)HttpStatusCode.NotFound)
        {
            return Fail(sessionId, operationId, response.Status,
                $"No {subject} operation was found for this session and operation ID for the signed-in user.",
                subject == InventorySubject
                    ? ReplyFormat.Failure("Inventory not found.", "offer to start a new inventory with their file.")
                    : null);
        }

        if (response.Status is not (>= 200 and < 300))
        {
            return Fail(sessionId, operationId, response.Status, response.Error,
                subject == InventorySubject
                    ? ReplyFormat.Failure("The inventory status could not be read.", "offer to check again; if it keeps failing, try later.")
                    : null);
        }

        string? status = DsBackdoorClient.ReadValue(response.Body, "status");
        if (string.IsNullOrWhiteSpace(status))
        {
            return Fail(sessionId, operationId, response.Status, "The Discovery Service response did not include a status.");
        }

        bool isCompleted = s_terminalStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        string? errorCode = DsBackdoorClient.ReadValue(response.Body, "error", "code");
        return new(
            Success: true,
            Error: null,
            HttpStatus: response.Status,
            SessionId: sessionId,
            OperationId: operationId,
            Status: status,
            IsCompleted: isCompleted,
            Result: DsBackdoorClient.ReadValue(response.Body, "properties", "result"),
            ErrorCode: errorCode,
            ErrorMessage: DsBackdoorClient.ReadValue(response.Body, "error", "message"),
            RetryAfterSeconds: isCompleted ? null : response.RetryAfterSeconds ?? 30,
            Instructions: GetInstructions(status, isCompleted, errorCode, subject));
    }

    internal static string GetInstructions(string status, bool isCompleted, string? errorCode, string subject = InventorySubject)
    {
        bool inventory = subject == InventorySubject;
        if (!isCompleted)
        {
            // "Quietly": the agent calls this repeatedly in one reply, and Running covers both waiting for the upload and
            // processing it, so any progress text can contradict the upload panel; the agent replies only once it completes.
            return inventory
                ? "Do not write anything to the user yet, and never tell them to upload their file or that you are waiting for it: " +
                    "this status cannot tell whether the file has arrived. Call the inventory wait tool again quietly with the same " +
                    "sessionId and operationId, and keep calling it until isCompleted is true before exporting the inventory or creating " +
                    "a business case; give your explanation only in the final reply. If you must stop before it completes, reply only: " +
                    "'Your inventory isn't ready yet. Ask me for its status in a minute.' Call the wait tool directly yourself; never " +
                    "write or run a script or code to call it."
                : $"Tell the user the {subject} is still in progress. If they have not uploaded their file yet, remind them to run the " +
                    "upload command shared earlier within the upload window. Suggest checking again in about a minute.";
        }

        if (!inventory)
        {
            if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                return $"Tell the user the {subject} is ready and summarise 'result' in plain language without inventing details.";
            }

            return string.Equals(errorCode, "NoFileUploadedForImport", StringComparison.OrdinalIgnoreCase)
                ? $"Tell the user the {subject} did not complete because the file was not uploaded within the upload window, and offer to start again."
                : $"Tell the user the {subject} did not complete and quote 'errorMessage'.";
        }

        if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return ReplyFormat.Reply(
                "'Inventory ready:' followed by the server count, with the OS split and the total storage when 'result' gives them, for " +
                "example 'Inventory ready: 312 servers (280 Windows, 32 Linux), 4.1 TB storage'. If this was a text change or a " +
                "question, the headline confirms the change or answers the question instead.",
                "the most useful exact figures from 'result' (for example cores, memory, databases or applications) and any " +
                "issues found, such as missing or invalid data, with counts.",
                "when there are issues the user can fix, say how (correct the file and upload it again, or describe the fix in " +
                "text) and that the business case can still be created; otherwise " + ReplyFormat.AfterInventory) +
                (InventorySummaryWidget.IsEnabled
                    ? " The full inventory summary is shown in the card above, so keep the reply short and do not paste 'result'."
                    : string.Empty);
        }

        if (string.Equals(errorCode, "NoFileUploadedForImport", StringComparison.OrdinalIgnoreCase))
        {
            return ReplyFormat.Failure("The inventory did not complete: the file was not uploaded within the upload window.",
                "offer to start again and upload the file within the window.");
        }

        return ReplyFormat.Failure("The inventory did not complete.",
            "say how to fix the cause (for example correct the file and upload it again), then offer to start again.");
    }

    internal const string InventorySubject = "inventory";

    private static GetInventoryStatusCommandResult Fail(string? sessionId, string? operationId, int? status, string? error, string? instructions = null) =>
        new(false, error ?? "Unknown error.", status, sessionId, operationId, null, false, null, null, null, null, instructions);

    /// <summary>Result of the inventory status command.</summary>
    public sealed record GetInventoryStatusCommandResult(
        bool Success,
        string? Error,
        int? HttpStatus,
        string? SessionId,
        string? OperationId,
        string? Status,
        bool IsCompleted,
        string? Result,
        string? ErrorCode,
        string? ErrorMessage,
        int? RetryAfterSeconds,
        string? Instructions);
}
