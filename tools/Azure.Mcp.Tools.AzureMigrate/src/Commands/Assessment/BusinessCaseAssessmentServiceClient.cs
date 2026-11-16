// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC client for the Business Case Service machine assessment from a collector ZIP (api-version <see cref="ApiVersion"/>):
/// PUT /businessCaseRestApi/machineAssessmentsFromZip/{assessmentName} starts a run and GET .../operations/{operationId} reads it.
/// The caller's bearer token is forwarded unchanged and is never logged, nor is the inventory SAS URL or any query string.
/// </summary>
public sealed class BusinessCaseAssessmentServiceClient : IAssessmentServiceClient
{
    /// <summary>Environment variable with the Business Case Service base URL. The real client is used only when it is set.</summary>
    internal const string BaseUrlSettingName = "AZUREMIGRATE_ASSESSMENT_SERVICE_URL";
    internal const string ApiVersion = "2022-01-05-preview";

    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(60) };

    private readonly ILogger<BusinessCaseAssessmentServiceClient> _logger;
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    /// <summary>
    /// Creates a client for the given Business Case Service base URL.
    /// </summary>
    public BusinessCaseAssessmentServiceClient(ILogger<BusinessCaseAssessmentServiceClient> logger, string baseUrl)
        : this(logger, s_http, baseUrl)
    {
    }

    internal BusinessCaseAssessmentServiceClient(ILogger<BusinessCaseAssessmentServiceClient> logger, HttpClient http, string baseUrl)
    {
        _logger = logger;
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>Returns the configured base URL, or null when the real client is not configured.</summary>
    internal static string? ConfiguredBaseUrl =>
        Environment.GetEnvironmentVariable(BaseUrlSettingName) is { Length: > 0 } s ? s.Trim() : null;

    internal static string CreateUrl(string baseUrl, string assessmentName) =>
        $"{baseUrl}/businessCaseRestApi/machineAssessmentsFromZip/{Uri.EscapeDataString(assessmentName)}?api-version={ApiVersion}";

    internal static string OperationUrl(string baseUrl, Guid operationId, string? workspaceId) =>
        $"{baseUrl}/businessCaseRestApi/machineAssessmentsFromZip/operations/{operationId:D}?api-version={ApiVersion}" +
        (string.IsNullOrEmpty(workspaceId) ? string.Empty : $"&workspaceId={Uri.EscapeDataString(workspaceId)}");

    internal static string BuildCreateBody(AssessmentRequest request) =>
        DsBackdoorClient.WriteJson(w =>
        {
            // TODO: workspaceId is required by the contract; it is sent only when set until we decide how to choose it.
            if (!string.IsNullOrEmpty(request.WorkspaceId))
            {
                w.WriteString("workspaceId", request.WorkspaceId);
            }

            w.WriteString("inventorySasUri", request.InventorySasUri);
            w.WriteStartObject("settings");
            w.WriteString("azureLocation", request.TargetRegion);
            w.WriteString("currency", request.Currency);
            w.WriteEndObject();
        });

    /// <inheritdoc/>
    public async Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken)
    {
        HttpResult result = await SendAsync(
            HttpMethod.Put, CreateUrl(_baseUrl, request.AssessmentName), token, BuildCreateBody(request), "create assessment", cancellationToken);
        return Parse(result, null);
    }

    /// <inheritdoc/>
    public async Task<AssessmentOperation> GetOperationAsync(Guid operationId, string? workspaceId, string token, CancellationToken cancellationToken)
    {
        HttpResult result = await SendAsync(
            HttpMethod.Get, OperationUrl(_baseUrl, operationId, workspaceId), token, null, "get assessment operation", cancellationToken);
        return Parse(result, operationId);
    }

    /// <summary>
    /// Maps a response to an operation. A successful body is { id, status, output?, error? }; an error body is
    /// { error: { code, message } } or { message }. The operation id falls back to the Operation-Id header, then to the polled id.
    /// </summary>
    internal static AssessmentOperation Parse(HttpResult result, Guid? requestedOperationId)
    {
        if (result.Status is null)
        {
            return new(null, requestedOperationId, null, null, null, null, result.TransportError);
        }

        string? errorCode = DsBackdoorClient.ReadValue(result.Body, "error", "code");
        string? errorMessage = DsBackdoorClient.ReadValue(result.Body, "error", "message")
            ?? DsBackdoorClient.ReadValue(result.Body, "message");
        if (result.Status is not (>= 200 and < 300))
        {
            if (errorCode is null && errorMessage is null && !string.IsNullOrWhiteSpace(result.Body))
            {
                errorMessage = result.Body.Length > 1000 ? result.Body[..1000] : result.Body;
            }

            return new(result.Status, requestedOperationId, null, null, result.RetryAfterSeconds, errorCode, errorMessage);
        }

        Guid? operationId = Guid.TryParse(DsBackdoorClient.ReadValue(result.Body, "id"), out Guid id) ? id
            : Guid.TryParse(result.OperationIdHeader, out Guid headerId) ? headerId
            : requestedOperationId;
        string? status = DsBackdoorClient.ReadValue(result.Body, "status");
        string? output = string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase)
            ? DsBackdoorClient.ReadValue(result.Body, "output")
            : null;
        return new(result.Status, operationId, status, output, result.RetryAfterSeconds, errorCode, errorMessage);
    }

    private async Task<HttpResult> SendAsync(
        HttpMethod method, string url, string token, string? body, string step, CancellationToken cancellationToken)
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

            using HttpResponseMessage resp = await _http.SendAsync(req, cancellationToken);
            string text = await resp.Content.ReadAsStringAsync(cancellationToken);
            int? retryAfter = resp.Headers.RetryAfter?.Delta is { } delta ? (int)delta.TotalSeconds : null;
            string? operationIdHeader = resp.Headers.TryGetValues("Operation-Id", out IEnumerable<string>? values) ? values.FirstOrDefault() : null;
            _logger.LogInformation(
                "AzureMigrate assessment: {Step} {Url} returned {Status} (request {RequestId}).", step, logUrl, (int)resp.StatusCode, clientRequestId);
            return new((int)resp.StatusCode, text, retryAfter, operationIdHeader, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "AzureMigrate assessment: {Step} {Url} failed: {Error} (request {RequestId}).", step, logUrl, ex.Message, clientRequestId);
            return new(null, null, null, null, $"Assessment Service {step} failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>Raw HTTP outcome; <see cref="Status"/> is null when the request did not get a response.</summary>
    internal sealed record HttpResult(int? Status, string? Body, int? RetryAfterSeconds, string? OperationIdHeader, string? TransportError);
}
