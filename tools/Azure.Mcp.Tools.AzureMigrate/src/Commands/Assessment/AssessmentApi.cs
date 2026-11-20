// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC helpers shared by the assessment clients to call the Assessment Service machine assessment API
/// (/businessCaseRestApi/machineAssessmentsFromZip/...). The caller's bearer token is forwarded unchanged and is never logged,
/// nor is any SAS query string.
/// </summary>
internal static class AssessmentApi
{
    internal const string BaseUrlSettingName = "AZUREMIGRATE_ASSESSMENT_SERVICE_URL";
    internal const string WorkspaceIdSettingName = "AZUREMIGRATE_ASSESSMENT_WORKSPACE_ID";
    internal const string ApiVersion = "2022-01-05-preview";

    // Assessment Service endpoint (private Stamp03).
    private const string DefaultBaseUrl = "https://asmsrv-pvt-stamp03.ecy.prod.migration.windowsazure.com";

    // TODO: Replace with the caller's workspace once it is defined; create and status must use the same value.
    private const string DefaultWorkspaceId = "azure-migrate-mcp";

    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(60) };

    internal static string BaseUrl =>
        (Environment.GetEnvironmentVariable(BaseUrlSettingName) is { Length: > 0 } s ? s.Trim() : DefaultBaseUrl).TrimEnd('/');

    internal static string ResolveWorkspaceId(string? workspaceId) =>
        !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId.Trim()
            : Environment.GetEnvironmentVariable(WorkspaceIdSettingName) is { Length: > 0 } s ? s.Trim() : DefaultWorkspaceId;

    internal static string CreateUrl(string baseUrl, string assessmentName) =>
        $"{baseUrl}/businessCaseRestApi/machineAssessmentsFromZip/{Uri.EscapeDataString(assessmentName)}?api-version={ApiVersion}";

    internal static string OperationUrl(string baseUrl, Guid operationId, string workspaceId) =>
        $"{baseUrl}/businessCaseRestApi/machineAssessmentsFromZip/operations/{operationId:D}?api-version={ApiVersion}" +
        $"&workspaceId={Uri.EscapeDataString(workspaceId)}";

    /// <summary>
    /// Sends the request and maps the response ({ id, status, output?, error? } or an error body) to an <see cref="AssessmentOperation"/>.
    /// </summary>
    internal static async Task<AssessmentOperation> SendAsync(
        ILogger logger, HttpMethod method, string url, string token, string? body, string step, CancellationToken cancellationToken)
    {
        string logUrl = url.Split('?')[0];
        string clientRequestId = Guid.NewGuid().ToString();
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
            logger.LogInformation("AzureMigrate assessment: {Step} {Url} returned {Status} (request {RequestId}).", step, logUrl, status, clientRequestId);
            return Parse(status, text, retryAfter);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning("AzureMigrate assessment: {Step} {Url} failed: {Error} (request {RequestId}).", step, logUrl, ex.Message, clientRequestId);
            return new(null, null, null, null, null, ex.GetType().Name, $"Assessment {step} failed: {ex.Message}");
        }
    }

    internal static AssessmentOperation Parse(int status, string? body, int? retryAfter)
    {
        Guid? id = Guid.TryParse(DsBackdoorClient.ReadValue(body, "id"), out Guid parsed) ? parsed : null;
        string? errorCode = DsBackdoorClient.ReadValue(body, "error", "code");
        string? errorMessage = DsBackdoorClient.ReadValue(body, "error", "message");
        if (status is not (>= 200 and < 300) && errorCode is null && errorMessage is null)
        {
            errorMessage = string.IsNullOrWhiteSpace(body) ? null : body.Length > 1000 ? body[..1000] : body;
        }

        return new(
            status,
            id,
            DsBackdoorClient.ReadValue(body, "status"),
            DsBackdoorClient.ReadValue(body, "output"),
            retryAfter,
            errorCode,
            errorMessage);
    }
}
