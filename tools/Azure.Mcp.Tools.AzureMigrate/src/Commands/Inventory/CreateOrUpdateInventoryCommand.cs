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
/// POC: creates or updates an Azure Migrate inventory for the signed-in user. It creates (or reuses) a user-owned
/// session (PUT /mcprestapi/sessions/{id}) and invokes it (POST /mcprestapi/sessions/{id}/invoke) on the Discovery
/// Service, forwarding the caller's bearer token unchanged. It does not upload or poll: it returns an azcopy
/// command the user runs on their own machine to upload the file, and the IDs to check progress with the status tool.
/// </summary>
[CommandMetadata(
    Id = "8f2d4b6a-1c3e-4a5f-9b7d-0e2c4a6b8d1f",
    Name = "createorupdate",
    Title = "Create or Update Azure Migrate Inventory",
    Description = "Creates or updates an Azure Migrate server inventory for the signed-in user from a file on the user's machine " +
        "(for example a CMDB, CSV, Excel or zip export). Call this once, straight away, when the user asks to create, update, build, " +
        "import or upload an inventory. Do not ask the user to attach or send the file in the chat first: the user uploads it from " +
        "their own machine with the returned command. Pass 'session-id' only to update an inventory created earlier in the " +
        "conversation. Returns 'sessionId', 'operationId', 'uploadCommand' (an azcopy command) and 'instructions'. Follow " +
        "'instructions' and show 'uploadCommand' to the user exactly as returned, in a code block, without retyping or shortening it. " +
        "This tool does not upload the file or wait for the result: use the inventory status tool with 'sessionId' and 'operationId'. " +
        "Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CreateOrUpdateInventoryCommand(ILogger<CreateOrUpdateInventoryCommand> logger, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateOrUpdateInventoryOptions, CreateOrUpdateInventoryCommand.CreateOrUpdateInventoryCommandResult>
{
    internal const string FilePathPlaceholder = "<path to your file>";

    // Mirrors the Discovery Service Discovery skill configuration (maxUploadWaitTimeInMinutes, maxInputBlobSizeInBytes):
    // the operation fails if no file arrives within this window.
    internal const int UploadWindowMinutes = 30;
    internal const int MaxFileSizeMegabytes = 500;

    private readonly ILogger<CreateOrUpdateInventoryCommand> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreateOrUpdateInventoryOptions options, CancellationToken cancellationToken)
    {
        CreateOrUpdateInventoryCommandResult result = await RunAsync(DsBackdoorClient.BaseUrl, DsBackdoorClient.SkillType, options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.CreateOrUpdateInventoryCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    private async Task<CreateOrUpdateInventoryCommandResult> RunAsync(string baseUrl, string skillType, CreateOrUpdateInventoryOptions options, CancellationToken cancellationToken)
    {
        string? requestedSessionId = string.IsNullOrWhiteSpace(options.SessionId) ? null : options.SessionId.Trim();
        if (requestedSessionId is not null && !DsBackdoorClient.IsValidSessionId(requestedSessionId))
        {
            return Fail("validation", requestedSessionId, null, null,
                "'session-id' must be 1-20 characters using only letters, digits, '-' or '_'.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail("auth", requestedSessionId, null, null, authError);
        }

        string sessionId = requestedSessionId ?? DsBackdoorClient.NewSessionId();
        string clientRequestId = Guid.NewGuid().ToString();
        bool reusedSession = false;

        // Step 1: create the user-owned session, or confirm the requested one exists (HTTP 200).
        string createBody = DsBackdoorClient.WriteJson(w =>
        {
            w.WriteStartObject("properties");
            w.WriteString("skillType", skillType);
            w.WriteEndObject();
        });
        DsBackdoorClient.DsResponse create = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Put, DsBackdoorClient.SessionUrl(baseUrl, sessionId), token, createBody, clientRequestId, "create session", cancellationToken);
        if (create.Status == (int)HttpStatusCode.Conflict && DsBackdoorClient.ReadActiveSessionId(create.Body) is { } activeSessionId)
        {
            if (requestedSessionId is not null && !string.Equals(activeSessionId, requestedSessionId, StringComparison.OrdinalIgnoreCase))
            {
                return Fail("createSession", requestedSessionId, clientRequestId, create.Status,
                    $"The user already has a different active inventory session '{activeSessionId}'. Update that session instead.");
            }

            // The Discovery Service allows one active session per user and skill type and has no close API yet,
            // so a new inventory reuses the caller's active session.
            sessionId = activeSessionId;
            reusedSession = true;
        }
        else if (create.Status is not (>= 200 and < 300))
        {
            return Fail("createSession", sessionId, clientRequestId, create.Status, create.Error);
        }
        else
        {
            reusedSession = create.Status == (int)HttpStatusCode.OK;
        }

        // Step 2: invoke it with blob input; the response carries the write-only upload SAS URL.
        string invokeBody = string.IsNullOrWhiteSpace(options.Message)
            ? "{}"
            : DsBackdoorClient.WriteJson(w => w.WriteString("message", options.Message.Trim()));
        DsBackdoorClient.DsResponse invoke = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Post, DsBackdoorClient.InvokeUrl(baseUrl, sessionId), token, invokeBody, clientRequestId, "invoke session", cancellationToken);
        if (invoke.Status is not (>= 200 and < 300))
        {
            return Fail("invokeSession", sessionId, clientRequestId, invoke.Status, invoke.Error);
        }

        string? operationId = DsBackdoorClient.ReadValue(invoke.Body, "operationId");
        string? uploadUrl = DsBackdoorClient.ReadValue(invoke.Body, "uri");
        string? uploadCommand = BuildUploadCommand(uploadUrl);
        if (uploadCommand is null)
        {
            return Fail("invokeSession", sessionId, clientRequestId, invoke.Status, "The Discovery Service did not return a valid upload URL.");
        }

        _logger.LogInformation(
            "AzureMigrate inventory: session {SessionId} (reused {Reused}) invoked (operation {OperationId}, request {RequestId}).",
            sessionId, reusedSession, operationId, clientRequestId);

        return new(
            Success: true,
            Error: null,
            FailedStep: null,
            HttpStatus: invoke.Status,
            SessionId: sessionId,
            ReusedExistingSession: reusedSession,
            OperationId: operationId,
            ClientRequestId: clientRequestId,
            UploadCommand: uploadCommand,
            UploadWindowMinutes: UploadWindowMinutes,
            MaxFileSizeMegabytes: MaxFileSizeMegabytes,
            Instructions: (reusedSession ? "Tell the user this updates their existing inventory. " : "Tell the user the inventory has started. ") +
                $"Show uploadCommand to the user exactly as returned, in a code block; never retype, shorten or split it, never show the URL " +
                $"inside it on its own, and do not repeat it later. Ask them to replace {FilePathPlaceholder} with the full path of their file " +
                $"and run it on their own machine within {UploadWindowMinutes} minutes; the file must be at most {MaxFileSizeMegabytes} MB. " +
                "AzCopy v10 is required (download: https://aka.ms/downloadazcopy-v10-windows). Do not ask them to send the file in the chat. " +
                "After the upload finishes they can ask for the inventory status; keep sessionId and operationId for that.");
    }

    /// <summary>
    /// Builds the azcopy command for a write-only blob SAS URL, or returns null if the URL is not a valid http(s) URL.
    /// Length checking is disabled because it reads the blob back, which a write-only SAS does not allow.
    /// </summary>
    internal static string? BuildUploadCommand(string? uploadUrl)
    {
        if (string.IsNullOrWhiteSpace(uploadUrl)
            || !Uri.TryCreate(uploadUrl, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || uploadUrl.Contains('"'))
        {
            return null;
        }

        return $"azcopy copy \"{FilePathPlaceholder}\" \"{uploadUrl}\" --from-to=LocalBlob --blob-type=BlockBlob --overwrite=true --check-length=false";
    }

    private static CreateOrUpdateInventoryCommandResult Fail(string step, string? sessionId, string? clientRequestId, int? status, string? error) =>
        new(false, error ?? "Unknown error.", step, status, sessionId, false, null, clientRequestId, null, null, null, null);

    /// <summary>Result of the inventory create-or-update command.</summary>
    public sealed record CreateOrUpdateInventoryCommandResult(
        bool Success,
        string? Error,
        string? FailedStep,
        int? HttpStatus,
        string? SessionId,
        bool ReusedExistingSession,
        string? OperationId,
        string? ClientRequestId,
        string? UploadCommand,
        int? UploadWindowMinutes,
        int? MaxFileSizeMegabytes,
        string? Instructions);
}
