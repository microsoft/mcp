// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.RegularExpressions;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Models;
using Azure.ResourceManager.Search;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.KnowledgeBases;
using Azure.Search.Documents.KnowledgeBases.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Search.Services;

public sealed partial class SearchService(
    ISubscriptionService subscriptionService,
    ICacheService cacheService,
    ITenantService tenantService,
    ILogger<SearchService> logger)
    : BaseAzureService(tenantService), ISearchService
{
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<SearchService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string CacheGroup = "search";
    private const string SearchServicesCacheKey = "services";
    private static readonly TimeSpan s_cacheDurationServices = CacheDurations.ServiceData;
    private static readonly TimeSpan s_cacheDurationClients = CacheDurations.AuthenticatedClient;

    public async Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var cacheKey = string.IsNullOrEmpty(tenantId)
            ? CacheKeyBuilder.Build(SearchServicesCacheKey, subscription, _tenantService.CloudConfiguration.CloudType.ToString())
            : CacheKeyBuilder.Build(SearchServicesCacheKey, subscription, tenantId, _tenantService.CloudConfiguration.CloudType.ToString());

        var cachedServices = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationServices, cancellationToken);
        if (cachedServices != null)
        {
            return cachedServices;
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, retryPolicy, cancellationToken);
        var services = new List<string>();
        await foreach (var service in subscriptionResource.GetSearchServicesAsync(cancellationToken: cancellationToken))
        {
            if (service?.Data?.Name != null)
            {
                services.Add(service.Data.Name);
            }
        }

        await _cacheService.SetAsync(CacheGroup, cacheKey, services, s_cacheDurationServices, cancellationToken);

        return services;
    }

    public async Task<List<IndexInfo>> GetIndexDetails(
        string serviceName,
        string? indexName,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var indexes = new List<IndexInfo>();

        if (string.IsNullOrEmpty(indexName))
        {
            var searchClient = await GetSearchIndexClient(serviceName, retryPolicy, cancellationToken);
            await foreach (var index in searchClient.GetIndexesAsync(cancellationToken: cancellationToken))
            {
                indexes.Add(MapToIndexInfo(index));
            }
            return indexes;
        }
        else
        {
            var searchClient = await GetSearchIndexClient(serviceName, retryPolicy, cancellationToken);
            var index = await searchClient.GetIndexAsync(indexName, cancellationToken: cancellationToken);

            indexes.Add(MapToIndexInfo(index.Value));
        }

        return indexes;
    }

    public async Task<List<JsonElement>> QueryIndex(
        string serviceName,
        string indexName,
        string searchText,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(serviceName), serviceName),
            (nameof(indexName), indexName),
            (nameof(searchText), searchText));

        var searchClient = await GetSearchIndexClient(serviceName, retryPolicy, cancellationToken);
        var indexDefinition = await searchClient.GetIndexAsync(indexName, cancellationToken: cancellationToken);
        var client = searchClient.GetSearchClient(indexName);

        var options = new SearchOptions
        {
            IncludeTotalCount = true,
            Size = 20
        };

        var vectorFields = FindVectorFields(indexDefinition.Value);
        // TODO (alzimmer): this isn't useed and probably should be.
        var vectorizableFields = FindVectorizableFields(indexDefinition.Value, vectorFields);
        ConfigureSearchOptions(searchText, options, indexDefinition.Value, vectorFields);

        var searchResponse = await client.SearchAsync(searchText, SearchJsonContext.Default.JsonElement, options, cancellationToken: cancellationToken);

        return await ProcessSearchResults(searchResponse, cancellationToken);
    }

    public async Task<List<KnowledgeSourceInfo>> ListKnowledgeSources(
        string serviceName,
        string? knowledgeSourceName = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var sources = new List<KnowledgeSourceInfo>();
        var searchClient = await GetSearchIndexClient(serviceName, retryPolicy, cancellationToken);

        if (string.IsNullOrEmpty(knowledgeSourceName))
        {
            await foreach (var source in searchClient.GetKnowledgeSourcesAsync(cancellationToken: cancellationToken))
            {
                sources.Add(new(source.Name, source.GetType().Name, source.Description));
            }
        }
        else
        {
            var result = await searchClient.GetKnowledgeSourceAsync(knowledgeSourceName, cancellationToken: cancellationToken);
            if (result?.Value != null)
            {
                sources.Add(new(result.Value.Name, result.Value.GetType().Name, result.Value.Description));
            }
        }

        return sources;
    }

    public async Task<List<KnowledgeBaseInfo>> ListKnowledgeBases(
        string serviceName,
        string? knowledgeBaseName = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var bases = new List<KnowledgeBaseInfo>();
        var searchClient = await GetSearchIndexClient(serviceName, retryPolicy, cancellationToken);

        if (string.IsNullOrEmpty(knowledgeBaseName))
        {
            await foreach (var knowledgeBase in searchClient.GetKnowledgeBasesAsync(cancellationToken: cancellationToken))
            {
                bases.Add(new(knowledgeBase.Name, knowledgeBase.Description, [.. knowledgeBase.KnowledgeSources.Select(ks => ks.Name)]));
            }
        }
        else
        {
            var result = await searchClient.GetKnowledgeBaseAsync(knowledgeBaseName, cancellationToken: cancellationToken);
            if (result?.Value != null)
            {
                if (result.Value.Name.Equals(knowledgeBaseName, StringComparison.OrdinalIgnoreCase))
                {
                    bases.Add(new(result.Value.Name, result.Value.Description, [.. result.Value.KnowledgeSources.Select(ks => ks.Name)]));
                }
            }
        }

        return bases;
    }

    public async Task<string> RetrieveFromKnowledgeBase(
        string serviceName,
        string baseName,
        string? query,
        IEnumerable<(string role, string message)>? messages,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName), (nameof(baseName), baseName));

        var searchClient = await GetSearchIndexClient(serviceName, retryPolicy, cancellationToken);

        var knowledgeBase = await searchClient.GetKnowledgeBaseAsync(baseName, cancellationToken: cancellationToken);
        if (knowledgeBase?.Value == null)
        {
            throw new InvalidOperationException($"Knowledge base '{baseName}' not found in service '{serviceName}'.");
        }

        var clientOptions = AddDefaultPolicies(new SearchClientOptions());
        clientOptions.Transport = new HttpClientTransport(TenantService.GetClient());
        clientOptions.Audience = GetSearchAudience();
        ConfigureRetryPolicy(clientOptions, retryPolicy);

        var knowledgeBaseClient = new KnowledgeBaseRetrievalClient(searchClient.Endpoint, baseName, await GetCredential(cancellationToken: cancellationToken), clientOptions);
        var useMinimalReasoning = knowledgeBase.Value.RetrievalReasoningEffort is KnowledgeRetrievalMinimalReasoningEffort;
        var request = BuildKnowledgeBaseRetrievalRequest(useMinimalReasoning, query, messages);

        var results = await knowledgeBaseClient.RetrieveAsync(request, cancellationToken: cancellationToken);

        var response = results.GetRawResponse().Content ?? throw new InvalidOperationException("Response had no content");
        return await ProcessRetrieveResponse(response.ToStream());
    }

    internal static KnowledgeBaseRetrievalRequest BuildKnowledgeBaseRetrievalRequest(
        bool useMinimalReasoning,
        string? query,
        IEnumerable<(string role, string message)>? messages)
    {
        var request = new KnowledgeBaseRetrievalRequest();

        if (useMinimalReasoning)
        {
            var intent = messages != null && messages.Any()
                ? string.Join("\n", messages.Select(m => m.message))
                : query ?? string.Empty;

            request.Intents.Add(new KnowledgeRetrievalSemanticIntent(intent));
            return request;
        }

        if (messages != null && messages.Any())
        {
            foreach ((string role, string message) in messages)
            {
                request.Messages.Add(new([new KnowledgeBaseMessageTextContent(message)]) { Role = role });
            }

            return request;
        }

        request.Messages.Add(new([new KnowledgeBaseMessageTextContent(query ?? string.Empty)]) { Role = "user" });
        return request;
    }

    internal static async Task<string> ProcessRetrieveResponse(Stream responseStream)
    {
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
            {
                if (prop.Name is "response" or "references")
                {
                    prop.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static List<string> FindVectorFields(SearchIndex indexDefinition)
    {
        return [.. indexDefinition.Fields
                    .Where(f => f.VectorSearchDimensions.HasValue)
                    .Select(f => f.Name)];
    }

    private static List<string> FindVectorizableFields(SearchIndex indexDefinition, List<string> vectorFields)
    {
        var vectorizableFields = new List<string>();

        if (indexDefinition.VectorSearch?.Profiles == null || indexDefinition.VectorSearch.Algorithms == null)
        {
            return vectorizableFields;
        }

        foreach (var field in indexDefinition.Fields)
        {
            if (vectorFields.Contains(field.Name) && !string.IsNullOrEmpty(field.VectorSearchProfileName))
            {
                var profile = indexDefinition.VectorSearch.Profiles
                    .FirstOrDefault(p => p.Name == field.VectorSearchProfileName);

                if (profile != null)
                {
                    if (!string.IsNullOrEmpty(profile.VectorizerName))
                    {
                        vectorizableFields.Add(field.Name);
                    }
                }
            }
        }

        return vectorizableFields;
    }

    private async Task<SearchIndexClient> GetSearchIndexClient(string serviceName, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken = default)
    {
        ValidateServiceName(serviceName);
        var key = CacheKeyBuilder.Build(SearchServicesCacheKey, serviceName, _tenantService.CloudConfiguration.CloudType.ToString());
        var searchClient = await _cacheService.GetAsync<SearchIndexClient>(CacheGroup, key, s_cacheDurationClients, cancellationToken);
        if (searchClient == null)
        {
            var credential = await GetCredential(cancellationToken);

            var clientOptions = AddDefaultPolicies(new SearchClientOptions());
            clientOptions.Transport = new HttpClientTransport(TenantService.GetClient());
            clientOptions.Audience = GetSearchAudience();
            ConfigureRetryPolicy(clientOptions, retryPolicy);

            var endpoint = new Uri(GetSearchEndpoint(serviceName));
            searchClient = new SearchIndexClient(endpoint, credential, clientOptions);
            await _cacheService.SetAsync(CacheGroup, key, searchClient, s_cacheDurationClients, cancellationToken);
        }
        return searchClient;
    }

    private static void ConfigureSearchOptions(string q, SearchOptions options, SearchIndex indexDefinition, List<string> vectorFields)
    {
        List<string> selectedFields = [.. indexDefinition.Fields
                                                         .Where(f => f.IsHidden == false && !vectorFields.Contains(f.Name))
                                                         .Select(f => f.Name)];
        foreach (var field in selectedFields)
        {
            options.Select.Add(field);
        }

        options.VectorSearch = new VectorSearchOptions();
        foreach (var vf in vectorFields)
        {
            options.VectorSearch.Queries.Add(new VectorizableTextQuery(q) { Fields = { vf }, KNearestNeighborsCount = 50 });
        }
    }

    private static async Task<List<JsonElement>> ProcessSearchResults(Response<SearchResults<JsonElement>> searchResponse, CancellationToken cancellationToken)
    {
        var results = new List<JsonElement>();
        await foreach (var result in searchResponse.Value.GetResultsAsync().WithCancellation(cancellationToken))
        {
            results.Add(result.Document);
        }
        return results;
    }

    private static void ConfigureRetryPolicy(SearchClientOptions options, RetryPolicyOptions? retryPolicy)
    {
        if (retryPolicy != null)
        {
            options.Retry.MaxRetries = retryPolicy.MaxRetries;
            options.Retry.Mode = retryPolicy.Mode;
            options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
        }
    }

    private static IndexInfo MapToIndexInfo(SearchIndex index)
        => new(index.Name, index.Description, [.. index.Fields.Select(MapToFieldInfo)]);

    private static FieldInfo MapToFieldInfo(SearchField field)
        => new(field.Name, field.Type.ToString(), field.IsKey, field.IsSearchable, field.IsFilterable, field.IsSortable,
            field.IsFacetable, field.IsHidden != true);

    // Service name pattern: lowercase letters, digits, hyphens; 2-60 chars; must start and end with alphanumeric.
    // Consecutive dashes must be checked separately as the regex pattern does not prevent them.
    [GeneratedRegex(@"^[a-z0-9][a-z0-9\-]{0,58}[a-z0-9]$")]
    private static partial Regex ServiceNamePattern();

    internal static void ValidateServiceName(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));
        }

        if (!ServiceNamePattern().IsMatch(serviceName))
        {
            throw new ArgumentException(
                "Service name must only contain lowercase letters, digits, or dashes, cannot start or end with dashes, and must be between 2 and 60 characters in length.", nameof(serviceName));
        }

        if (serviceName[1] == '-')
        {
            throw new ArgumentException(
                "Service name must not have a dash as its second character.", nameof(serviceName));
        }

        if (serviceName.Contains("--", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Service name cannot contain consecutive dashes.", nameof(serviceName));
        }

        if (string.Equals(serviceName, "ext", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Service name 'ext' is reserved and cannot be used.", nameof(serviceName));
        }
    }

    private string GetSearchEndpoint(string serviceName)
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => $"https://{serviceName}.search.windows.net",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => $"https://{serviceName}.search.azure.cn",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => $"https://{serviceName}.search.azure.us",
            _ => $"https://{serviceName}.search.windows.net"
        };
    }

    private SearchAudience GetSearchAudience()
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => SearchAudience.AzurePublicCloud,
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => SearchAudience.AzureChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => SearchAudience.AzureGovernment,
            _ => SearchAudience.AzurePublicCloud
        };
    }
}
