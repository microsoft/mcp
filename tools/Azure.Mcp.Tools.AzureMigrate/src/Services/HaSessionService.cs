// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Tools.AzureMigrate.Helpers;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Starts the HA skill on the Discovery Service MCP backdoor: a session PUT with the HA skill type, an invoke, whose response carries
/// the write-only SAS URL for the HA input, and the upload of the HA input mini zip to that URL. The caller's bearer token is
/// forwarded unchanged to the Discovery Service only, never to the SAS URL, and is never logged, nor is any SAS query string.
/// </summary>
public sealed class HaSessionService(ILogger<HaSessionService> logger) : IHaSessionService
{
    internal const string SkillTypeSettingName = "AZUREMIGRATE_HA_SKILL_TYPE";
    private const string DefaultSkillType = "BusinessCase";

    /// <summary>Discovery Service that hosts the business case skill; it can differ from the inventory Discovery Service.</summary>
    internal const string BaseUrlSettingName = "AZUREMIGRATE_HA_DS_BASE_URL";
    private const string DefaultBaseUrl = "https://discoverysrv-pvt-stamp03.ecy.prod.migration.windowsazure.com";

    internal static string BaseUrl =>
        (Environment.GetEnvironmentVariable(BaseUrlSettingName) is { Length: > 0 } s ? s.Trim() : DefaultBaseUrl).TrimEnd('/');

    /// <summary>Discovery Service export 'artifacts' value for the business case report: 'external' is ROI.md, 'internal' the artifacts ZIP.</summary>
    internal const string ExportArtifactsSettingName = "AZUREMIGRATE_HA_EXPORT_ARTIFACTS";

    /// <summary>Discovery Service export 'artifacts' value of the business case artifacts ZIP.</summary>
    internal const string ArtifactsZipArtifacts = "internal";

    internal static string ExportArtifacts =>
        Environment.GetEnvironmentVariable(ExportArtifactsSettingName) is { Length: > 0 } s ? s.Trim() : "external";

    private static readonly HttpClient s_storageHttp = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly ILogger<HaSessionService> _logger = logger;

    internal static string SkillType =>
        Environment.GetEnvironmentVariable(SkillTypeSettingName) is { Length: > 0 } s ? s.Trim() : DefaultSkillType;

    internal static string NewSessionId() => "ha-" + Guid.NewGuid().ToString("N")[..16];

    /// <inheritdoc/>
    public Task<HaSessionResult> StartAsync(string token, HaInput input, CancellationToken cancellationToken) =>
        StartCoreAsync(token, input, NewSessionId(), reuseActiveSession: true, cancellationToken);

    /// <inheritdoc/>
    public Task<HaSessionResult> StartInSessionAsync(string token, HaInput input, string sessionId, CancellationToken cancellationToken) =>
        StartCoreAsync(token, input, sessionId, reuseActiveSession: true, cancellationToken);

