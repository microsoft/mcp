// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Starts the HA skill on the Discovery Service MCP backdoor: a session PUT with the HA skill type, an invoke, whose response carries
/// the write-only SAS URL for the HA input, and the upload of the HA input mini zip to that URL. The caller's bearer token is
/// forwarded unchanged to the Discovery Service only, never to the SAS URL, and is never logged, nor is any SAS query string.
/// </summary>
public sealed class DsHaSessionClient(ILogger<DsHaSessionClient> logger) : IHaSessionClient
{
    internal const string SkillTypeSettingName = "AZUREMIGRATE_HA_SKILL_TYPE";
    private const string DefaultSkillType = "BusinessCase";

    /// <summary>Discovery Service that hosts the business case skill; it can differ from the inventory Discovery Service.</summary>
    internal const string BaseUrlSettingName = "AZUREMIGRATE_HA_DS_BASE_URL";
    private const string DefaultBaseUrl = "https://discoverysrv-pvt-stamp03.ecy.prod.migration.windowsazure.com";

    internal static string BaseUrl =>
        (Environment.GetEnvironmentVariable(BaseUrlSettingName) is { Length: > 0 } s ? s.Trim() : DefaultBaseUrl).TrimEnd('/');

    /// <summary>Discovery Service export 'artifacts' value for the business case report: 'internal' is ROI.md, 'external' the migration proposal.</summary>
    internal const string ExportArtifactsSettingName = "AZUREMIGRATE_HA_EXPORT_ARTIFACTS";

    internal static string ExportArtifacts =>
        Environment.GetEnvironmentVariable(ExportArtifactsSettingName) is { Length: > 0 } s ? s.Trim() : "internal";

    private static readonly HttpClient s_storageHttp = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly ILogger<DsHaSessionClient> _logger = logger;

    internal static string SkillType =>
        Environment.GetEnvironmentVariable(SkillTypeSettingName) is { Length: > 0 } s ? s.Trim() : DefaultSkillType;

    internal static string NewSessionId() => "ha-" + Guid.NewGuid().ToString("N")[..16];

    /// <inheritdoc/>
    public Task<HaSessionResult> StartAsync(string token, HaInput input, CancellationToken cancellationToken) =>
        StartCoreAsync(token, input, NewSessionId(), reuseActiveSession: true, cancellationToken);

    /// <inheritdoc/>
    public Task<HaSessionResult> StartInSessionAsync(string token, HaInput input, string sessionId, CancellationToken cancellationToken) =>
        StartCoreAsync(token, input, sessionId, reuseActiveSession: false, cancellationToken);

    /// <inheritdoc/>
    public async Task<HaOperationStatus> GetOperationStatusAsync(string token, string sessionId, Guid operationId, CancellationToken cancellationToken)
    {
        DsBackdoorClient.DsResponse response = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Get, DsBackdoorClient.OperationStatusUrl(BaseUrl, sessionId, operationId), token, null,
            Guid.NewGuid().ToString(), "get HA operation status", cancellationToken);
        return new(
            response.Status,
            DsBackdoorClient.ReadValue(response.Body, "status"),
            DsBackdoorClient.ReadValue(response.Body, "properties", "result"),
            DsBackdoorClient.ReadValue(response.Body, "error", "code"),
            DsBackdoorClient.ReadValue(response.Body, "error", "message"),
            response.Error,
            response.RetryAfterSeconds);
    }

    /// <inheritdoc/>
    public async Task<InventoryExportResult> ExportAsync(string token, string sessionId, CancellationToken cancellationToken)
    {
        DsBackdoorClient.DsResponse response = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Get, DsBackdoorClient.ExportUrl(BaseUrl, sessionId, ExportArtifacts), token, null,
            Guid.NewGuid().ToString(), "export business case report", cancellationToken);
        if (response.Status is not (>= 200 and < 300))
        {
            return new(response.Status, null, response.Error);
        }

        string? uri = DsBackdoorClient.ReadValue(response.Body, "uri");
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

            string report = await response.Content.ReadAsStringAsync(cancellationToken);
            return report.Length is > 0 and <= MaxInlineReportLength ? report : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AzureMigrate HA: download report {Url} failed: {Error}.", logUrl, ex.Message);
            return null;
        }
    }

    private async Task<HaSessionResult> StartCoreAsync(
        string token, HaInput input, string sessionId, bool reuseActiveSession, CancellationToken cancellationToken)
    {
        string baseUrl = BaseUrl;
        string clientRequestId = Guid.NewGuid().ToString();
        string skillType = SkillType;

        string createBody = DsBackdoorClient.WriteJson(w =>
        {
            w.WriteStartObject("properties");
            w.WriteString("skillType", skillType);
            w.WriteEndObject();
        });
        DsBackdoorClient.DsResponse create = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Put, DsBackdoorClient.SessionUrl(baseUrl, sessionId), token, createBody, clientRequestId, "create HA session", cancellationToken);

        bool reused = false;
        if (reuseActiveSession && create.Status == (int)HttpStatusCode.Conflict && DsBackdoorClient.ReadActiveSessionId(create.Body) is { } activeSessionId)
        {
            // One active session per user and skill type: on a later run, continue in the caller's existing HA session.
            sessionId = activeSessionId;
            reused = true;
        }
        else if (create.Status is not (>= 200 and < 300))
        {
            return new("createSession", create.Status, sessionId, false, null, clientRequestId, create.Error);
        }

        DsBackdoorClient.DsResponse invoke = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Post, DsBackdoorClient.InvokeUrl(baseUrl, sessionId), token, "{}", clientRequestId, "invoke HA session", cancellationToken);
        if (invoke.Status is not (>= 200 and < 300))
        {
            return new("invokeSession", invoke.Status, sessionId, reused, null, clientRequestId, invoke.Error);
        }

        string? operationId = DsBackdoorClient.ReadValue(invoke.Body, "operationId");
        string? uploadUrl = DsBackdoorClient.ReadValue(invoke.Body, "uri");
        if (!CreateAssessmentCommand.IsValidInventorySasUri(uploadUrl))
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
