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
    private const string DefaultSkillType = "HA";

    private static readonly HttpClient s_storageHttp = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly ILogger<DsHaSessionClient> _logger = logger;

    internal static string SkillType =>
        Environment.GetEnvironmentVariable(SkillTypeSettingName) is { Length: > 0 } s ? s.Trim() : DefaultSkillType;

    internal static string NewSessionId() => "ha-" + Guid.NewGuid().ToString("N")[..16];

    /// <inheritdoc/>
    public async Task<HaSessionResult> StartAsync(string token, HaInput input, CancellationToken cancellationToken)
    {
        string baseUrl = DsBackdoorClient.BaseUrl;
        string sessionId = NewSessionId();
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
        if (create.Status == (int)HttpStatusCode.Conflict && DsBackdoorClient.ReadActiveSessionId(create.Body) is { } activeSessionId)
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
