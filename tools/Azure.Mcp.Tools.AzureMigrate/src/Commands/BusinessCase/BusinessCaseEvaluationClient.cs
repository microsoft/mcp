// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// Synchronous evaluations a business case runs after its assessment: the Business Case Service on-premises TCO and the HA evaluate.
/// Both take blob SAS links as input and answer with a SAS link to their result manifest. An identical request reuses the earlier
/// result, so repeating a call is safe.
/// </summary>
public interface IBusinessCaseEvaluationClient
{
    /// <summary>POST /dataplane/onprem-tco on the Business Case Service.</summary>
    Task<EvaluationResult> GetOnPremTcoAsync(string token, string assessedMachinesUri, string inventoryUri, CancellationToken cancellationToken);

    /// <summary>POST /dataplane/ha/evaluate on the HA service.</summary>
    Task<EvaluationResult> EvaluateHaAsync(
        string token, string assessmentsUri, string onPremTcoUri, BusinessCaseHaSettings settings, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the HA evaluate result manifest (a read SAS URL, so no token is sent) and returns its business case per migration strategy.
    /// </summary>
    Task<HaSummaryResult> GetHaSummaryAsync(string haManifestUri, CancellationToken cancellationToken);
}

/// <summary>
/// The business case per migration strategy read from an HA evaluate manifest, as compact JSON in <see cref="Summary"/>.
/// <see cref="HttpStatus"/> is null on a network error or timeout; <see cref="Error"/> is set when there is no summary.
/// </summary>
public sealed record HaSummaryResult(int? HttpStatus, string? Summary, string? Error);

/// <summary>
/// Response of a synchronous evaluation: { id, status, result: { manifestUri } } or { error: { code, message } }.
/// <see cref="HttpStatus"/> is null on a network error or timeout. <see cref="ManifestUri"/> is a read SAS URL, so it is left out of
/// <see cref="ToString"/>.
/// </summary>
public sealed record EvaluationResult(
    int? HttpStatus,
    string? Status,
    string? ManifestUri,
    string? ErrorCode,
    string? ErrorMessage,
    int? RetryAfterSeconds)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"EvaluationResult {{ HttpStatus = {HttpStatus}, Status = {Status}, ErrorCode = {ErrorCode}, ErrorMessage = {ErrorMessage} }}";
}

/// <summary>
/// Calls the Business Case Service and HA service data planes with the caller's bearer token, forwarded unchanged. Neither the token
/// nor any SAS query string is logged.
/// </summary>
public sealed class BusinessCaseEvaluationClient(ILogger<BusinessCaseEvaluationClient> logger) : IBusinessCaseEvaluationClient
{
    internal const string BcsBaseUrlSettingName = "AZUREMIGRATE_BCS_SERVICE_URL";
    internal const string HaBaseUrlSettingName = "AZUREMIGRATE_HA_SERVICE_URL";

    // Private stamps; override with the settings above.
    private const string DefaultBcsBaseUrl = "https://bcs-pvt-stamp04.ecy.prod.migration.windowsazure.com";
    private const string DefaultHaBaseUrl = "https://ha-pvt-aksstamp10.ecy.prod.migration.windowsazure.com";