    /// <inheritdoc/>
    public async Task<HaOperationStatus> GetOperationStatusAsync(string token, string sessionId, Guid operationId, CancellationToken cancellationToken)
    {
        DiscoveryServiceApi.DsResponse response = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Get, DiscoveryServiceApi.OperationStatusUrl(BaseUrl, sessionId, operationId), token, null,
            Guid.NewGuid().ToString(), "get HA operation status", cancellationToken);
        return new(
            response.Status,
            DiscoveryServiceApi.ReadValue(response.Body, "status"),
            DiscoveryServiceApi.ReadValue(response.Body, "properties", "result"),
            DiscoveryServiceApi.ReadValue(response.Body, "error", "code"),
            DiscoveryServiceApi.ReadValue(response.Body, "error", "message"),
            response.Error,
            response.RetryAfterSeconds);
    }

    /// <inheritdoc/>
    public Task<InventoryExportResult> ExportAsync(string token, string sessionId, CancellationToken cancellationToken) =>
        ExportCoreAsync(token, sessionId, ExportArtifacts, "export business case report", cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryExportResult> ExportArtifactsAsync(string token, string sessionId, CancellationToken cancellationToken) =>
        ExportCoreAsync(token, sessionId, ArtifactsZipArtifacts, "export business case artifacts", cancellationToken);

    private async Task<InventoryExportResult> ExportCoreAsync(
        string token, string sessionId, string artifacts, string operation, CancellationToken cancellationToken)
    {
        DiscoveryServiceApi.DsResponse response = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Get, DiscoveryServiceApi.ExportUrl(BaseUrl, sessionId, artifacts), token, null,
            Guid.NewGuid().ToString(), operation, cancellationToken);
        if (response.Status is not (>= 200 and < 300))
        {
            return new(response.Status, null, response.Error);
        }

        string? uri = DiscoveryServiceApi.ReadValue(response.Body, "uri");
        return string.IsNullOrWhiteSpace(uri)
            ? new(response.Status, null, "The Discovery Service export response did not include a download URL.")
            : new(response.Status, uri, null);
    }

    /// <summary>Largest report returned inline (characters); larger reports are only linked.</summary>
    internal const int MaxInlineReportLength = 200_000;

    /// <inheritdoc/>
    public async Task<string?> DownloadReportAsync(string reportUri, CancellationToken cancellationToken)
    {
        string logUrl = reportUri.Split('?')[0];
        try
        {
            using HttpResponseMessage response = await s_storageHttp.GetAsync(reportUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            _logger.LogInformation("AzureMigrate HA: download report {Url} returned {Status}.", logUrl, (int)response.StatusCode);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaxInlineReportLength * 4L)
            {
                return null;
            }

            byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (IsZip(bytes))
            {
                _logger.LogWarning("AzureMigrate HA: report {Url} is a ZIP, not Markdown; it is only linked.", logUrl);
                return null;
            }

            string report = System.Text.Encoding.UTF8.GetString(bytes);
            return report.Length is > 0 and <= MaxInlineReportLength ? report : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AzureMigrate HA: download report {Url} failed: {Error}.", logUrl, ex.Message);
            return null;
        }
    }

    /// <summary>Returns true when the content starts with the ZIP local file header ('PK\x03\x04').</summary>
    internal static bool IsZip(ReadOnlySpan<byte> content) =>
        content.Length >= 4 && content[0] == 0x50 && content[1] == 0x4B && content[2] == 0x03 && content[3] == 0x04;

    /// <summary>The failed step reported while the reused active session still runs an earlier invocation.</summary>
    internal const string SessionBusyStep = "sessionBusy";

    /// <summary>Returns true for a Discovery Service session state in which the session still runs an invocation.</summary>
    internal static bool IsBusy(string? sessionState) =>
        string.Equals(sessionState, "Invoking", StringComparison.OrdinalIgnoreCase)
        || string.Equals(sessionState, "Publishing", StringComparison.OrdinalIgnoreCase);

    private async Task<HaSessionResult> StartCoreAsync(
        string token, HaInput input, string sessionId, bool reuseActiveSession, CancellationToken cancellationToken)
    {
        string baseUrl = BaseUrl;
        string clientRequestId = Guid.NewGuid().ToString();
        string skillType = SkillType;

        string createBody = DiscoveryServiceApi.WriteJson(w =>
        {
            w.WriteStartObject("properties");
            w.WriteString("skillType", skillType);
            w.WriteEndObject();
        });
        DiscoveryServiceApi.DsResponse create = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Put, DiscoveryServiceApi.SessionUrl(baseUrl, sessionId), token, createBody, clientRequestId, "create HA session", cancellationToken);

        bool reused = false;
        if (reuseActiveSession && create.Status == (int)HttpStatusCode.Conflict && DiscoveryServiceApi.ReadActiveSessionId(create.Body) is { } activeSessionId)
        {
            // One active session per user and skill type: on a later run, continue in the caller's existing HA session.
            sessionId = activeSessionId;
            reused = true;

            // A PUT of an existing session returns it unchanged, with its state. While it still runs an earlier business case, wait
            // for it to finish instead of invoking it.
            DiscoveryServiceApi.DsResponse active = await DiscoveryServiceApi.SendAsync(
                _logger, HttpMethod.Put, DiscoveryServiceApi.SessionUrl(baseUrl, sessionId), token, createBody, clientRequestId, "read active HA session", cancellationToken);
            string? state = DiscoveryServiceApi.ReadValue(active.Body, "properties", "sessionState");
            if (IsBusy(state))
            {
                _logger.LogInformation("AzureMigrate HA: active session {SessionId} is {State}; waiting for it to finish.", sessionId, state);
                return new(SessionBusyStep, active.Status, sessionId, true, null, clientRequestId, $"Session '{sessionId}' is {state}.");
            }
        }
        else if (create.Status is not (>= 200 and < 300))
        {
            return new("createSession", create.Status, sessionId, false, null, clientRequestId, create.Error);
        }

        DiscoveryServiceApi.DsResponse invoke = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Post, DiscoveryServiceApi.InvokeUrl(baseUrl, sessionId), token, "{}", clientRequestId, "invoke HA session", cancellationToken);
        if (invoke.Status is not (>= 200 and < 300))
        {
            return new("invokeSession", invoke.Status, sessionId, reused, null, clientRequestId, invoke.Error);
        }

        string? operationId = DiscoveryServiceApi.ReadValue(invoke.Body, "operationId");
        string? uploadUrl = DiscoveryServiceApi.ReadValue(invoke.Body, "uri");
        if (!AssessmentValidation.IsValidInventorySasUri(uploadUrl))
        {
            return new("invokeSession", invoke.Status, sessionId, reused, operationId, clientRequestId,
                "The Discovery Service did not return a valid upload URL for the HA session.");
        }

        (int? uploadStatus, string? uploadError) = await HaInputPackage.UploadAsync(
            s_storageHttp, uploadUrl!, HaInputPackage.Build(input), _logger, (delay, ct) => Task.Delay(delay, ct), cancellationToken);
        if (uploadError is not null)
        {
            return new("uploadInput", uploadStatus, sessionId, reused, operationId, clientRequestId, uploadError);
        }

        _logger.LogInformation(
            "AzureMigrate HA: session {SessionId} (reused {Reused}) invoked and input uploaded (operation {OperationId}, request {RequestId}).",
            sessionId, reused, operationId, clientRequestId);
        return new(null, invoke.Status, sessionId, reused, operationId, clientRequestId, null);
    }
}
