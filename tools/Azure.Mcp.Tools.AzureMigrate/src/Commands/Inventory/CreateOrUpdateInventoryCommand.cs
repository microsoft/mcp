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
/// POC: creates or updates an Azure Catalyst inventory for the signed-in user. It creates (or reuses) a user-owned
/// session (PUT /mcprestapi/sessions/{id}) and invokes it (POST /mcprestapi/sessions/{id}/invoke) on the Discovery
/// Service, forwarding the caller's bearer token unchanged. It does not upload or poll: it returns an azcopy
/// command the user runs on their own machine to upload the file, and the IDs to check progress with the status tool.
/// With 'text-only', it invokes an existing session with the message alone (inputType Text) and returns no upload command.
/// </summary>
/// <remarks>
/// Status tracking is driven by text, not code: the server cannot call tools itself, so this description ("Next") and each
/// result's 'instructions' tell the agent to call the status tool straight away and keep calling it until it completes. The
/// agent then tracks and summarises in the same reply, without waiting for the user to say the file is uploaded. When the
/// upload panel is enabled, the panel replaces 'uploadCommand', so the description defers to 'instructions'.
/// </remarks>
[CommandMetadata(
    Id = CommandId,
    Name = "createorupdate",
    Title = "Create or Update Azure Catalyst Inventory",
    Description = "Starts or updates the signed-in user's Azure Catalyst server inventory; the required first step before exporting " +
        "the inventory, assessing an estate or generating an ROI summary. " +
        "When: the user has a new or updated inventory file (a CMDB, CSV, Excel or zip export on their machine), for example 'I want " +
        "to upload the inventory of my estate'; wants to correct or ask about their inventory in text, for example 'set web-01's OS " +
        "to Windows Server 2025' or 'memory is in GB'; or wants to start again after a failed or expired inventory. " +
        "How: call it once, straight away; do not ask the user to attach or send the file in the chat. Omit 'session-id' for a new " +
        "inventory; pass the 'sessionId' from an earlier result to update that inventory. For a text change, pass 'session-id', " +
        "'message' with the user's request as written, and 'text-only' true. " +
        "Returns: 'sessionId', 'operationId', 'instructions' and, for a file, 'uploadCommand' (an azcopy command the user runs on " +
        "their own machine). Unless 'instructions' say an upload panel is shown, show 'uploadCommand' exactly as returned, in a code " +
        "block, without retyping or shortening it. " +
        "Next: success means the inventory has started, not finished. Start tracking it right away with the inventory status tool " +
        "using the same 'sessionId' and 'operationId', and keep tracking until it completes, before exporting the inventory or " +
        "creating an assessment; call the status tool directly yourself, never from a script or code. " +
        "Follow 'instructions'. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CreateOrUpdateInventoryCommand(ILogger<CreateOrUpdateInventoryCommand> logger, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateOrUpdateInventoryOptions, CreateOrUpdateInventoryCommand.CreateOrUpdateInventoryCommandResult>
{
    internal const string CommandId = "8f2d4b6a-1c3e-4a5f-9b7d-0e2c4a6b8d1f";
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

        string? message = string.IsNullOrWhiteSpace(options.Message) ? null : options.Message.Trim();
        if (ValidateTextOnly(options.TextOnly, requestedSessionId, message) is { } textOnlyError)
        {
            return Fail("validation", requestedSessionId, null, null, textOnlyError);
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail("auth", requestedSessionId, null, null, authError);
        }

        if (options.TextOnly)
        {
            return await RunTextOnlyAsync(baseUrl, requestedSessionId!, message!, token, cancellationToken);
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
                    $"The user already has a different active inventory session '{activeSessionId}'. Update that session instead.",
                    instructions: ActiveSessionInstructions(activeSessionId));
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
        string invokeBody = BuildInvokeBody(message, textOnly: false);
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
            return Fail("invokeSession", sessionId, clientRequestId, invoke.Status, "The Discovery Service did not return a valid upload URL.",
                instructions: RetryOnceInstructions);
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
            Instructions: FileInstructions(reusedSession));
    }

    /// <summary>Returns the instructions for a successful file-based create or update.</summary>
    /// <remarks>The upload panel is enabled for the whole server (see <see cref="InventoryUploadWidget.IsEnabled"/>), so the server
    /// returns one set of instructions or the other, never both.</remarks>
    internal static string FileInstructions(bool reusedSession) => InventoryUploadWidget.IsEnabled
        ? WidgetFileInstructions(reusedSession)
        : (reusedSession
            ? "Tell the user this updates their existing inventory, not a new one. "
            : "Tell the user the inventory has started. ") +
        "Show uploadCommand to the user exactly as returned, in a code block; never retype, shorten or split it, never show the URL " +
        $"inside it on its own, and do not repeat it later. Ask them to replace {FilePathPlaceholder} with the full path of their file " +
        $"and run it on their own machine within {UploadWindowMinutes} minutes; the file must be at most {MaxFileSizeMegabytes} MB. " +
        "AzCopy v10 is required (download: https://aka.ms/downloadazcopy-v10-windows). Do not ask them to send the file in the chat. " +
        "Then start tracking the inventory status right away with this sessionId and operationId; do not wait for the user to say " +
        "the upload has finished. Keep checking until it completes, then summarise the result. Do not export the inventory or " +
        "create an assessment before then.";

    /// <summary>Returns the instructions for a successful file-based create or update when the upload widget is shown.</summary>
    /// <remarks>
    /// Reasons for each part of the text:
    /// - Hide 'uploadCommand': the panel uploads the file, and a second path confuses users. The azcopy command is a fallback only,
    ///   for hosts that cannot render the panel, or storage without the CORS rule.
    /// - "Right away, in this same reply": the panel never posts chat messages (ui/message appears as if the user wrote it), so
    ///   nothing would wake the agent after the upload. It must already be tracking when the file arrives.
    /// - "Stays in progress until the file arrives": the Discovery Service reports Running both while it waits for the file and
    ///   while it processes it, so Running alone is not a reason to stop.
    /// - "Quietly": each status call can take up to about 25 seconds and repeats many times; narrating every check floods the chat.
    /// </remarks>
    internal static string WidgetFileInstructions(bool reusedSession) =>
        (reusedSession
            ? "Tell the user this updates their existing inventory, not a new one. "
            : "Tell the user the inventory has started. ") +
        $"An upload panel is shown to the user with this result: ask them to choose their file in it within {UploadWindowMinutes} " +
        $"minutes; the file must be at most {MaxFileSizeMegabytes} MB. Do not show uploadCommand or the URL in it, and do not ask " +
        "them to send the file in the chat. Only if they say the panel is missing or the upload failed, show uploadCommand exactly " +
        $"as returned in a code block and ask them to replace {FilePathPlaceholder} with their file path and run it (AzCopy v10). " +
        "Ask them to choose their file in the panel; they do not need to tell you when it is uploaded. Right away, in this same " +
        "reply, start tracking the inventory status with this sessionId and operationId and keep calling it quietly while it has " +
        "not completed: it stays in progress until the file arrives and is processed. Do not wait for the user to say the file " +
        "is uploaded. When it completes, summarise the result. Do not export the inventory or create an assessment before then.";

    internal const string RetryOnceInstructions =
        "Tell the user the inventory could not be started because of a temporary service problem and offer to try once more. " +
        "If it fails again, ask them to try again later.";

    /// <summary>Returns the instructions when the user already has a different active inventory session.</summary>
    internal static string ActiveSessionInstructions(string activeSessionId) =>
        $"Tell the user they already have an active inventory ('{activeSessionId}') and offer to continue with it. If they agree, " +
        $"use '{activeSessionId}' as the session ID for this and later updates.";

    /// <summary>
    /// Returns the instructions for a failed call, by the step that failed, the Discovery Service HTTP status (null when it
    /// could not be reached) and whether it was a text-only update.
    /// </summary>
    internal static string GetFailureInstructions(string step, int? status, bool textOnly) => (step, status, textOnly) switch
    {
        ("validation", _, _) => "Tell the user which input is not valid, based on 'error', and ask them for the correct value; do not guess it.",
        ("auth", _, _) => "Tell the user their sign-in could not be verified and ask them to sign in again, then try once more.",
        ("invokeSession", 404, true) => "Tell the user this inventory could not be found for them and offer to start a new inventory with their file.",
        ("invokeSession", 400, true) => "Tell the user this inventory has no completed import yet, so it cannot be changed in text; " +
            "ask them to upload their inventory file first.",
        (_, null or >= 500, _) => RetryOnceInstructions,
        _ => "Tell the user the inventory could not be started and quote 'error' in plain language.",
    };

    /// <summary>
    /// Updates an existing inventory from the message alone (inputType Text): the Discovery Service continues the skill
    /// conversation without issuing an upload URL. The session is not created first, so an unknown session fails with 404
    /// rather than starting an empty inventory, and a session with no completed import fails with 400.
    /// </summary>
    private async Task<CreateOrUpdateInventoryCommandResult> RunTextOnlyAsync(
        string baseUrl, string sessionId, string message, string token, CancellationToken cancellationToken)
    {
        string clientRequestId = Guid.NewGuid().ToString();
        DsBackdoorClient.DsResponse invoke = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Post, DsBackdoorClient.InvokeUrl(baseUrl, sessionId), token, BuildInvokeBody(message, textOnly: true),
            clientRequestId, "invoke session", cancellationToken);
        if (invoke.Status is not (>= 200 and < 300))
        {
            return Fail("invokeSession", sessionId, clientRequestId, invoke.Status, invoke.Error, textOnly: true);
        }

        string? operationId = DsBackdoorClient.ReadValue(invoke.Body, "operationId");
        _logger.LogInformation(
            "AzureMigrate inventory: session {SessionId} invoked with text only (operation {OperationId}, request {RequestId}).",
            sessionId, operationId, clientRequestId);
        return TextOnlyResult(sessionId, operationId, clientRequestId, invoke.Status);
    }

    /// <summary>Returns the validation error for a text-only update, or null when the options are valid.</summary>
    internal static string? ValidateTextOnly(bool textOnly, string? sessionId, string? message) =>
        !textOnly ? null
        : sessionId is null ? "'session-id' is required for a text-only update: it updates an inventory created earlier in the conversation."
        : message is null ? "'message' is required for a text-only update."
        : null;

    /// <summary>Returns the invoke body: {"inputType":"Text","message"} for text only, else {} or {"message"} for blob input.</summary>
    internal static string BuildInvokeBody(string? message, bool textOnly) =>
        !textOnly && message is null
            ? "{}"
            : DsBackdoorClient.WriteJson(w =>
            {
                if (textOnly)
                {
                    w.WriteString("inputType", "Text");
                }

                w.WriteString("message", message);
            });

    /// <summary>Returns the result of a text-only update.</summary>
    /// <remarks>
    /// There is no upload, but the change still runs as a Discovery Service operation, so the agent tracks it the same way. Hosts
    /// with the upload panel render it here too; the panel shows "Updating your inventory" instead of a file picker. Null fields are
    /// omitted when serialized, so the panel recognises this result by 'sessionId' and 'success'.
    /// </remarks>
    internal static CreateOrUpdateInventoryCommandResult TextOnlyResult(string sessionId, string? operationId, string clientRequestId, int? status) =>
        new(
            Success: true,
            Error: null,
            FailedStep: null,
            HttpStatus: status,
            SessionId: sessionId,
            ReusedExistingSession: true,
            OperationId: operationId,
            ClientRequestId: clientRequestId,
            UploadCommand: null,
            UploadWindowMinutes: null,
            MaxFileSizeMegabytes: null,
            Instructions: "Tell the user their change is being applied to the inventory and nothing needs to be uploaded. " +
                "Start tracking the inventory status right away with this sessionId and operationId and keep checking until it " +
                "completes, then summarise the result, which answers questions or confirms the changes. Do not export the inventory " +
                "or create an assessment before then.");

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

    internal static CreateOrUpdateInventoryCommandResult Fail(
        string step, string? sessionId, string? clientRequestId, int? status, string? error, bool textOnly = false, string? instructions = null) =>
        new(false, error ?? "Unknown error.", step, status, sessionId, false, null, clientRequestId, null, null, null,
            instructions ?? GetFailureInstructions(step, status, textOnly));

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
