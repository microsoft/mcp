using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Web;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.ApplicationInsights.Commands;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

/// <summary>
/// A simple client to call Profiler dataplane. This is not a full fledged client.
/// Expect to be replaced by Azure SDK in future.
/// </summary>
public class ProfilerDataService(
    IHttpClientService httpClientService,
    ILogger<ProfilerDataService> logger,
    ITenantService? tenantService = null, ILoggerFactory? loggerFactory = null)
    : BaseAzureService(tenantService, loggerFactory), IProfilerDataService
{
    private const string Endpoint = "https://dataplane.diagnosticservices.azure.com/";
    private const string DefaultScope = "api://dataplane.diagnosticservices.azure.com/.default";

    private readonly HttpClient _httpClient = httpClientService.CreateClient(new Uri(Endpoint));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<JsonNode>> GetInsightsAsync(IEnumerable<ResourceIdentifier> resourceIds, DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resourceIds);

        if (!resourceIds.Any())
        {
            throw new ArgumentException($"'{nameof(resourceIds)}' cannot be empty.", nameof(resourceIds));
        }

        List<Guid> appIds = [];
        foreach (ResourceIdentifier resourceId in resourceIds)
        {
            appIds.Add(await ResolveAppIdAsync(resourceId, cancellationToken).ConfigureAwait(false));
        }

        return await GetInsightsAsync(appIds, startDateTimeUtc, endDateTimeUtc, cancellationToken).ConfigureAwait(false);
    }

    private Task<IEnumerable<JsonNode>> GetInsightsAsync(
        IEnumerable<Guid> appIds,
        DateTime? startDateTimeUtc = null,
        DateTime? endDateTimeUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(appIds);

        if (!appIds.Any())
        {
            throw new ArgumentException($"'{nameof(appIds)}' cannot be empty.", nameof(appIds));
        }

        return GetInsightsImpAsync(appIds, startDateTimeUtc, endDateTimeUtc, ApplicationInsightsJsonContext.Default.IEnumerableJsonNode, cancellationToken);
    }

    private async Task<IEnumerable<T>> GetInsightsImpAsync<T>(IEnumerable<Guid> appIds, DateTime? startDateTimeUtc, DateTime? endDateTimeUtc, JsonTypeInfo<IEnumerable<T>> jsonTypeInfo, CancellationToken cancellationToken)
    {
        endDateTimeUtc ??= DateTime.UtcNow;
        startDateTimeUtc ??= endDateTimeUtc.Value.AddDays(-1);

        BulkAppsPostBody bulkAppsPostBody = new()
        {
            Apps = appIds
        };

        string path = $"api/apps/bulk/insights/rollups";
        Dictionary<string, string> queries = new()
        {
            ["startTime"] = startDateTimeUtc.Value.ToString("o"),
            ["endTime"] = endDateTimeUtc.Value.ToString("o"),
        };

        using JsonContent appsPostBody = JsonContent.Create(bulkAppsPostBody, ApplicationInsightsJsonContext.Default.BulkAppsPostBody, mediaType: MediaTypeHeaderValue.Parse("application/json"));
        using HttpResponseMessage response = await PostAsync(path, queries, apiVersion: "2025-01-07-preview", clientRequestId: null, appsPostBody, additionalHeaders: null, cancellationToken: cancellationToken).ConfigureAwait(false);

        IEnumerable<T>? result = await ReadAsAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            jsonTypeInfo,
            cancellationToken).ConfigureAwait(false);

        return result ?? [];
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path, IDictionary<string, string>? queries, string apiVersion, string? clientRequestId, HttpContent? httpContent, IDictionary<string, IEnumerable<string>>? additionalHeaders, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder = new(Endpoint)
        {
            Path = path
        };

        NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
        if (queries is not null)
        {
            foreach (var kvp in queries)
            {
                query[kvp.Key] = kvp.Value;
            }
        }

        query["api-version"] = apiVersion;
        uriBuilder.Query = query.ToString() ?? string.Empty;

        HttpRequestMessage request = new(method, uriBuilder.Uri);

        var scopes = new string[]
        {
            DefaultScope
        };
        string clientRequestIdLocal = clientRequestId ?? Guid.NewGuid().ToString();
        TokenRequestContext tokenRequestContext = new(scopes, clientRequestIdLocal);
        TokenCredential tokenCredential = await GetCredential().ConfigureAwait(false);
        AccessToken accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        request.Headers.Add("x-ms-client-request-id", clientRequestIdLocal);

        if (additionalHeaders is not null)
        {
            foreach (var header in additionalHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (httpContent is not null)
        {
            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                throw new ArgumentException($"HTTP method '{method}' does not support content.", nameof(method));
            }

            request.Content = httpContent;
        }

        return request;
    }

    private ValueTask<T?> ReadAsAsync<T>(Stream stream, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (jsonTypeInfo is null)
        {
            throw new ArgumentNullException(nameof(jsonTypeInfo));
        }

        return JsonSerializer.DeserializeAsync(stream, jsonTypeInfo, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Call the given path on the data plane, passing the tenant ID and object ID of the
    /// caller in headers. Posted with content.
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="queries">Optional queries to append to the path.</param>
    /// <param name="clientRequestId">Optional client request ID.</param>
    /// <param name="httpContent">The content of the incoming request.</param>
    /// <param name="additionalHeaders">Additional headers to be added to the request</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    internal async ValueTask<HttpResponseMessage> PostAsync(string path, IDictionary<string, string>? queries, string apiVersion, string? clientRequestId, HttpContent? httpContent, IDictionary<string, IEnumerable<string>>? additionalHeaders, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = await CreateRequestAsync(HttpMethod.Post, path, queries, apiVersion, clientRequestId, httpContent, additionalHeaders, cancellationToken);
        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> ResolveAppIdAsync(ResourceIdentifier resourceId, CancellationToken cancellationToken, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantId, retryPolicy).ConfigureAwait(false);

        ApplicationInsightsComponentResource applicationInsightsComponentResource = armClient.GetApplicationInsightsComponentResource(resourceId);

        if (applicationInsightsComponentResource is null)
        {
            throw new ArgumentException($"Resource with ID '{resourceId}' is not an Application Insights component.", nameof(resourceId));
        }

        applicationInsightsComponentResource = applicationInsightsComponentResource.Get(cancellationToken);

        string appId = applicationInsightsComponentResource.Data.AppId;
        _logger.LogInformation("Resolving appId: {resourceId} => {appId}", resourceId, appId);
        return Guid.Parse(appId);
    }
}
