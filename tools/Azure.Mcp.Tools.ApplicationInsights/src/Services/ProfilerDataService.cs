// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ApplicationInsights.Commands;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

/// <summary>
/// A simple client to call Profiler dataplane. This is not a full fledged client.
/// Expect to be replaced by Azure SDK in future.
/// </summary>
public class ProfilerDataService(ILogger<ProfilerDataService> logger, IAzureService azureService)
    : BaseAzureService(azureService), IProfilerDataService
{

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

        return GetInsightsImpAsync(appIds, startDateTimeUtc, endDateTimeUtc, cancellationToken);
    }

    private async Task<IEnumerable<JsonNode>> GetInsightsImpAsync(IEnumerable<Guid> appIds, DateTime? startDateTimeUtc, DateTime? endDateTimeUtc, CancellationToken cancellationToken)
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

        return await JsonSerializer.DeserializeAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            ApplicationInsightsJsonContext.Default.IEnumerableJsonNode,
            cancellationToken).ConfigureAwait(false) ?? [];
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path, IDictionary<string, string>? queries, string apiVersion, string? clientRequestId, HttpContent? httpContent, IDictionary<string, IEnumerable<string>>? additionalHeaders, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder = new(GetDiagnosticServiceEndpoint())
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
            GetDiagnosticServicesScope()
        };
        string clientRequestIdLocal = clientRequestId ?? Guid.NewGuid().ToString();
        TokenRequestContext tokenRequestContext = new(scopes, clientRequestIdLocal);
        TokenCredential tokenCredential = await GetCredential(null, cancellationToken).ConfigureAwait(false);
        AccessToken accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new("Bearer", accessToken.Token);
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

    /// <summary>
    /// Call the given path on the data plane, passing the tenant ID and object ID of the
    /// caller in headers. Posted with content.
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="queries">Optional queries to append to the path.</param>
    /// <param name="apiVersion">The API version to use.</param>
    /// <param name="clientRequestId">Optional client request ID.</param>
    /// <param name="httpContent">The content of the incoming request.</param>
    /// <param name="additionalHeaders">Additional headers to be added to the request</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    internal async ValueTask<HttpResponseMessage> PostAsync(string path, IDictionary<string, string>? queries, string apiVersion, string? clientRequestId, HttpContent? httpContent, IDictionary<string, IEnumerable<string>>? additionalHeaders, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = await CreateRequestAsync(HttpMethod.Post, path, queries, apiVersion, clientRequestId, httpContent, additionalHeaders, cancellationToken);
        var client = AzureService.GetClient();
        return await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
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

    private string GetDiagnosticServiceEndpoint()
    {
        return AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => "https://dataplane.diagnosticservices.azure.com",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => "https://dataplane.diagnosticservices.azure.cn",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => "https://dataplane.diagnosticservices.azure.us",
            _ => "https://dataplane.diagnosticservices.azure.com"
        };
    }

    private string GetDiagnosticServicesScope()
    {
        return AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => "api://dataplane.diagnosticservices.azure.com/.default",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => "api://dataplane.diagnosticservices.azure.cn/.default",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => "api://dataplane.diagnosticservices.azure.us/.default",
            _ => "api://dataplane.diagnosticservices.azure.com/.default"
        };
    }
}
