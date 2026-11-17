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
[CommandMetadata(
    Id = "6e866a7d-d20d-4e98-9737-8240b7ad9957",
    Name = "status",
    Title = "Get Azure Migrate Inventory Status",
    Description = "Tracks an Azure Migrate inventory operation (file upload and transformation, or a text-only update) started by " +
        "the inventory create-or-update tool. " +
        "When: right after create-or-update succeeds, and again while the operation has not completed. " +
        "How: pass 'session-id' and 'operation-id' from the latest create-or-update result in the conversation; ask the user to " +
        "start an inventory if there is none. " +
        "Returns: 'status' (for example Running, Succeeded or Failed), 'isCompleted', 'instructions' and, when finished, either " +
        "'result' or 'errorCode'/'errorMessage'. Each call waits up to about 25 seconds for the operation to finish. " +
        "Next: while 'isCompleted' is false, call it again quietly with the same 'session-id' and 'operation-id'; the inventory " +
        "must be complete before it is exported or assessed. When it is done, summarise 'result' in plain language without " +
        "inventing details. " +
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
    private static readonly string[] s_terminalStatuses = ["Succeeded", "Failed", "Canceled", "Cancelled"];

    // Re-checks within one call while the operation runs: 2, 4, 8 and 11 seconds apart (about 25 seconds in total).
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

    private async Task<GetInventoryStatusCommandResult> RunAsync(string baseUrl, GetInventoryStatusOptions options, CancellationToken cancellationToken)
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
                    ? "Tell the user this inventory could not be found for them and offer to start a new inventory with their file."
                    : null);
        }

        if (response.Status is not (>= 200 and < 300))
        {
            return Fail(sessionId, operationId, response.Status, response.Error);
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
            return inventory
                ? "If you have not already said so, tell the user once that you are waiting for their upload or processing their " +
                    "inventory; do not repeat it on later checks. Then check the status again quietly with the same sessionId and " +
                    "operationId, and keep checking until isCompleted is true before exporting the inventory or creating an assessment."
                : $"Tell the user the {subject} is still in progress. If they have not uploaded their file yet, remind them to run the " +
                    "upload command shared earlier within the upload window. Suggest checking again in about a minute.";
        }

        if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return $"Tell the user the {subject} is ready and summarise 'result' in plain language without inventing details." +
                (inventory ? " Then offer to export the inventory or create an assessment." : string.Empty);
        }

        if (string.Equals(errorCode, "NoFileUploadedForImport", StringComparison.OrdinalIgnoreCase))
        {
            return $"Tell the user the {subject} did not complete because the file was not uploaded within the upload window, and offer to start again.";
        }

        return $"Tell the user the {subject} did not complete and quote 'errorMessage'." +
            (inventory ? " Offer to start again." : string.Empty);
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
