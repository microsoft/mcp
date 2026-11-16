// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Starts the HA skill on the Discovery Service MCP backdoor: a session PUT with the HA skill type followed by an invoke, whose
/// response carries the write-only SAS URL for the HA input. The caller's bearer token is forwarded unchanged and is never logged,
/// nor is the SAS query string.
/// </summary>
public sealed class DsHaSessionClient(ILogger<DsHaSessionClient> logger) : IHaSessionClient
{
    internal const string SkillTypeSettingName = "AZUREMIGRATE_HA_SKILL_TYPE";
    private const string DefaultSkillType = "HA";

    private readonly ILogger<DsHaSessionClient> _logger = logger;

    internal static string SkillType =>
        Environment.GetEnvironmentVariable(SkillTypeSettingName) is { Length: > 0 } s ? s.Trim() : DefaultSkillType;

    internal static string NewSessionId() => "ha-" + Guid.NewGuid().ToString("N")[..16];

    /// <inheritdoc/>
    public async Task<HaSessionResult> StartAsync(string token, CancellationToken cancellationToken)
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
            // One active session per user and skill type, with no close API yet, so reuse the caller's active HA session.
            sessionId = activeSessionId;
            reused = true;
        }
        else if (create.Status is not (>= 200 and < 300))
        {
            return new("createSession", create.Status, sessionId, false, null, clientRequestId, null, create.Error);
        }

        DsBackdoorClient.DsResponse invoke = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Post, DsBackdoorClient.InvokeUrl(baseUrl, sessionId), token, "{}", clientRequestId, "invoke HA session", cancellationToken);
        if (invoke.Status is not (>= 200 and < 300))
        {
            return new("invokeSession", invoke.Status, sessionId, reused, null, clientRequestId, null, invoke.Error);
        }

        string? operationId = DsBackdoorClient.ReadValue(invoke.Body, "operationId");
        string? uploadUrl = DsBackdoorClient.ReadValue(invoke.Body, "uri");
        if (string.IsNullOrWhiteSpace(uploadUrl))
        {
            return new("invokeSession", invoke.Status, sessionId, reused, operationId, clientRequestId, null,
                "The Discovery Service did not return an upload URL for the HA session.");
        }

        _logger.LogInformation(
            "AzureMigrate HA: session {SessionId} (reused {Reused}) invoked (operation {OperationId}, request {RequestId}).",
            sessionId, reused, operationId, clientRequestId);
        return new(null, invoke.Status, sessionId, reused, operationId, clientRequestId, uploadUrl, null);
    }
}
