// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Monitor.Services.Traces;

/// <summary>
/// Backing service for the <c>monitor traces</c> sub-namespace. A trace account is an Azure Monitor
/// Workspace enabled for Trace Store by creating its <c>traceContainers/default</c> sub-resource.
/// </summary>
public sealed class MonitorTracesService(
    IAzureService azureService,
    ICacheService cacheService,
    ILogger<MonitorTracesService> logger)
    : BaseAzureResourceService(azureService), IMonitorTracesService
{
    internal const string TraceContainerResourceType = "Microsoft.Monitor/accounts/traceContainers";
    internal const string TraceContainerApiVersion = "2024-10-03-preview";
    internal const string TraceContainerRelativePath = "traceContainers/default";

    // 2025-05-03-preview is the first api-version that publishes properties.endpoints.query.
    internal const string MonitorAccountResourceType = "Microsoft.Monitor/accounts";
    internal const string MonitorAccountApiVersion = "2025-05-03-preview";

    // Kept per-cloud so no public-cloud hostname is baked into code running in a sovereign cloud.
    internal const string TraceDataPlaneScope = "https://data.monitor.azure.com/.default";
    internal const string TraceDataPlaneHostSuffix = ".data.monitor.azure.com";
    internal const string TraceDataPlaneApiVersion = "2026-01-10-preview";

    // Bounded so a slow query hands back an operationId instead of blocking the MCP transport.
    private static readonly TimeSpan PollDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxPollBudget = TimeSpan.FromSeconds(180);

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
        string account,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var (amwResourceId, endpointBase) = await ResolveTraceEndpointAsync(
            subscription, account, tenant, cancellationToken).ConfigureAwait(false);
        var token = await GetDataPlaneTokenAsync(tenant, cancellationToken).ConfigureAwait(false);

        var options = AddDefaultPolicies(new MonitorTracesClientOptions());
        // Per-attempt cap for a slow submit or poll.
        options.Retry.NetworkTimeout = MaxPollBudget;
        options.Transport = new HttpClientTransport(AzureService.GetClient());

        return new TraceSession(amwResourceId, endpointBase, token, HttpPipelineBuilder.Build(options));
    }

    private static async Task<Response> SendDataPlaneAsync(
        TraceSession session,
        RequestMethod method,
        string url,
        RequestContent? content,
        CancellationToken cancellationToken)
    {
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

    public async Task<List<TraceAccountInfo>> ListAccountsAsync(
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
            return [];
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

        return [.. probed.OfType<TraceAccountInfo>()];
    }

    public async Task<TraceFieldNames> ListFieldNamesAsync(
        string subscription,
        string account,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(account), account));

        var session = await OpenSessionAsync(subscription, account, tenant, cancellationToken).ConfigureAwait(false);
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
        string account,
        string? query,
        string? operationId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int maxResults,
        bool includeSpans,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(account), account));

        if (string.IsNullOrWhiteSpace(query) == string.IsNullOrWhiteSpace(operationId))
        {
            throw new ArgumentException($"Supply exactly one of {nameof(query)} or {nameof(operationId)}.", nameof(query));
        }

        var session = await OpenSessionAsync(subscription, account, tenant, cancellationToken).ConfigureAwait(false);

        operationId ??= await SendSubmitAsync(
            session, query!, startTime, endTime, maxResults, cancellationToken).ConfigureAwait(false);

        return await PollUntilCompleteAsync(
            session, operationId, maxResults, includeSpans, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TraceQueryCancelResult> CancelQueryAsync(
        string subscription,
        string account,
        string operationId,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(account), account),
            (nameof(operationId), operationId));

        var session = await OpenSessionAsync(subscription, account, tenant, cancellationToken).ConfigureAwait(false);
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
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("status", out var s) &&
                s.ValueKind == JsonValueKind.String)
            {
                status = s.GetString() ?? "unknown";
            }
        }
        catch (JsonException)
        {
            // Cancel doesn't guarantee a JSON body.
        }

        return new TraceQueryCancelResult { OperationId = operationId, Status = status };
    }

    private async Task<(string AmwResourceId, string EndpointBase)> ResolveTraceEndpointAsync(
        string subscription,
        string account,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var cacheKey = BuildEndpointCacheKey(subscription, account, tenant);
        var cached = await cacheService
            .GetAsync<CachedTraceEndpoint>(CacheGroup, cacheKey, EndpointCacheDuration, cancellationToken)
            .ConfigureAwait(false);
        if (cached is not null)
        {
            return (cached.AmwResourceId, cached.Endpoint);
        }

        var amwResourceId = await ResolveAmwResourceIdAsync(subscription, account, tenant, cancellationToken).ConfigureAwait(false);
        var armClient = await CreateArmClientWithApiVersionAsync(
            MonitorAccountResourceType, MonitorAccountApiVersion, tenant, cancellationToken).ConfigureAwait(false);

        var amw = await GetGenericResourceAsync(armClient, new ResourceIdentifier(amwResourceId), cancellationToken).ConfigureAwait(false);
        var endpoint = ExtractQueryEndpoint(amw.Data.Properties)
            ?? throw new InvalidOperationException(
                $"Trace account '{account}' does not publish a data-plane query endpoint. The Azure Monitor Workspace "
                + $"may need an endpoints refresh (GET + PUT at api-version {MonitorAccountApiVersion}).");

        var expectedHostSuffix = GetTraceCloudSettings().HostSuffix;
        if (!IsAcceptableTraceHost(endpoint, expectedHostSuffix))
        {
            throw new InvalidOperationException(
                $"Trace account '{account}' returned query endpoint '{endpoint}', which is not an HTTPS "
                + $"'*{expectedHostSuffix}' host — refusing to send a data-plane token to an unexpected host.");
        }

        var endpointBase = endpoint.TrimEnd('/');
        await cacheService
            .SetAsync(CacheGroup, cacheKey, new CachedTraceEndpoint(amwResourceId, endpointBase), EndpointCacheDuration, cancellationToken)
            .ConfigureAwait(false);

        return (amwResourceId, endpointBase);
    }

    private sealed record CachedTraceEndpoint(string AmwResourceId, string Endpoint);

    // Every field that selects a workspace must be in the key; CacheKeyBuilder length-prefixes each
    // part so adjacent values can't run together into a collision.
    internal static string BuildEndpointCacheKey(string subscription, string account, string? tenant)
        => CacheKeyBuilder.Build(TraceEndpointCacheKey, subscription, account, tenant);

    private async Task<string> ResolveAmwResourceIdAsync(
        string subscription,
        string account,
        string? tenant,
        CancellationToken cancellationToken)
    {
        // An ARM id is validated for shape, then looked up through the SAME subscription-scoped ARG
        // query, constrained to its own resource group. An id belonging to another subscription simply
        // isn't found, so --account can't be used to reach past the resolved --subscription — and
        // keeping the resource group means two workspaces sharing a name stay distinguishable.
        var accountName = account;
        string? accountResourceGroup = null;
        if (account.StartsWith('/'))
        {
            if (!TryParseAmwArmId(account, out var parsed))
            {
                throw new ArgumentException(
                    "--account must be an Azure Monitor Workspace name, or an ARM id of the form "
                    + "'/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Monitor/accounts/{name}'.",
                    nameof(account));
            }

            accountName = parsed.Name;
            accountResourceGroup = parsed.ResourceGroupName;
        }

        var matches = await ExecuteResourceQueryAsync(
            MonitorAccountResourceType,
            accountResourceGroup,
            subscription,
            item => item.TryGetProperty("id", out var i) ? i.GetString() ?? string.Empty : string.Empty,
            additionalFilter: $"name =~ '{EscapeKqlString(accountName)}'",
            tenant: tenant,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var ids = matches.Results.Where(id => !string.IsNullOrEmpty(id)).ToList();
        return SelectSingleAccountId(ids, accountName, subscription);
    }

    // A bare name isn't unique across resource groups; returning the first match would silently
    // query the wrong workspace.
    internal static string SelectSingleAccountId(IReadOnlyList<string> ids, string accountName, string subscription)
    {
        if (ids.Count == 0)
        {
            throw new KeyNotFoundException($"No Azure Monitor Workspace named '{accountName}' in subscription '{subscription}'.");
        }

        if (ids.Count > 1)
        {
            throw new ArgumentException(
                $"Subscription '{subscription}' has {ids.Count} Azure Monitor Workspaces named '{accountName}' "
                + $"({string.Join(", ", ids)}). Pass the full resource id to --account to choose one.",
                nameof(accountName));
        }

        return ids[0];
    }

    // Canonical Microsoft.Monitor/accounts ids only, so a caller can't smuggle an extra
    // ?api-version= or a fake provider segment past the check.
    internal static bool TryParseAmwArmId(string value, out ResourceIdentifier parsed)
    {
        parsed = null!;

        if (value.Contains('?', StringComparison.Ordinal) || value.Contains('#', StringComparison.Ordinal))
        {
            return false;
        }

        if (!ResourceIdentifier.TryParse(value, out var id) || id is null)
        {
            return false;
        }

        // An AMW id has exactly one resource layer under a resource group.
        if (id.Parent is null
            || !id.Parent.ResourceType.ToString().Equals("Microsoft.Resources/resourceGroups", StringComparison.OrdinalIgnoreCase)
            || !id.ResourceType.Namespace.Equals("Microsoft.Monitor", StringComparison.OrdinalIgnoreCase)
            || !id.ResourceType.Type.Equals("accounts", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(id.SubscriptionId)
            || string.IsNullOrWhiteSpace(id.ResourceGroupName)
            || string.IsNullOrWhiteSpace(id.Name))
        {
            return false;
        }

        parsed = id;
        return true;
    }

    private async Task<string> GetDataPlaneTokenAsync(string? tenant, CancellationToken cancellationToken)
    {
        var credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);
        var token = await credential.GetTokenAsync(
            new TokenRequestContext([GetTraceCloudSettings().Scope]),
            cancellationToken).ConfigureAwait(false);
        return token.Token;
    }

    // Only public cloud publishes these; failing loudly beats minting a public-cloud audience
    // against a sovereign endpoint, which surfaces as an opaque 401.
    private (string Scope, string HostSuffix) GetTraceCloudSettings() =>
        AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                (TraceDataPlaneScope, TraceDataPlaneHostSuffix),
            var cloud => throw new NotSupportedException(
                $"Azure Monitor Trace Store has no published data-plane audience for {cloud}. "
                + "The 'monitor traces' commands are Azure public cloud only."),
        };

    // A data-plane token is about to be sent to whatever host ARM named, so verify it first.
    internal static bool IsAcceptableTraceHost(string endpoint, string expectedHostSuffix) =>
        Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.Host.EndsWith(expectedHostSuffix, StringComparison.OrdinalIgnoreCase);

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

    private static async Task<string> SendSubmitAsync(
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

    private async Task<TraceQueryResult> PollUntilCompleteAsync(
        TraceSession session,
        string operationId,
        int maxResults,
        bool includeSpans,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Max(1, maxResults);
        var pollUrl = $"{session.EndpointBase}/operations/{Uri.EscapeDataString(operationId)}/results?api-version={TraceDataPlaneApiVersion}&pageSize={pageSize}&addEnrichmentFields=false&preserveQueryFieldNames=true&includeSpans={includeSpans}";

        var deadline = DateTime.UtcNow + MaxPollBudget;
        while (DateTime.UtcNow < deadline)
        {
            using var response = await SendDataPlaneAsync(session, RequestMethod.Get, pollUrl, null, cancellationToken).ConfigureAwait(false);

            if (response.Status == 202)
            {
                response.Headers.TryGetValue("Retry-After", out var retryAfter);
                var delay = ClampPollDelay(retryAfter, deadline - DateTime.UtcNow, PollDelay);
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }

                continue;
            }

            if (response.Status == 200)
            {
                using var doc = JsonDocument.Parse(response.Content.ToString());
                return new TraceQueryResult
                {
                    OperationId = operationId,
                    Status = "Succeeded",
                    Payload = doc.RootElement.Clone(),
                };
            }

            throw DataPlaneFailure("query poll", response);
        }

        logger.LogWarning("Trace query {OperationId} still running after {Budget}s poll budget.", operationId, MaxPollBudget.TotalSeconds);
        return new TraceQueryResult
        {
            OperationId = operationId,
            Status = "Running",
            Payload = null,
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

    // Retry-After is the service's number, not ours: honour it only up to the budget we promised,
    // otherwise one header parks the transport far longer. A non-positive or non-numeric value
    // (the header also permits an HTTP-date) falls back rather than throwing out of Task.Delay.
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
