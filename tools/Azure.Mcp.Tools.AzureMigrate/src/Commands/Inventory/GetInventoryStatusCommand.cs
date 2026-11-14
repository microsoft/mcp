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
/// POC: returns the current status of an inventory operation with one call to the Discovery Service
/// (GET /mcprestapi/sessions/{id}/operationstatus/{operationId}), forwarding the caller's bearer token unchanged.
/// It does not wait or poll.
/// </summary>
[CommandMetadata(
    Id = "6e866a7d-d20d-4e98-9737-8240b7ad9957",
    Name = "status",
    Title = "Get Azure Migrate Inventory Status",
    Description = "Gets the current status of an Azure Migrate inventory operation started by the inventory create-or-update tool. " +
        "Call this once when the user asks about the progress or result of their inventory; do not loop or wait. Requires 'session-id' " +
        "and 'operation-id' from the latest create-or-update result in the conversation (ask the user to start an inventory if there " +
        "is none). Returns 'status' (for example Running, Succeeded or Failed), 'isCompleted', 'instructions', and when finished either " +
        "'result' or 'errorCode'/'errorMessage'. Follow 'instructions'. Summarise 'result' in plain language without inventing details. " +
        "Do not mention tool or parameter names to the user.",
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

        DsBackdoorClient.DsResponse response = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Get, DsBackdoorClient.OperationStatusUrl(baseUrl, sessionId!, operationId), token, null,
            Guid.NewGuid().ToString(), "get operation status", cancellationToken);
        return Parse(sessionId!, operationId.ToString("D"), response);
    }

    internal static GetInventoryStatusCommandResult Parse(string sessionId, string operationId, DsBackdoorClient.DsResponse response)
    {
        if (response.Status == (int)HttpStatusCode.NotFound)
        {
            return Fail(sessionId, operationId, response.Status,
                "No inventory operation was found for this session and operation ID for the signed-in user.");
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
            Instructions: GetInstructions(status, isCompleted, errorCode));
    }

    internal static string GetInstructions(string status, bool isCompleted, string? errorCode)
    {
        if (!isCompleted)
        {
            return "Tell the user the inventory is still in progress. If they have not uploaded their file yet, remind them to run the " +
                "upload command shared earlier within the upload window. Suggest checking again in about a minute.";
        }

        if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return "Tell the user the inventory is ready and summarise 'result' in plain language without inventing details.";
        }

        return string.Equals(errorCode, "NoFileUploadedForImport", StringComparison.OrdinalIgnoreCase)
            ? "Tell the user the inventory did not complete because the file was not uploaded within the upload window, and offer to start again."
            : "Tell the user the inventory did not complete and quote 'errorMessage'.";
    }

    private static GetInventoryStatusCommandResult Fail(string? sessionId, string? operationId, int? status, string? error) =>
        new(false, error ?? "Unknown error.", status, sessionId, operationId, null, false, null, null, null, null, null);

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