    // Both evaluations run inside one status call, which must finish within the client's tool call limit.
    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(45) };

    private readonly ILogger<BusinessCaseEvaluationClient> _logger = logger;

    internal static string BcsBaseUrl => ReadBaseUrl(BcsBaseUrlSettingName, DefaultBcsBaseUrl);

    internal static string HaBaseUrl => ReadBaseUrl(HaBaseUrlSettingName, DefaultHaBaseUrl);

    /// <inheritdoc/>
    public Task<EvaluationResult> GetOnPremTcoAsync(string token, string assessedMachinesUri, string inventoryUri, CancellationToken cancellationToken) =>
        SendAsync($"{BcsBaseUrl}/dataplane/onprem-tco", token, BuildOnPremTcoBody(assessedMachinesUri, inventoryUri), "BCS on-premises TCO",
            cancellationToken);

    /// <inheritdoc/>
    public Task<EvaluationResult> EvaluateHaAsync(
        string token, string assessmentsUri, string onPremTcoUri, BusinessCaseHaSettings settings, CancellationToken cancellationToken) =>
        SendAsync($"{HaBaseUrl}/dataplane/ha/evaluate", token, BuildHaEvaluateBody(assessmentsUri, onPremTcoUri, settings), "HA evaluate",
            cancellationToken);

    /// <inheritdoc/>
    public async Task<HaSummaryResult> GetHaSummaryAsync(string haManifestUri, CancellationToken cancellationToken)
    {
        string location = new Uri(haManifestUri).GetLeftPart(UriPartial.Path);
        try
        {
            using HttpResponseMessage resp = await s_http.GetAsync(haManifestUri, cancellationToken);
            int status = (int)resp.StatusCode;
            _logger.LogInformation("AzureMigrate business case: HA manifest {Location} returned {Status}.", location, status);
            if (!resp.IsSuccessStatusCode)
            {
                return new(status, null, $"Reading the HA evaluation result failed (HTTP {status}).");
            }

            string text = await resp.Content.ReadAsStringAsync(cancellationToken);
            string? summary = BuildHaSummary(text);
            return new(status, summary, summary is null ? "The HA evaluation result has no business case summary." : null);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            _logger.LogWarning("AzureMigrate business case: HA manifest {Location} failed: {Error}.", location, ex.Message);
            return new(null, null, $"Reading the HA evaluation result failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns { currency, strategies: [ { strategy, status, annualSavings, paybackPeriodYears, azureTotalCost, onPremisesTotalCost,
    /// yearOnYear } or { strategy, unavailableReason } ] } from 'summary' of an HA evaluate manifest, or null when it has none.
    /// </summary>
    internal static string? BuildHaSummary(string manifestJson)
    {
        JsonElement summary;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(manifestJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object
                || !doc.RootElement.TryGetProperty("summary", out JsonElement s)
                || !s.TryGetProperty("strategies", out JsonElement strategies)
                || strategies.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            summary = s.Clone();
        }
        catch (JsonException)
        {
            return null;
        }

        return DsBackdoorClient.WriteJson(w =>
        {
            if (summary.TryGetProperty("currency", out JsonElement currency) && currency.ValueKind == JsonValueKind.String)
            {
                w.WriteString("currency", currency.GetString());
            }

            w.WriteStartArray("strategies");
            foreach (JsonProperty strategy in summary.GetProperty("strategies").EnumerateObject())
            {
                w.WriteStartObject();
                w.WriteString("strategy", strategy.Name);
                if (strategy.Value.ValueKind == JsonValueKind.Object
                    && strategy.Value.TryGetProperty("businessCase", out JsonElement bc) && bc.ValueKind == JsonValueKind.Object)
                {
                    WriteString(w, "status", bc, "status");
                    WriteRounded(w, "annualSavings", bc, "annualSavings");
                    WriteRounded(w, "paybackPeriodYears", bc, "paybackPeriod");
                    WriteRounded(w, "azureTotalCost", bc, "totalAzureCost", "total");
                    WriteRounded(w, "onPremisesTotalCost", bc, "totalOnPremCost", "total");
                    if (bc.TryGetProperty("yearOnYear", out JsonElement years) && years.ValueKind == JsonValueKind.Array)
                    {
                        w.WriteStartArray("yearOnYear");
                        foreach (JsonElement year in years.EnumerateArray().Where(y => y.ValueKind == JsonValueKind.Object))
                        {
                            w.WriteStartObject();
                            WriteString(w, "year", year, "year");
                            WriteRounded(w, "onPremisesCost", year, "onPremisesCost");
                            WriteRounded(w, "azureCost", year, "azureCost");
                            WriteRounded(w, "savings", year, "savings");
                            w.WriteEndObject();
                        }

                        w.WriteEndArray();
                    }
                }
                else
                {
                    string? reason = strategy.Value.ValueKind == JsonValueKind.Object
                        && strategy.Value.TryGetProperty("businessCaseUnavailableReason", out JsonElement r) && r.ValueKind == JsonValueKind.String
                        ? r.GetString()
                        : null;
                    w.WriteString("unavailableReason", reason ?? "No business case was computed for this strategy.");
                }

                w.WriteEndObject();
            }

            w.WriteEndArray();
        });
    }

    private static void WriteString(Utf8JsonWriter w, string name, JsonElement source, string property)
    {
        if (source.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String)
        {
            w.WriteString(name, value.GetString());
        }
    }

    private static void WriteRounded(Utf8JsonWriter w, string name, JsonElement source, params string[] path)
    {
        JsonElement value = source;
        foreach (string property in path)
        {
            if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(property, out value))
            {
                return;
            }
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            w.WriteNumber(name, Math.Round(value.GetDouble(), 2));
        }
    }

    internal static string BuildOnPremTcoBody(string assessedMachinesUri, string inventoryUri) =>
        DsBackdoorClient.WriteJson(w =>
        {
            w.WriteStartObject("inputs");
            w.WriteStartObject("assessedMachines");
            w.WriteString("uri", assessedMachinesUri);
            w.WriteEndObject();
            w.WriteStartObject("inventory");
            w.WriteString("uri", inventoryUri);
            w.WriteEndObject();
            w.WriteEndObject();
        });

    internal static string BuildHaEvaluateBody(string assessmentsUri, string onPremTcoUri, BusinessCaseHaSettings settings) =>
        DsBackdoorClient.WriteJson(w =>
        {
            w.WriteStartObject("inputs");
            w.WriteStartObject("assessments");
            w.WriteString("uri", assessmentsUri);
            w.WriteEndObject();
            w.WriteStartObject("onPremTco");
            w.WriteString("uri", onPremTcoUri);
            w.WriteEndObject();
            w.WriteEndObject();
            w.WriteStartObject("settings");
            w.WriteNumber("networkCostPercentage", settings.NetworkCostPercentage);
            w.WriteNumber("iaasLaborCostPercentage", settings.IaasLaborCostPercentage);
            w.WriteNumber("paasLaborCostPercentage", settings.PaasLaborCostPercentage);
            w.WriteNumber("managementLaborCostPercentage", settings.ManagementLaborCostPercentage);
            w.WriteNumber("infrastructureGrowthRate", settings.InfrastructureGrowthRate);
            w.WriteStartObject("perYearMigrationCompletionPercentage");
            for (int year = 0; year < settings.MigrationCompletionByYear.Count; year++)
            {
                w.WriteNumber($"Year{year}", settings.MigrationCompletionByYear[year]);
            }

            w.WriteEndObject();
            w.WriteEndObject();
        });

    internal static EvaluationResult Parse(int status, string? body, int? retryAfter)
    {
        string? errorCode = DsBackdoorClient.ReadValue(body, "error", "code");
        string? errorMessage = DsBackdoorClient.ReadValue(body, "error", "message");
        if (status is not (>= 200 and < 300) && errorCode is null && errorMessage is null && !string.IsNullOrWhiteSpace(body))
        {
            errorMessage = body.Length > 1000 ? body[..1000] : body;
        }

        return new(
            status,
            DsBackdoorClient.ReadValue(body, "status"),
            DsBackdoorClient.ReadValue(body, "result", "manifestUri"),
            errorCode,
            errorMessage,
            retryAfter);
    }

    private static string ReadBaseUrl(string settingName, string defaultUrl) =>
        (Environment.GetEnvironmentVariable(settingName) is { Length: > 0 } s ? s.Trim() : defaultUrl).TrimEnd('/');

    private async Task<EvaluationResult> SendAsync(string url, string token, string body, string step, CancellationToken cancellationToken)
    {
        string clientRequestId = Guid.NewGuid().ToString();
        try
        {
            using HttpRequestMessage req = new(HttpMethod.Post, url) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Headers.Add("x-ms-client-request-id", clientRequestId);

            using HttpResponseMessage resp = await s_http.SendAsync(req, cancellationToken);
            int status = (int)resp.StatusCode;
            string text = await resp.Content.ReadAsStringAsync(cancellationToken);
            int? retryAfter = resp.Headers.RetryAfter?.Delta is { } delta ? (int)delta.TotalSeconds : null;
            _logger.LogInformation("AzureMigrate business case: {Step} {Url} returned {Status} (request {RequestId}).", step, url, status, clientRequestId);
            return Parse(status, text, retryAfter);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            _logger.LogWarning("AzureMigrate business case: {Step} {Url} failed: {Error} (request {RequestId}).", step, url, ex.Message, clientRequestId);
            return new(null, null, null, ex.GetType().Name, $"{step} failed: {ex.Message}", null);
        }
    }
}
