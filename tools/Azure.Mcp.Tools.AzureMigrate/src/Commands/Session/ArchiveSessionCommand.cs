// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureMigrate.Services;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Session;

/// <summary>
/// POC: closes (archives) the caller's inventory (Discovery) session and business case (HA) session together
/// (POST /mcprestapi/sessions/{id}/archive on each Discovery Service), so the next inventory and business case start in new sessions.
/// </summary>
/// <remarks>
/// The inventory sessions are found from the session list. The business case Discovery Service may not serve the list and read
/// routes, so its active session is found from the 'ActiveSessionAlreadyExists' conflict a session create returns, and its archive is
/// confirmed once that conflict is gone; a create that succeeds leaves a new, empty session, which the next business case reuses.
/// </remarks>
[CommandMetadata(
    Id = "3b9c51e2-7d4a-4f0e-8a6b-2c1d9e5f7a30",
    Name = "archive",
    Title = "Start a New Azure Catalyst Session",
    Description = "Closes (archives) the signed-in user's current Azure Catalyst session: both the inventory session and the business " +
        "case session, so the next inventory and business case start fresh. " +
        "When: only when the user explicitly asks to archive, delete, close or reset their session; never call it automatically or " +
        "as part of another flow. Before calling it, ask with your question tool that shows choices (a dropdown), offering exactly '" +
        NewSessionChoice + "' and '" + SameSessionChoice + "', not in text; call it only for '" + NewSessionChoice + "'. For '" +
        SameSessionChoice + "' do not call it and continue the existing flow. " +
        "How: takes no parameters. " +
        "Returns: 'inventoryArchived', 'businessCaseArchived' (true once closed, or when there was nothing to close) and " +
        "'instructions'. " +
        "Next: follow 'instructions'. Do not mention tool or parameter names, or session IDs, to the user. " + ReplyFormat.ToolRule,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ArchiveSessionCommand(ILogger<ArchiveSessionCommand> logger, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<ArchiveSessionOptions, ArchiveSessionCommand.ArchiveSessionCommandResult>
{
    internal const string NewSessionChoice = "Start a new session";
    internal const string SameSessionChoice = "Continue in the same session";
    internal const string ArchivedState = "Archived";

    // The archive workflow normally finishes in a few seconds; wait at most this long for each session to close.
    internal static readonly TimeSpan ArchiveWait = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan s_pollDelay = TimeSpan.FromSeconds(3);

    private readonly ILogger<ArchiveSessionCommand> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ArchiveSessionOptions options, CancellationToken cancellationToken)
    {
        ArchiveSessionCommandResult result = await RunAsync(cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.ArchiveSessionCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    private async Task<ArchiveSessionCommandResult> RunAsync(CancellationToken cancellationToken)
    {
        (string? token, string? authError) = DiscoveryServiceApi.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return new(false, authError ?? "Unknown error.", false, false,
                ReplyFormat.Failure("Your session could not be closed.", "offer to try again, or to continue in the same session."));
        }

        string clientRequestId = Guid.NewGuid().ToString();
        Task<Outcome> inventory = ArchiveInventoryAsync(DiscoveryServiceApi.BaseUrl, token, clientRequestId, cancellationToken);
        Task<Outcome> businessCase = ArchiveBusinessCaseAsync(HaSessionService.BaseUrl, token, clientRequestId, cancellationToken);
        await Task.WhenAll(inventory, businessCase);
        return BuildResult(inventory.Result, businessCase.Result);
    }

    internal static ArchiveSessionCommandResult BuildResult(Outcome inventory, Outcome businessCase)
    {
        if (inventory.Closed && businessCase.Closed)
        {
            bool nothingClosed = !inventory.HadActive && !businessCase.HadActive;
            return new(true, null, true, true,
                ReplyFormat.Reply(
                    nothingClosed ? "There was no active session to close; you can start fresh." : "Previous session closed; a new session is ready.",
                    "none; skip this line.",
                    "offer to start a new inventory with their file."));
        }

        string error = string.Join(" ", new[]
        {
            inventory.Closed ? null : "Inventory: " + inventory.Error,
            businessCase.Closed ? null : "Business case: " + businessCase.Error,
        }.Where(e => e is not null));
        string which = (inventory.Closed, businessCase.Closed) switch
        {
            (false, false) => "Your previous session could not be closed.",
            (false, true) => "Your previous business case was closed, but your inventory could not be closed.",
            _ => "Your previous inventory was closed, but your business case could not be closed.",
        };
        return new(false, error, inventory.Closed, businessCase.Closed,
            ReplyFormat.Failure(which, "offer to try again in a minute, or to continue in the same session."));
    }

    private async Task<Outcome> ArchiveInventoryAsync(string baseUrl, string token, string clientRequestId, CancellationToken cancellationToken)
    {
        DiscoveryServiceApi.DsResponse list = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Get, SessionsUrl(baseUrl), token, null, clientRequestId, "list sessions", cancellationToken);
        if (list.Status is not (>= 200 and < 300))
        {
            return new(false, false, list.Error ?? $"Listing sessions failed ({list.Status}).");
        }

        List<string> targets = ReadActiveSessionIds(list.Body, DiscoveryServiceApi.SkillType);
        if (targets.Count == 0)
        {
            return new(true, false, null);
        }

        foreach (string id in targets)
        {
            DiscoveryServiceApi.DsResponse archive = await DiscoveryServiceApi.SendAsync(
                _logger, HttpMethod.Post, ArchiveUrl(baseUrl, id), token, "{}", clientRequestId, "archive session", cancellationToken);
            if (archive.Status is not (>= 200 and < 300))
            {
                return new(false, true, archive.Error ?? $"Archive failed ({archive.Status}).");
            }
        }

        // Archive runs as a workflow; wait until every session reports Archived so a new one can be created straight away.
        Stopwatch elapsed = Stopwatch.StartNew();
        HashSet<string> pending = new(targets, StringComparer.OrdinalIgnoreCase);
        while (pending.Count > 0 && elapsed.Elapsed < ArchiveWait)
        {
            await Task.Delay(s_pollDelay, cancellationToken);
            foreach (string id in pending.ToList())
            {
                DiscoveryServiceApi.DsResponse get = await DiscoveryServiceApi.SendAsync(
                    _logger, HttpMethod.Get, DiscoveryServiceApi.SessionUrl(baseUrl, id), token, null, clientRequestId, "read session", cancellationToken);
                if (string.Equals(DiscoveryServiceApi.ReadValue(get.Body, "properties", "sessionState"), ArchivedState, StringComparison.OrdinalIgnoreCase))
                {
                    pending.Remove(id);
                }
            }
        }

        return pending.Count == 0 ? new(true, true, null) : new(false, true, "The inventory session was not closed in time.");
    }

    private async Task<Outcome> ArchiveBusinessCaseAsync(string baseUrl, string token, string clientRequestId, CancellationToken cancellationToken)
    {
        (int? status, string? sessionId, string? error) = await FindActiveHaSessionAsync(baseUrl, token, clientRequestId, cancellationToken);
        if (error is not null)
        {
            return new(false, false, error);
        }

        if (sessionId is null)
        {
            return new(true, false, null);
        }

        DiscoveryServiceApi.DsResponse archive = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Post, ArchiveUrl(baseUrl, sessionId), token, "{}", clientRequestId, "archive HA session", cancellationToken);
        if (archive.Status is not (>= 200 and < 300))
        {
            return new(false, true, archive.Error ?? $"Archive failed ({archive.Status}).");
        }

        // Archive runs as a workflow: it is done once the session no longer blocks a new one.
        Stopwatch elapsed = Stopwatch.StartNew();
        while (elapsed.Elapsed < ArchiveWait)
        {
            await Task.Delay(s_pollDelay, cancellationToken);
            (_, string? activeId, string? probeError) = await FindActiveHaSessionAsync(baseUrl, token, clientRequestId, cancellationToken);
            if (probeError is null && !string.Equals(activeId, sessionId, StringComparison.OrdinalIgnoreCase))
            {
                return new(true, true, null);
            }
        }

        return new(false, true, $"The business case session was not closed in time ({status}).");
    }

    /// <summary>
    /// Returns the caller's active business case session ID from the conflict a session create returns, or null when the create
    /// succeeds because none is active.
    /// </summary>
    private async Task<(int? Status, string? ActiveSessionId, string? Error)> FindActiveHaSessionAsync(
        string baseUrl, string token, string clientRequestId, CancellationToken cancellationToken)
    {
        string body = DiscoveryServiceApi.WriteJson(w =>
        {
            w.WriteStartObject("properties");
            w.WriteString("skillType", HaSessionService.SkillType);
            w.WriteEndObject();
        });
        DiscoveryServiceApi.DsResponse create = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Put, DiscoveryServiceApi.SessionUrl(baseUrl, HaSessionService.NewSessionId()), token, body, clientRequestId,
            "find active HA session", cancellationToken);
        if (create.Status is >= 200 and < 300)
        {
            return (create.Status, null, null);
        }

        return create.Status == (int)HttpStatusCode.Conflict && DiscoveryServiceApi.ReadActiveSessionId(create.Body) is { } activeId
            ? (create.Status, activeId, null)
            : (create.Status, null, create.Error ?? "The active business case session could not be found.");
    }

    internal static string SessionsUrl(string baseUrl) => $"{baseUrl}/mcprestapi/sessions?api-version={DiscoveryServiceApi.ApiVersion}";

    internal static string ArchiveUrl(string baseUrl, string sessionId) =>
        $"{baseUrl}/mcprestapi/sessions/{sessionId}/archive?api-version={DiscoveryServiceApi.ApiVersion}";

    /// <summary>Returns the IDs of the listed sessions of the given skill type that are not archived yet.</summary>
    internal static List<string> ReadActiveSessionIds(string? listJson, string skillType)
    {
        List<string> ids = [];
        if (string.IsNullOrWhiteSpace(listJson))
        {
            return ids;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(listJson);
            if (!doc.RootElement.TryGetProperty("value", out JsonElement value) || value.ValueKind != JsonValueKind.Array)
            {
                return ids;
            }

            foreach (JsonElement session in value.EnumerateArray())
            {
                string raw = session.GetRawText();
                string? id = DiscoveryServiceApi.ReadValue(raw, "id");
                string? skill = DiscoveryServiceApi.ReadValue(raw, "properties", "skillType");
                string? state = DiscoveryServiceApi.ReadValue(raw, "properties", "sessionState");
                if (DiscoveryServiceApi.IsValidSessionId(id)
                    && string.Equals(skill, skillType, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(state, ArchivedState, StringComparison.OrdinalIgnoreCase))
                {
                    ids.Add(id!);
                }
            }
        }
        catch (JsonException)
        {
        }

        return ids;
    }

    /// <summary>Outcome of closing one session kind: closed (or nothing to close), whether one was active, and the error otherwise.</summary>
    internal sealed record Outcome(bool Closed, bool HadActive, string? Error);

    /// <summary>Result of the session archive command.</summary>
    public sealed record ArchiveSessionCommandResult(
        bool Success,
        string? Error,
        bool InventoryArchived,
        bool BusinessCaseArchived,
        string? Instructions);
}
