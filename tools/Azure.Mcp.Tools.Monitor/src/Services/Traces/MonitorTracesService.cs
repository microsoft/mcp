// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.Time;

namespace Azure.Mcp.Tools.Monitor.Services.Traces;

/// <summary>
/// Backing service for the <c>monitor traces</c> sub-namespace. A trace account is an Azure Monitor
/// Workspace enabled for Trace Store by creating its <c>traceContainers/default</c> sub-resource.
/// </summary>
public sealed class MonitorTracesService(
    IAzureService azureService,
    ICacheService cacheService,
    ILogger<MonitorTracesService> logger,
    IDateTimeProvider dateTimeProvider)
    : BaseAzureResourceService(azureService), IMonitorTracesService
{
    internal const string TraceContainerResourceType = "Microsoft.Monitor/accounts/traceContainers";
    internal const string TraceContainerApiVersion = "2024-10-03-preview";
    internal const string TraceContainerRelativePath = "traceContainers/default";

    // These ARM versions target distinct resources: the workspace endpoint and its trace container.
    internal const string MonitorAccountResourceType = "Microsoft.Monitor/accounts";
    internal const string MonitorAccountApiVersion = "2025-10-03-preview";

    internal const string TraceDataPlaneScope = "https://data.monitor.azure.com/.default";
    internal const string TraceDataPlaneHostSuffix = ".data.monitor.azure.com";
    internal const string TraceDataPlaneApiVersion = "2026-01-10-preview";

    // Bounded so a slow query hands back an operationId instead of blocking the MCP transport.
    private static readonly TimeSpan PollDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxPollBudget = TimeSpan.FromSeconds(180);
    private const int MaxResumeRows = 1000;

    // account list probes ARM once per workspace; an unbounded burst is what provokes the
    // throttling that would otherwise make the list come back short.
    private const int MaxProbeConcurrency = 8;

    private const string CacheGroup = "monitor";
    private const string TraceEndpointCacheKey = "trace-endpoint";
    private static readonly TimeSpan EndpointCacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>Everything a data-plane call needs: resolved account, base URL, token and pipeline.</summary>
    private sealed record TraceSession(string AmwResourceId, string EndpointBase, string Token, HttpPipeline Pipeline);

    private async Task<TraceSession> OpenSessionAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tenant,
        CancellationToken cancellationToken)
    {
        // Reject unsupported clouds before ARM discovery or data-plane token acquisition.
        string scope = AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => TraceDataPlaneScope,
            var cloud => throw new NotSupportedException(
                $"Azure Monitor Trace Store has no published data-plane audience for {cloud}. "
                + "The 'monitor traces' commands are Azure public cloud only."),
        };
        var (amwResourceId, endpointBase) = await ResolveTraceEndpointAsync(
            subscription, resourceGroup, workspace, tenant, cancellationToken).ConfigureAwait(false);
        ValidateTraceEndpoint(AzureService, endpointBase);
        TokenCredential credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);
        AccessToken token = await credential.GetTokenAsync(
            new TokenRequestContext([scope]), cancellationToken).ConfigureAwait(false);

        var options = AddDefaultPolicies(new MonitorTracesClientOptions());
        // Per-attempt cap for a slow submit or poll.
        options.Retry.NetworkTimeout = MaxPollBudget;
        options.Transport = new HttpClientTransport(AzureService.GetClient());

        return new TraceSession(amwResourceId, endpointBase, token.Token, HttpPipelineBuilder.Build(options));
    }

    private async Task<Response> SendDataPlaneAsync(
        TraceSession session,
        RequestMethod method,
        string url,
        RequestContent? content,
        CancellationToken cancellationToken)
    {
        AzureService.ValidateAzureServiceEndpoint(endpoint: url, serviceType: "monitor-traces");
        using var request = session.Pipeline.CreateRequest();
        request.Method = method;
        request.Uri.Reset(new Uri(url));
        request.Headers.Add("Authorization", $"Bearer {session.Token}");
        if (content is not null)
        {
            request.Headers.Add("Content-Type", "application/json");
            request.Content = content;
        }

        return await session.Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    // Carries the backend status so BaseCommand maps it, rather than collapsing everything to 422.
    private static HttpRequestException DataPlaneFailure(string operation, Response response) =>
        new($"Trace {operation} failed ({response.Status} {response.ReasonPhrase}): {Truncate(response.Content.ToString())}",
            inner: null,
            statusCode: (System.Net.HttpStatusCode)response.Status);

    public async Task<ResourceQueryResults<TraceAccountInfo>> ListAccountsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        // Resource Graph doesn't index the preview traceContainers resource type, so we
        // enumerate AMWs (which Resource Graph does index) and probe each for the container.
        var workspaces = await ExecuteResourceQueryAsync(
            MonitorAccountResourceType,
            resourceGroup,
            subscription,
            ExtractWorkspaceRow,
            tenant: tenant,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (workspaces.Results.Count == 0)
        {
            return new([], workspaces.AreResultsTruncated);
        }

        var armClient = await CreateArmClientWithApiVersionAsync(
            TraceContainerResourceType,
            TraceContainerApiVersion,
            tenant,
            cancellationToken).ConfigureAwait(false);

        var rows = workspaces.Results;
        var probed = new TraceAccountInfo?[rows.Count];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, rows.Count),
            new ParallelOptions { MaxDegreeOfParallelism = MaxProbeConcurrency, CancellationToken = cancellationToken },
            async (index, ct) =>
            {
                probed[index] = await TryLoadContainerAsync(armClient, rows[index], ct).ConfigureAwait(false);
            }).ConfigureAwait(false);

        return new([.. probed.OfType<TraceAccountInfo>()], workspaces.AreResultsTruncated);
    }

    public async Task<TraceFieldNames> ListFieldNamesAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(workspace), workspace));

        var session = await OpenSessionAsync(subscription, resourceGroup, workspace, tenant, cancellationToken).ConfigureAwait(false);
        var url = $"{session.EndpointBase}/field-names?api-version={TraceDataPlaneApiVersion}";

        using var response = await SendDataPlaneAsync(session, RequestMethod.Get, url, null, cancellationToken).ConfigureAwait(false);

        if (response.IsError)
        {
            throw DataPlaneFailure("field-names request", response);
        }

        using var doc = JsonDocument.Parse(response.Content.ToString());
        return new TraceFieldNames
        {
            TraceFields = ParseFieldArray(doc.RootElement, "traceFields"),
            SpanFields = ParseFieldArray(doc.RootElement, "spanFields"),
        };
    }

    public async Task<TraceQueryResult> RunQueryAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string query,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int maxResults,
        bool includeSpans,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(workspace), workspace),
            (nameof(query), query));

        var session = await OpenSessionAsync(subscription, resourceGroup, workspace, tenant, cancellationToken).ConfigureAwait(false);

        string operationId = await SendSubmitAsync(
            session, query, startTime, endTime, maxResults, cancellationToken).ConfigureAwait(false);

        return await PollForResultsAsync(
            session, operationId, maxResults, maxResults, includeSpans, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TraceQueryResult> ResumeQueryAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string operationId,
        int pageSize = 100,
        bool includeSpans = true,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(workspace), workspace),
            (nameof(operationId), operationId));

        TraceSession session = await OpenSessionAsync(subscription, resourceGroup, workspace, tenant, cancellationToken).ConfigureAwait(false);
        return await PollForResultsAsync(
            session, operationId, pageSize, MaxResumeRows, includeSpans, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TraceQueryCancelResult> CancelQueryAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string operationId,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(workspace), workspace),
            (nameof(operationId), operationId));

        var session = await OpenSessionAsync(subscription, resourceGroup, workspace, tenant, cancellationToken).ConfigureAwait(false);
        var url = $"{session.EndpointBase}/operations/{Uri.EscapeDataString(operationId)}:cancel?api-version={TraceDataPlaneApiVersion}";

        using var response = await SendDataPlaneAsync(session, RequestMethod.Post, url, null, cancellationToken).ConfigureAwait(false);
        var content = response.Content.ToString();

        if (response.Status == 404)
        {
            throw new KeyNotFoundException($"Operation '{operationId}' not found or has expired.");
        }

        if (response.IsError)
        {
            throw DataPlaneFailure("query cancel", response);
        }

        var status = "unknown";
        try
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                using JsonDocument doc = JsonDocument.Parse(content);
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("status", out var s) &&
                    s.ValueKind == JsonValueKind.String)
                {
                    status = s.GetString() ?? "unknown";
                }
            }
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException(
                "Trace query cancel returned malformed JSON.", ex, System.Net.HttpStatusCode.BadGateway);
        }

        return new TraceQueryCancelResult { OperationId = operationId, Status = status };
    }

    private async Task<(string AmwResourceId, string EndpointBase)> ResolveTraceEndpointAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tenant,
        CancellationToken cancellationToken)
    {
        // This is a resource name, not an ARM ID or URL; separators must not add resource layers.
        if (workspace.IndexOfAny(['/', '\\', '?', '#']) >= 0)
        {
            throw new ArgumentException("--workspace must be an Azure Monitor Workspace resource name.", nameof(workspace));
        }

        var cacheKey = BuildEndpointCacheKey(subscription, resourceGroup, workspace, tenant);
        var cached = await cacheService
            .GetAsync<CachedTraceEndpoint>(CacheGroup, cacheKey, EndpointCacheDuration, cancellationToken)
            .ConfigureAwait(false);
        if (cached is not null)
        {
            return (cached.AmwResourceId, cached.Endpoint);
        }

        ResourceGroupResource resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription, resourceGroup, tenant, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
        ResourceIdentifier workspaceId = new($"{resourceGroupResource.Id}/providers/{MonitorAccountResourceType}/{workspace}");
        string amwResourceId = workspaceId.ToString();
        var armClient = await CreateArmClientWithApiVersionAsync(
            MonitorAccountResourceType, MonitorAccountApiVersion, tenant, cancellationToken).ConfigureAwait(false);

        var amw = await GetGenericResourceAsync(armClient, workspaceId, cancellationToken).ConfigureAwait(false);
        var endpoint = ExtractQueryEndpoint(amw.Data.Properties)
            ?? throw new InvalidOperationException(
                $"Azure Monitor Workspace '{workspace}' does not publish a data-plane query endpoint. The workspace "
                + $"may need an endpoints refresh (GET + PUT at api-version {MonitorAccountApiVersion}).");

        ValidateTraceEndpoint(AzureService, endpoint);

        var endpointBase = endpoint.TrimEnd('/');
        await cacheService
            .SetAsync(CacheGroup, cacheKey, new CachedTraceEndpoint(amwResourceId, endpointBase), EndpointCacheDuration, cancellationToken)
            .ConfigureAwait(false);

        return (amwResourceId, endpointBase);
    }

    private sealed record CachedTraceEndpoint(string AmwResourceId, string Endpoint);

    // Every field that selects a workspace must be in the key; CacheKeyBuilder length-prefixes each
    // part so adjacent values can't run together into a collision.
    internal static string BuildEndpointCacheKey(string subscription, string resourceGroup, string workspace, string? tenant)
        => CacheKeyBuilder.Build(TraceEndpointCacheKey, subscription, resourceGroup, workspace, tenant);

    /// <summary>Authorizes an ARM-advertised workspace base URL before acquiring a data-plane token.</summary>
    /// <exception cref="System.Security.SecurityException">The configured-cloud endpoint policy rejects the URL.</exception>
    /// <exception cref="InvalidOperationException">The URL is not a workspace base endpoint.</exception>
    internal static void ValidateTraceEndpoint(IAzureService azureService, string endpoint)
    {
        azureService.ValidateAzureServiceEndpoint(endpoint: endpoint, serviceType: "monitor-traces");
        // The shared policy permits the root domain; workspace URLs also need a resource host
        // and no authority/path modifiers that would change the appended request routes.
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri)
            || !uri.Host.EndsWith(TraceDataPlaneHostSuffix, StringComparison.OrdinalIgnoreCase)
            || uri.UserInfo.Length != 0 || !uri.IsDefaultPort
            || uri.Query.Length != 0 || uri.Fragment.Length != 0)
        {
            throw new InvalidOperationException("Trace query endpoint must be a workspace base URL without user info, a custom port, query, or fragment.");
        }
    }

    private async Task<TraceAccountInfo?> TryLoadContainerAsync(
        ArmClient armClient,
        WorkspaceRow row,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(row.ResourceId))
        {
            return null;
        }

        var containerId = new ResourceIdentifier($"{row.ResourceId}/{TraceContainerRelativePath}");
        try
        {
            var container = await GetGenericResourceAsync(armClient, containerId, cancellationToken).ConfigureAwait(false);
            var retention = ExtractRetentionSeconds(container.Data.Properties);
            var identifier = new ResourceIdentifier(row.ResourceId);
            return new TraceAccountInfo
            {
                Name = identifier.Name,
                ResourceId = row.ResourceId,
                ResourceGroup = identifier.ResourceGroupName ?? string.Empty,
                Location = row.Location,
                TraceDurationWindowInSeconds = retention,
            };
        }
        catch (Azure.RequestFailedException rfe) when (rfe.Status == 404)
        {
            // Absence of the container is the expected "not trace-enabled" signal. Anything else,
            // throttling above all, must surface: swallowing it would drop a real trace account
            // from the list and still report success.
            return null;
        }
    }

    private async Task<string> SendSubmitAsync(
        TraceSession session,
        string query,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int maxResults,
        CancellationToken cancellationToken)
    {
        var submitUrl = $"{session.EndpointBase}/traces:query?preserveQueryFieldNames=true&api-version={TraceDataPlaneApiVersion}";

        var body = new TraceQueryRequest
        {
            TraceAccountId = session.AmwResourceId,
            Query = query,
            Language = "KQL",
            TimeRange = new TraceQueryTimeRange
            {
                StartTime = FormatUtc(startTime),
                EndTime = FormatUtc(endTime),
            },
            Options = new TraceQueryOptionsBody { MaxResults = maxResults },
        };

        var content = RequestContent.Create(
            JsonSerializer.SerializeToUtf8Bytes(body, MonitorJsonContext.Default.TraceQueryRequest));

        using var response = await SendDataPlaneAsync(session, RequestMethod.Post, submitUrl, content, cancellationToken).ConfigureAwait(false);
        var payload = response.Content.ToString();

        if (response.Status != 202)
        {
            throw DataPlaneFailure("query submit", response);
        }

        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("operationId", out var opId) || opId.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"Trace query submit response missing operationId: {Truncate(payload)}");
        }

        return opId.GetString()!;
    }

    private async Task<TraceQueryResult> PollForResultsAsync(
        TraceSession session,
        string operationId,
        int pageSize,
        int maxRows,
        bool includeSpans,
        CancellationToken cancellationToken)
    {
        pageSize = Math.Max(1, pageSize);
        var pollUrl = $"{session.EndpointBase}/operations/{Uri.EscapeDataString(operationId)}/results?api-version={TraceDataPlaneApiVersion}&top={pageSize}&addEnrichmentFields=false&preserveQueryFieldNames=true&includeSpans={includeSpans}";
        var rows = new List<JsonElement>();
        TraceQueryPage? payload = null;
        var deadline = dateTimeProvider.UtcNow + MaxPollBudget;
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(MaxPollBudget);
        try
        {
            while (dateTimeProvider.UtcNow < deadline)
            {
                using var response = await SendDataPlaneAsync(session, RequestMethod.Get, pollUrl, null, budget.Token).ConfigureAwait(false);
                if (response.Status == 202)
                {
                    response.Headers.TryGetValue("Retry-After", out var retryAfter);
                    var delay = ClampPollDelay(retryAfter, deadline - dateTimeProvider.UtcNow, PollDelay);
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, budget.Token).ConfigureAwait(false);
                    }
                    continue;
                }

                if (response.Status != 200)
                {
                    throw DataPlaneFailure("query poll", response);
                }
                var page = JsonSerializer.Deserialize(response.Content.ToString(), MonitorJsonContext.Default.TraceQueryPage);
                if (page is not { Value: { } pageRows })
                {
                    throw new HttpRequestException("Trace query results must contain a 'value' array.", null, System.Net.HttpStatusCode.BadGateway);
                }
                var pageRowCount = pageRows.Count;
                rows.AddRange(pageRows);
                page.MergePropertiesFrom(payload);
                page.Value = rows;
                payload = page;
                var hasMoreResults = page.HasMoreResults;
                logger.LogDebug("Trace query {OperationId} received {Rows} rows; continuation: {HasMoreResults}.",
                    operationId, pageRowCount, hasMoreResults);
                if (!hasMoreResults)
                {
                    return CreateResult("Succeeded");
                }
                // Retain whole received pages: discarding an overshoot would lose consumed rows.
                if (rows.Count >= maxRows)
                {
                    return CreateResult("Running", outputLimitReached: true);
                }
            }
        }
        catch (OperationCanceledException) when (budget.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // The shared deadline also bounds an in-flight request or Retry-After wait.
        }
        catch (Exception ex) when (payload is not null && ex is HttpRequestException or RequestFailedException or JsonException or OperationCanceledException)
        {
            throw new TraceQueryException(CreateResult("Failed"), ex);
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException("Trace query results contain invalid JSON.", ex, System.Net.HttpStatusCode.BadGateway);
        }

        logger.LogWarning("Trace query {OperationId} results incomplete after {Budget}s poll budget.", operationId, MaxPollBudget.TotalSeconds);
        return CreateResult("Running");

        TraceQueryResult CreateResult(string status, bool outputLimitReached = false) => new()
        {
            OperationId = operationId,
            Status = status,
            Payload = payload is null ? null : JsonSerializer.SerializeToElement(payload, MonitorJsonContext.Default.TraceQueryPage),
            OutputLimitReached = outputLimitReached,
        };
    }

    private static WorkspaceRow ExtractWorkspaceRow(JsonElement item)
    {
        var id = item.TryGetProperty("id", out var i) ? i.GetString() : null;
        var location = item.TryGetProperty("location", out var l) ? l.GetString() : null;
        return new WorkspaceRow(id, location);
    }

    private static int? ExtractRetentionSeconds(BinaryData? properties)
    {
        using var doc = ParseProperties(properties);
        return doc?.RootElement is { ValueKind: JsonValueKind.Object } root
            && root.TryGetProperty("traceDurationWindowInSeconds", out var window)
            && window.TryGetInt32(out var value)
                ? value
                : null;
    }

    private static string? ExtractQueryEndpoint(BinaryData? properties)
    {
        using var doc = ParseProperties(properties);
        return doc?.RootElement is { ValueKind: JsonValueKind.Object } root
            && root.TryGetProperty("endpoints", out var endpoints)
            && endpoints.ValueKind == JsonValueKind.Object
            && endpoints.TryGetProperty("query", out var query)
            && query.ValueKind == JsonValueKind.String
                ? query.GetString()
                : null;
    }

    private static JsonDocument? ParseProperties(BinaryData? properties)
        => properties is null ? null : JsonDocument.Parse(properties);

    private static List<TraceFieldName> ParseFieldArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<TraceFieldName>(array.GetArrayLength());
        foreach (var item in array.EnumerateArray())
        {
            var name = item.TryGetProperty("name", out var n) ? n.GetString() : null;
            var dataType = item.TryGetProperty("dataType", out var d) ? d.GetString() : null;
            if (name is null || dataType is null)
            {
                continue;
            }

            List<TraceFieldName>? nested = null;
            if (item.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
            {
                nested = ParseFieldArray(item, "fields");
            }

            result.Add(new TraceFieldName { Name = name, DataType = dataType, Fields = nested });
        }

        return result;
    }

    internal static TimeSpan ClampPollDelay(string? retryAfter, TimeSpan remaining, TimeSpan fallback)
    {
        var delay = int.TryParse(retryAfter, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : fallback;

        return delay < remaining ? delay : remaining;
    }

    private static string FormatUtc(DateTimeOffset value)
    {
        // Trace data-plane strictly requires the trailing 'Z' form (e.g. 2026-07-24T18:03:00Z).
        // DateTimeOffset.ToString("o") emits +00:00 offset instead, which fails validation.
        return value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string Truncate(string content, int max = 1024)
        => content.Length <= max ? content : content[..max] + "…";

    private readonly record struct WorkspaceRow(string? ResourceId, string? Location);
}
