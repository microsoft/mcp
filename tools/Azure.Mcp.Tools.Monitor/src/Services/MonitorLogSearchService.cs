// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models.Log;
using Azure.Mcp.Tools.Monitor.Validation;
using Azure.ResourceManager.OperationalInsights;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// Runs a single bounded Basic or Auxiliary table search against the Log Analytics <c>/search</c> API.
/// </summary>
public sealed class MonitorLogSearchService(
    IAzureService azureService,
    IHttpClientFactory httpClientFactory)
    : BaseAzureService(azureService), IMonitorLogSearchService
{
    private const int MaxResponseBytes = 1024 * 1024;
    private const int MaxRowLimit = 100;
    private const string PublicCloudScope = "https://api.loganalytics.io/.default";
    private const string SearchTimedOutMessage =
        "The Logs search timed out. Use a shorter timespan or a more selective predicate.";
    private static readonly Uri s_publicCloudBaseUri = new("https://api.loganalytics.io/");
    private static readonly TimeSpan s_httpTimeout = TimeSpan.FromSeconds(200);
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public async Task<WorkspaceLogSearchResult> SearchWorkspaceLogs(
        string subscription,
        string resourceGroup,
        string workspace,
        string table,
        string query,
        string timespan,
        int limit,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var timeRange = ValidateRequest(subscription, resourceGroup, workspace, table, query, timespan, limit, now);
        var endpoint = ResolveLogsEndpoint();

        var target = await ResolveSearchTargetAsync(
            subscription,
            resourceGroup,
            workspace,
            table,
            timeRange,
            now,
            tenant,
            cancellationToken);

        var responseBytes = await SendSearchAsync(
            target.CustomerId,
            table,
            query,
            timespan,
            limit,
            endpoint,
            tenant,
            cancellationToken);

        if (responseBytes is null)
        {
            return new(table, target.Plan, timespan, [], [], 0, limit, false, null);
        }

        return MapResponse(responseBytes, table, target.Plan, timespan, limit);
    }

    private static LogSearchTimeRange ValidateRequest(
        string subscription,
        string resourceGroup,
        string workspace,
        string table,
        string query,
        string timespan,
        int limit,
        DateTimeOffset now)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(workspace), workspace),
            (nameof(table), table),
            (nameof(query), query),
            (nameof(timespan), timespan));
        LogSearchQueryValidator.Validate(table, query);

        if (limit is < 1 or > MaxRowLimit)
        {
            throw new CommandValidationException($"--limit must be between 1 and {MaxRowLimit}.");
        }

        return LogSearchTimeRangeParser.Parse(timespan, now);
    }

    private async Task<(Guid CustomerId, string Plan)> ResolveSearchTargetAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string table,
        LogSearchTimeRange timeRange,
        DateTimeOffset now,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ResourceGroupResource? resourceGroupResource;
        try
        {
            resourceGroupResource = await AzureService.GetResourceGroupResource(
                subscription,
                resourceGroup,
                tenant,
                cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw CreateMetadataException(ex, "The specified resource group was not found.", "ResourceGroupNotFound");
        }

        if (resourceGroupResource is null)
        {
            throw new CommandValidationException(
                "The specified resource group was not found.",
                HttpStatusCode.NotFound,
                "ResourceGroupNotFound");
        }

        OperationalInsightsWorkspaceResource workspaceResource;
        try
        {
            var workspaceResponse = await resourceGroupResource
                .GetOperationalInsightsWorkspaces()
                .GetAsync(workspace, cancellationToken);
            workspaceResource = workspaceResponse.Value;
        }
        catch (RequestFailedException ex)
        {
            throw CreateMetadataException(
                ex,
                "The specified Log Analytics workspace was not found in the resource group.",
                "WorkspaceNotFound");
        }

        var customerId = workspaceResource.Data.CustomerId;
        if (!customerId.HasValue)
        {
            throw new CommandValidationException(
                "The Log Analytics workspace metadata did not contain a customer identifier.",
                HttpStatusCode.BadGateway,
                "InvalidWorkspaceMetadata");
        }

        OperationalInsightsTableResource tableResource;
        try
        {
            var tableResponse = await workspaceResource
                .GetOperationalInsightsTables()
                .GetAsync(table, cancellationToken);
            tableResource = tableResponse.Value;
        }
        catch (RequestFailedException ex)
        {
            throw CreateMetadataException(ex, "The specified Log Analytics table was not found.", "TableNotFound");
        }

        var plan = ValidateTablePlan(tableResource, timeRange, now);
        return (customerId.Value, plan);
    }

    private static string ValidateTablePlan(
        OperationalInsightsTableResource tableResource,
        LogSearchTimeRange timeRange,
        DateTimeOffset now)
    {
        var plan = tableResource.Data.Plan?.ToString();
        if (string.IsNullOrWhiteSpace(plan))
        {
            throw CreateIncompleteTableMetadataException();
        }

        // Default Analytics tables can omit the plan-change timestamp.
        bool isBasic = string.Equals(plan, "Basic", StringComparison.OrdinalIgnoreCase);
        if (!isBasic && !string.Equals(plan, "Auxiliary", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandValidationException(
                "This tool only searches Basic and Auxiliary tables. Use monitor_workspace_log_query for Analytics tables.",
                HttpStatusCode.Conflict,
                "UnsupportedTablePlan");
        }

        var lastPlanModifiedDate = tableResource.Data.LastPlanModifiedDate;
        if (string.IsNullOrWhiteSpace(lastPlanModifiedDate))
        {
            throw CreateIncompleteTableMetadataException();
        }

        if (!DateTimeOffset.TryParse(
            lastPlanModifiedDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var transition))
        {
            throw new CommandValidationException(
                "The Log Analytics table plan transition metadata was invalid.",
                HttpStatusCode.BadGateway,
                "InvalidTableMetadata");
        }

        if (isBasic && timeRange.Start < now - LogSearchTimeRangeParser.MaximumTimespan)
        {
            throw new CommandValidationException(
                "Basic table searches cannot start more than 30 days ago.",
                HttpStatusCode.BadRequest,
                "BasicTimespanTooOld");
        }

        if (timeRange.Start < transition)
        {
            throw timeRange.End <= transition
                ? new CommandValidationException(
                    $"The requested interval is entirely before the table's current plan boundary at {transition:O}. Query a range beginning at or after that boundary.",
                    HttpStatusCode.Conflict,
                    "HistoricalTablePlanRange")
                : new CommandValidationException(
                    $"The requested interval crosses a table plan boundary at {transition:O}. Split the interval and query the supported portion beginning at that boundary.",
                    HttpStatusCode.Conflict,
                    "TablePlanTransition");
        }

        return plan;
    }

    /// <summary>
    /// Sends one bounded <c>/search</c> request. Returns <see langword="null"/> when the service reports
    /// no content, otherwise the size-limited response body.
    /// </summary>
    private async Task<byte[]?> SendSearchAsync(
        Guid customerId,
        string table,
        string query,
        string timespan,
        int limit,
        (Uri BaseUri, string Scope) endpoint,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var accessToken = await AcquireLogsTokenAsync(endpoint.Scope, tenant, cancellationToken);

        var requestUri = new Uri(
            endpoint.BaseUri,
            $"v1/workspaces/{customerId:D}/search?timespan={Uri.EscapeDataString(timespan)}");
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        request.Headers.TryAddWithoutValidation("Prefer", "wait=180");
        var requestBytes = JsonSerializer.SerializeToUtf8Bytes(
            new LogSearchApiRequest($"{table} {query.Trim()}\n| take {limit}"),
            MonitorJsonContext.Default.LogSearchApiRequest);
        request.Content = new ByteArrayContent(requestBytes);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var client = _httpClientFactory.CreateClient();
        // ResponseHeadersRead ends HttpClient.Timeout before the body is read.
        client.Timeout = Timeout.InfiniteTimeSpan;
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(s_httpTimeout);

        try
        {
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutSource.Token);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            var responseBytes = await ReadSizeLimitedBytesAsync(response.Content, timeoutSource.Token);
            return response.IsSuccessStatusCode
                ? responseBytes
                : throw CreateSearchHttpException(response, responseBytes);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CommandValidationException(
                SearchTimedOutMessage,
                HttpStatusCode.GatewayTimeout,
                "LogsSearchTimeout");
        }
    }

    private async Task<AccessToken> AcquireLogsTokenAsync(
        string scope,
        string? tenant,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = string.IsNullOrWhiteSpace(tenant)
                ? null
                : await AzureService.ResolveTenantIdAsync(tenant, cancellationToken);
            var credential = await AzureService.GetTokenCredentialAsync(tenantId, cancellationToken);
            return await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
        }
        catch (CredentialUnavailableException)
        {
            throw new CommandValidationException(
                "Azure credentials were unavailable for the Logs search.",
                HttpStatusCode.Unauthorized,
                "CredentialUnavailable");
        }
        catch (AuthenticationFailedException)
        {
            throw new CommandValidationException(
                "Authentication failed while acquiring credentials for the Logs search.",
                HttpStatusCode.Unauthorized,
                "LogsAuthenticationFailed");
        }
        catch (RequestFailedException ex)
        {
            var status = ex.Status is >= 400 and <= 599
                ? (HttpStatusCode)ex.Status
                : HttpStatusCode.BadGateway;
            throw new CommandValidationException(
                $"Azure could not resolve the requested tenant ({SanitizeBackendCode(ex.ErrorCode)}).",
                status,
                "TenantResolutionFailed");
        }
    }

    private static WorkspaceLogSearchResult MapResponse(
        byte[] responseBytes,
        string table,
        string plan,
        string timespan,
        int limit)
    {
        LogSearchApiResponse apiResponse;
        try
        {
            apiResponse = JsonSerializer.Deserialize(
                responseBytes,
                MonitorJsonContext.Default.LogSearchApiResponse)
                ?? throw new JsonException("The response body was empty.");
        }
        catch (JsonException)
        {
            throw new CommandValidationException(
                "The Logs service returned a malformed response.",
                HttpStatusCode.BadGateway,
                "MalformedLogsResponse");
        }

        var result = MapLogSearchResponse(apiResponse, table, plan, timespan, limit);
        var serializedResult = JsonSerializer.SerializeToUtf8Bytes(
            result,
            MonitorJsonContext.Default.WorkspaceLogSearchResult);
        if (serializedResult.Length > MaxResponseBytes)
        {
            throw CreateResponseTooLargeException();
        }

        return result;
    }

    private (Uri BaseUri, string Scope) ResolveLogsEndpoint() =>
        AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => (s_publicCloudBaseUri, PublicCloudScope),
            _ => throw new CommandValidationException(
                "Synchronous Basic and Auxiliary log search is not supported for the configured Azure cloud because its endpoint has not been verified.",
                HttpStatusCode.NotImplemented,
                "UnsupportedCloud")
        };

    /// <summary>
    /// Translates an Azure Resource Manager failure into a sanitized validation error so no raw service
    /// message, header, or body content reaches the caller.
    /// </summary>
    private static CommandValidationException CreateMetadataException(
        RequestFailedException exception,
        string notFoundMessage,
        string notFoundCode)
    {
        var status = (HttpStatusCode)exception.Status;
        return status switch
        {
            HttpStatusCode.NotFound => new(notFoundMessage, HttpStatusCode.NotFound, notFoundCode),
            HttpStatusCode.Unauthorized => new(
                "Authentication failed while reading Log Analytics workspace or table metadata.",
                HttpStatusCode.Unauthorized,
                "MetadataAuthenticationFailed"),
            HttpStatusCode.Forbidden => new(
                "Authorization failed while reading Log Analytics workspace or table metadata.",
                HttpStatusCode.Forbidden,
                "MetadataAuthorizationFailed"),
            HttpStatusCode.Conflict => new(
                "Azure Resource Manager reported a conflict while reading Log Analytics workspace or table metadata.",
                HttpStatusCode.Conflict,
                "MetadataConflict"),
            (HttpStatusCode)429 => new(
                "Azure Resource Manager throttled the workspace or table metadata request. Retry later.",
                (HttpStatusCode)429,
                "MetadataThrottled"),
            _ when exception.Status >= 500 => new(
                $"Azure Resource Manager could not complete the workspace or table metadata request ({SanitizeBackendCode(exception.ErrorCode)}).",
                status,
                "MetadataRequestFailed"),
            _ when exception.Status >= 400 => new(
                $"Azure Resource Manager rejected the workspace or table metadata request ({SanitizeBackendCode(exception.ErrorCode)}).",
                status,
                "MetadataRequestRejected"),
            _ => new(
                $"Azure Resource Manager could not complete the workspace or table metadata request ({SanitizeBackendCode(exception.ErrorCode)}).",
                HttpStatusCode.BadGateway,
                "MetadataRequestFailed")
        };
    }

    private static async Task<byte[]> ReadSizeLimitedBytesAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength > MaxResponseBytes)
        {
            throw CreateResponseTooLargeException();
        }

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        while (true)
        {
            var bytesRead = await stream.ReadAsync(chunk, cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            if (buffer.Length + bytesRead > MaxResponseBytes)
            {
                throw CreateResponseTooLargeException();
            }

            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
        }

        return buffer.ToArray();
    }

    private static WorkspaceLogSearchResult MapLogSearchResponse(
        LogSearchApiResponse response,
        string table,
        string plan,
        string timespan,
        int limit)
    {
        bool isPartial = string.Equals(response.Error?.Code, "PartialError", StringComparison.OrdinalIgnoreCase);
        if (response.Error is not null && !isPartial)
        {
            throw new CommandValidationException(
                $"The Logs service returned a fatal query error ({SanitizeBackendCode(response.Error.Code)}).",
                HttpStatusCode.BadGateway,
                "FatalLogsError");
        }

        if (response.Tables is null)
        {
            throw new CommandValidationException(
                "The Logs service response did not contain a tables collection.",
                HttpStatusCode.BadGateway,
                "MalformedLogsResponse");
        }

        List<LogSearchColumn> columns = [];
        List<IReadOnlyList<JsonElement>> rows = [];
        if (response.Tables.Count > 0)
        {
            var resultTable = SelectResultTable(response.Tables);
            if (resultTable.Columns is null || resultTable.Rows is null ||
                resultTable.Columns.Any(column => string.IsNullOrWhiteSpace(column.Name) || string.IsNullOrWhiteSpace(column.Type)))
            {
                throw new CommandValidationException(
                    "The Logs service response contained invalid table metadata.",
                    HttpStatusCode.BadGateway,
                    "MalformedLogsResponse");
            }

            if (resultTable.Rows.Count > limit ||
                resultTable.Rows.Any(row => row.Count != resultTable.Columns.Count))
            {
                throw new CommandValidationException(
                    "The Logs service response contained an invalid row shape.",
                    HttpStatusCode.BadGateway,
                    "InvalidRowShape");
            }

            columns = resultTable.Columns
                .Select(column => new LogSearchColumn(column.Name!, column.Type!))
                .ToList();
            rows = resultTable.Rows
                .Select(row => (IReadOnlyList<JsonElement>)row)
                .ToList();
        }

        return new(
            table,
            plan,
            timespan,
            columns,
            rows,
            rows.Count,
            limit,
            isPartial,
            isPartial ? CreatePartialError(response.Error!) : null);
    }

    private static LogSearchApiTable SelectResultTable(List<LogSearchApiTable> tables)
    {
        var primaryTables = tables
            .Where(item => string.Equals(item.Name, "PrimaryResult", StringComparison.OrdinalIgnoreCase))
            .ToList();
        return primaryTables.Count switch
        {
            1 => primaryTables[0],
            0 when tables.Count == 1 => tables[0],
            _ => throw new CommandValidationException(
                "The Logs service response contained an ambiguous table shape.",
                HttpStatusCode.BadGateway,
                "MalformedLogsResponse")
        };
    }

    private static LogSearchError CreatePartialError(LogSearchApiError error)
    {
        var details = error.Details?
            .Take(10)
            .Select(detail => new LogSearchErrorDetail(
                SanitizeBackendCode(detail.Code),
                "The service reported an additional partial-query detail."))
            .ToList() ?? [];

        return new(
            "PartialError",
            "The service returned incomplete query results.",
            details);
    }

    private static CommandValidationException CreateSearchHttpException(
        HttpResponseMessage response,
        byte[] responseBytes)
    {
        string code = "LogsRequestFailed";
        if (responseBytes.Length > 0)
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize(
                    responseBytes,
                    MonitorJsonContext.Default.LogSearchApiResponse);
                if (errorResponse?.Error is not null)
                {
                    code = SanitizeBackendCode(errorResponse.Error.Code);
                }
                else
                {
                    var error = JsonSerializer.Deserialize(
                        responseBytes,
                        MonitorJsonContext.Default.LogSearchApiError);
                    code = SanitizeBackendCode(error?.Code);
                }
            }
            catch (JsonException)
            {
                code = "MalformedErrorResponse";
            }
        }

        var status = response.StatusCode;
        string message = status switch
        {
            HttpStatusCode.BadRequest => $"The Logs service rejected the query ({code}).",
            HttpStatusCode.Unauthorized => "Authentication failed for the Logs service.",
            HttpStatusCode.Forbidden => "Authorization failed for the Logs data query.",
            HttpStatusCode.NotFound => "The requested Logs search resource was not found.",
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => SearchTimedOutMessage,
            (HttpStatusCode)429 => CreateThrottledMessage(response),
            _ => $"The Logs service request failed with status {(int)status} ({code})."
        };

        return new(message, status, code);
    }

    private static string CreateThrottledMessage(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        var delay = retryAfter?.Delta ?? (retryAfter?.Date - DateTimeOffset.UtcNow);
        if (!delay.HasValue)
        {
            return "The Logs service throttled the search. Retry later.";
        }

        var seconds = Math.Clamp((int)Math.Ceiling(delay.Value.TotalSeconds), 0, 3600);
        return $"The Logs service throttled the search. Retry after {seconds} seconds.";
    }

    private static string SanitizeBackendCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Unknown";
        }

        var sanitized = new string(code
            .Where(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-')
            .Take(64)
            .ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Unknown" : sanitized;
    }

    private static CommandValidationException CreateResponseTooLargeException() =>
        new(
            "The Logs service response exceeded the 1 MiB limit. Use a shorter timespan, a more selective predicate, or fewer projected columns.",
            HttpStatusCode.RequestEntityTooLarge,
            "ResponseTooLarge");

    private static CommandValidationException CreateIncompleteTableMetadataException() =>
        new(
            "The Log Analytics table plan metadata was incomplete.",
            HttpStatusCode.BadGateway,
            "InvalidTableMetadata");
}
