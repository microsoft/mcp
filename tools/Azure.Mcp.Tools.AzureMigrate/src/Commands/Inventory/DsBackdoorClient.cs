// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;

/// <summary>
/// POC helpers shared by the inventory commands to call the Discovery Service MCP backdoor
/// (/mcprestapi/...). The caller's bearer token is forwarded unchanged and is never logged, nor is any
/// SAS query string.
/// </summary>
internal static partial class DsBackdoorClient
{
    internal const string SkillTypeSettingName = "AZUREMIGRATE_INVENTORY_SKILL_TYPE";
    internal const string ApiVersion = "2026-11-11-preview";

    // Discovery Service backdoor endpoint (private Stamp04).
    internal const string BaseUrl = "https://discoverysrv-pvt-stamp04.ecy.prod.migration.windowsazure.com";

    private const string DefaultSkillType = "Discovery";

    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(60) };

    internal static string SkillType =>
        Environment.GetEnvironmentVariable(SkillTypeSettingName) is { Length: > 0 } s ? s.Trim() : DefaultSkillType;

    // Discovery Service limit: 1-20 characters from [A-Za-z0-9-_].
    [GeneratedRegex("^[A-Za-z0-9_-]{1,20}$")]
    internal static partial Regex SessionIdRegex();

    [GeneratedRegex(@"Session '([A-Za-z0-9_\-]{1,20})'")]
    private static partial Regex ActiveSessionRegex();

    internal static bool IsValidSessionId(string? sessionId) =>
        !string.IsNullOrEmpty(sessionId) && SessionIdRegex().IsMatch(sessionId);

    internal static string SessionUrl(string baseUrl, string sessionId) =>
        $"{baseUrl}/mcprestapi/sessions/{sessionId}?api-version={ApiVersion}";

    internal static string InvokeUrl(string baseUrl, string sessionId) =>
        $"{baseUrl}/mcprestapi/sessions/{sessionId}/invoke?api-version={ApiVersion}";

    internal static string OperationStatusUrl(string baseUrl, string sessionId, Guid operationId) =>
        $"{baseUrl}/mcprestapi/sessions/{sessionId}/operationstatus/{operationId:D}?api-version={ApiVersion}";

    /// <summary>
    /// Returns the caller's bearer token, or an error explaining why it is missing.
    /// </summary>
    internal static (string? Token, string? Error) GetBearerToken(IHttpContextAccessor accessor)
    {
        string? header = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return (null, accessor.HttpContext is null
                ? "No HTTP request context (server is running over stdio)."
                : "No bearer token on the inbound request.");
        }

        return (header["Bearer ".Length..].Trim(), null);
    }

    internal static async Task<DsResponse> SendAsync(
        ILogger logger, HttpMethod method, string url, string token, string? body, string clientRequestId, string step, CancellationToken cancellationToken)
    {
        string logUrl = url.Split('?')[0];
        try
        {
            using HttpRequestMessage req = new(method, url);
            if (body is not null)
            {
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Headers.Add("x-ms-client-request-id", clientRequestId);

            using HttpResponseMessage resp = await s_http.SendAsync(req, cancellationToken);
            int status = (int)resp.StatusCode;
            string text = await resp.Content.ReadAsStringAsync(cancellationToken);
            int? retryAfter = resp.Headers.RetryAfter?.Delta is { } delta ? (int)delta.TotalSeconds : null;
            logger.LogInformation("AzureMigrate inventory: {Step} {Url} returned {Status} (request {RequestId}).", step, logUrl, status, clientRequestId);
            return resp.IsSuccessStatusCode
                ? new(status, text, null, retryAfter)
                : new(status, text, $"Discovery Service {step} failed (HTTP {status}): {(text.Length > 1000 ? text[..1000] : text)}", retryAfter);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning("AzureMigrate inventory: {Step} {Url} failed: {Error} (request {RequestId}).", step, logUrl, ex.Message, clientRequestId);
            return new(null, null, $"Discovery Service {step} failed: {ex.GetType().Name}: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Reads the caller's active session id from a Discovery Service 'ActiveSessionAlreadyExists' error, whose
    /// message names it as "An active Session '{id}' already exists ...".
    /// </summary>
    internal static string? ReadActiveSessionId(string? errorJson)
    {
        if (string.IsNullOrWhiteSpace(errorJson) || !errorJson.Contains("ActiveSessionAlreadyExists", StringComparison.Ordinal))
        {
            return null;
        }

        Match match = ActiveSessionRegex().Match(errorJson);
        return match.Success ? match.Groups[1].Value : null;
    }

    internal static string NewSessionId() => "inv-" + Guid.NewGuid().ToString("N")[..16];

    /// <summary>
    /// Reads a property by case-insensitive path. Strings are returned as-is; objects and arrays as raw JSON.
    /// </summary>
    internal static string? ReadValue(string? json, params string[] path)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement current = doc.RootElement;
            foreach (string name in path)
            {
                if (current.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                JsonElement? next = null;
                foreach (JsonProperty property in current.EnumerateObject())
                {
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        next = property.Value;
                        break;
                    }
                }

                if (next is null)
                {
                    return null;
                }

                current = next.Value;
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Object or JsonValueKind.Array => current.GetRawText(),
                _ => null,
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static string WriteJson(Action<Utf8JsonWriter> writeProperties)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writeProperties(writer);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal sealed record DsResponse(int? Status, string? Body, string? Error, int? RetryAfterSeconds);
}
