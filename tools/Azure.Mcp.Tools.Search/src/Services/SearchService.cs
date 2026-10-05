// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.ResourceManager.Search;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.KnowledgeBases;
using Azure.Search.Documents.KnowledgeBases.Models;
using Azure.Search.Documents.Models;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Search.Services;

public sealed partial class SearchService(ICacheService cacheService, IAzureService azureService)
    : BaseAzureService(azureService), ISearchService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CacheGroup = "search";
    private const string SearchServicesCacheKey = "services";
    private static readonly TimeSpan s_cacheDurationServices = CacheDurations.ServiceData;
    private static readonly TimeSpan s_cacheDurationClients = CacheDurations.AuthenticatedClient;

    public async Task<List<string>> ListServices(
        string subscription,
        string? resourceGroup = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgCacheKey = string.IsNullOrEmpty(tenantId)
                ? CacheKeyBuilder.Build(SearchServicesCacheKey, subscription, resourceGroup, AzureService.CloudConfiguration.CloudType.ToString())
                : CacheKeyBuilder.Build(SearchServicesCacheKey, subscription, resourceGroup, tenantId, AzureService.CloudConfiguration.CloudType.ToString());

            var cachedRgServices = await _cacheService.GetAsync<List<string>>(CacheGroup, rgCacheKey, s_cacheDurationServices, cancellationToken);
            if (cachedRgServices != null)
            {
                return cachedRgServices;
            }

            var subForRg = await AzureService.GetSubscription(subscription, tenantId, cancellationToken: cancellationToken);
            var rgResource = (await subForRg.GetResourceGroupAsync(resourceGroup, cancellationToken)).Value;
            var rgServices = new List<string>();
            await foreach (var service in rgResource.GetSearchServices().GetAllAsync(cancellationToken: cancellationToken))
            {
                if (service?.Data?.Name != null)
                {
                    rgServices.Add(service.Data.Name);
                }
            }

            await _cacheService.SetAsync(CacheGroup, rgCacheKey, rgServices, s_cacheDurationServices, cancellationToken);
            return rgServices;
        }

        var cacheKey = string.IsNullOrEmpty(tenantId)
            ? CacheKeyBuilder.Build(SearchServicesCacheKey, subscription, AzureService.CloudConfiguration.CloudType.ToString())
            : CacheKeyBuilder.Build(SearchServicesCacheKey, subscription, tenantId, AzureService.CloudConfiguration.CloudType.ToString());

        var cachedServices = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationServices, cancellationToken);
        if (cachedServices != null)
        {
            return cachedServices;
        }

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenantId, cancellationToken: cancellationToken);
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
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var indexes = new List<IndexInfo>();

        if (string.IsNullOrEmpty(indexName))
        {
            var searchClient = await GetSearchIndexClient(serviceName, cancellationToken);
            await foreach (var index in searchClient.GetIndexesAsync(cancellationToken: cancellationToken))
            {
                indexes.Add(MapToIndexInfo(index));
            }
            return indexes;
        }
        else
        {
            var searchClient = await GetSearchIndexClient(serviceName, cancellationToken);
            var index = await searchClient.GetIndexAsync(indexName, cancellationToken: cancellationToken);

            indexes.Add(MapToIndexInfo(index.Value));
        }

        return indexes;
    }

    public async Task<List<JsonElement>> QueryIndex(
        string serviceName,
        string indexName,
        string searchText,
        IndexQueryType? queryType = null,
        string? semanticConfiguration = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(serviceName), serviceName),
            (nameof(indexName), indexName),
            (nameof(searchText), searchText));

        var searchClient = await GetSearchIndexClient(serviceName, cancellationToken);
        var indexDefinition = await searchClient.GetIndexAsync(indexName, cancellationToken: cancellationToken);
        var client = searchClient.GetSearchClient(indexName);

        var options = new SearchOptions
        {
            IncludeTotalCount = true,
            Size = 20
        };

        var vectorFields = FindVectorFields(indexDefinition.Value);
        // TODO (alzimmer): this isn't used and probably should be.
        var vectorizableFields = FindVectorizableFields(indexDefinition.Value, vectorFields);
        ConfigureSearchOptions(searchText, options, indexDefinition.Value, vectorFields);
        ConfigureQueryType(options, indexDefinition.Value, queryType, semanticConfiguration);

        var searchResponse = await client.SearchAsync(searchText, SearchJsonContext.Default.JsonElement, options, cancellationToken: cancellationToken);

        return await ProcessSearchResults(searchResponse, cancellationToken);
    }

    public async Task<List<KnowledgeSourceInfo>> ListKnowledgeSources(
        string serviceName,
        string? knowledgeSourceName = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var sources = new List<KnowledgeSourceInfo>();
        var searchClient = await GetSearchIndexClient(serviceName, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var bases = new List<KnowledgeBaseInfo>();
        var searchClient = await GetSearchIndexClient(serviceName, cancellationToken);

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
                if (result.Value.Name.Equals(knowledgeBaseName, StringComparisons.ResourceName))
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
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName), (nameof(baseName), baseName));

        var searchClient = await GetSearchIndexClient(serviceName, cancellationToken);

        var knowledgeBase = await searchClient.GetKnowledgeBaseAsync(baseName, cancellationToken: cancellationToken);
        if (knowledgeBase?.Value == null)
        {
            throw new InvalidOperationException($"Knowledge base '{baseName}' not found in service '{serviceName}'.");
        }

        var clientOptions = AddDefaultPolicies(new SearchClientOptions());
        clientOptions.Transport = new HttpClientTransport(AzureService.GetClient());
        clientOptions.Audience = GetSearchAudience();

        // SearchIndexClient manages service-level resources and knowledge-base metadata, while
        // KnowledgeBaseRetrievalClient executes retrieval for one knowledge base. Both target the same validated
        // Search service endpoint. This service only caches SearchIndexClient instances created from the validated
        // endpoint below, so reusing its Endpoint here preserves that authorization; baseName affects the
        // SDK-managed request path, not the destination host.
        var knowledgeBaseClient = new KnowledgeBaseRetrievalClient(searchClient.Endpoint, baseName, await GetCredential(null, cancellationToken), clientOptions);
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

    private async Task<SearchIndexClient> GetSearchIndexClient(string serviceName, CancellationToken cancellationToken = default)
    {
        ValidateServiceName(serviceName);
        var key = CacheKeyBuilder.Build(SearchServicesCacheKey, serviceName, AzureService.CloudConfiguration.CloudType.ToString());
        var searchClient = await _cacheService.GetAsync<SearchIndexClient>(CacheGroup, key, s_cacheDurationClients, cancellationToken);
        if (searchClient == null)
        {
            Uri endpoint = CreateAndValidateSearchEndpoint(serviceName);
            TokenCredential credential = await GetCredential(null, cancellationToken);

            var clientOptions = AddDefaultPolicies(new SearchClientOptions());
            clientOptions.Transport = new HttpClientTransport(AzureService.GetClient());
            clientOptions.Audience = GetSearchAudience();

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

    internal static void ConfigureQueryType(
        SearchOptions options,
        SearchIndex indexDefinition,
        IndexQueryType? queryType,
        string? semanticConfiguration = null)
    {
        switch (queryType)
        {
            case IndexQueryType.Simple:
                options.QueryType = SearchQueryType.Simple;
                break;

            case IndexQueryType.Semantic:
                var semanticConfigurationName = string.IsNullOrWhiteSpace(semanticConfiguration)
                    ? indexDefinition.SemanticSearch?.DefaultConfigurationName
                        ?? indexDefinition.SemanticSearch?.Configurations?.FirstOrDefault()?.Name
                    : semanticConfiguration;

                options.QueryType = SearchQueryType.Semantic;
                options.SemanticSearch = new SemanticSearchOptions
                {
                    SemanticConfigurationName = semanticConfigurationName,
                    QueryCaption = new(QueryCaptionType.Extractive),
                    QueryAnswer = new(QueryAnswerType.Extractive)
                };
                break;

            // Full Lucene syntax is used by default as it is a superset of the simple syntax.
            case IndexQueryType.Full:
            default:
                options.QueryType = SearchQueryType.Full;
                break;
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

    /// <summary>
    /// Creates and validates the shared Azure AI Search service endpoint used by Search SDK clients in the
    /// configured Azure cloud.
    /// </summary>
    /// <param name="serviceName">The Azure AI Search service name.</param>
    /// <returns>The validated Search SDK endpoint.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the service name is malformed or the Search endpoint allow-list is unavailable.
    /// </exception>
    /// <exception cref="InvalidConfigurationException">
    /// Thrown when the configured Azure cloud is not supported for Azure AI Search.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">
    /// Thrown when the completed endpoint is not authorized for the configured Azure cloud.
    /// </exception>
    internal Uri CreateAndValidateSearchEndpoint(string serviceName)
    {
        ValidateServiceName(serviceName);
        AzureCloudConfiguration.AzureCloud cloud = AzureService.CloudConfiguration.CloudType;

        // EndpointValidator.AllowLists.cs contains the authorization copy of these DNS suffixes. This construction
        // copy selects the configured cloud, so changes to either location and their tests must stay synchronized.
        string endpoint = cloud switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => $"https://{serviceName}.search.windows.net",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => $"https://{serviceName}.search.azure.cn",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => $"https://{serviceName}.search.azure.us",
            _ => throw GetUnsupportedCloudException(cloud)
        };

        Uri endpointUri = new(endpoint);

        // Service-name validation restricts the interpolated host label, while the shared validator independently
        // authorizes the completed SDK endpoint against the configured Azure cloud immediately before use.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: endpointUri.AbsoluteUri,
            serviceType: "search",
            armEnvironment: AzureService.CloudConfiguration.ArmEnvironment,
            executingToolNamespaceName: "search");

        return endpointUri;
    }

    private SearchAudience GetSearchAudience()
    {
        AzureCloudConfiguration.AzureCloud cloud = AzureService.CloudConfiguration.CloudType;
        return cloud switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => SearchAudience.AzurePublicCloud,
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => SearchAudience.AzureChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => SearchAudience.AzureGovernment,
            _ => throw GetUnsupportedCloudException(cloud)
        };
    }

    /// <summary>
    /// Gets a new exception to throw when the currently configured Azure cloud is not supported.
    /// </summary>
    /// <param name="cloud"></param>
    /// <returns></returns>
    /// <remarks>
    /// The Azure Search SDK docs specify defaulting to the public cloud for a <see langword="null"/>
    /// delegated permission audience, but the docs do not define an endpoint-domain fallback for an
    /// unrecognized cloud. We'll reject unknown clouds rather than use that fallback for endpoint
    /// creation or audience selection. This was a subjective choice that should be changed if required.
    /// <seealso href="https://learn.microsoft.com/dotnet/api/azure.search.documents.searchclientoptions.audience"/>
    /// </remarks>
    private static InvalidOperationException GetUnsupportedCloudException(AzureCloudConfiguration.AzureCloud cloud)
    {
        return new InvalidOperationException(
            $"The configured Azure cloud is not supported for Azure AI Search. Value given: '{cloud}'.");
    }
}
