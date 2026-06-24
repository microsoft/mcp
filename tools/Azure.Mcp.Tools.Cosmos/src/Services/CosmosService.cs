// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Cosmos.Services;

public sealed class CosmosService(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService, IHttpClientFactory httpClientFactory, ILogger<CosmosService> logger)
    : BaseAzureService(tenantService), ICosmosService, IAsyncDisposable
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<CosmosService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string CacheGroup = "cosmos";
    private const string CosmosClientsCacheKeyPrefix = "clients";
    private const string CosmosDatabasesCacheKeyPrefix = "databases";
    private const string CosmosContainersCacheKeyPrefix = "containers";
    private static readonly TimeSpan s_cacheDurationClients = CacheDurations.AuthenticatedClient;
    private static readonly TimeSpan s_cacheDurationResources = CacheDurations.ServiceData;
    private bool _disposed;

    private async Task<CosmosDBAccountResource> GetCosmosAccountAsync(
        string subscription,
        string accountName,
        string? tenant = null,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(accountName), accountName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        // Fast path: when the resource group is known, look the account up directly
        // instead of enumerating every Cosmos DB account in the subscription.
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            ResourceGroupResource resourceGroupResource;
            try
            {
                var resourceGroupResponse = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                resourceGroupResource = resourceGroupResponse.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
            }

            try
            {
                var account = await resourceGroupResource.GetCosmosDBAccountAsync(accountName, cancellationToken);
                return account.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new KeyNotFoundException($"Cosmos DB account '{accountName}' not found in resource group '{resourceGroup}', subscription '{subscription}'.");
            }
        }

        // Fallback: resource group unknown, enumerate and match case-insensitively.
        await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync(cancellationToken))
        {
            // Cosmos DB account names are case-insensitive in Azure, so compare accordingly.
            if (account.Data.Name.Equals(accountName, StringComparison.OrdinalIgnoreCase))
            {
                return account;
            }
        }
        throw new KeyNotFoundException($"Cosmos DB account '{accountName}' not found in subscription '{subscription}'.");
    }

    private async Task<CosmosClient> CreateCosmosClientWithAuth(
        string accountName,
        string subscription,
        AuthMethod authMethod,
        string? tenant = null,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        // Enable bulk execution and distributed tracing telemetry features once they are supported by the Microsoft.Azure.Cosmos.Aot package.
        // var clientOptions = new CosmosClientOptions { AllowBulkExecution = true };
        // clientOptions.CosmosClientTelemetryOptions.DisableDistributedTracing = false;
        var clientOptions = new CosmosClientOptions();
        clientOptions.CustomHandlers.Add(new UserPolicyRequestHandler(UserAgent));

        if (retryPolicy != null)
        {
            if (retryPolicy.MaxRetries is { } maxRetries)
                clientOptions.MaxRetryAttemptsOnRateLimitedRequests = maxRetries;
            if (retryPolicy.MaxDelaySeconds is { } maxDelaySeconds)
                clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(maxDelaySeconds);
        }

        clientOptions.HttpClientFactory = () => _httpClientFactory.CreateClient();

        CosmosClient cosmosClient;
        switch (authMethod)
        {
            case AuthMethod.Key:
                var cosmosAccount = await GetCosmosAccountAsync(subscription, accountName, tenant, resourceGroup, retryPolicy, cancellationToken);
                var keys = await cosmosAccount.GetKeysAsync(cancellationToken);
                cosmosClient = new(GetCosmosBaseUri(accountName), keys.Value.PrimaryMasterKey, clientOptions);
                break;

            case AuthMethod.Credential:
            default:
                cosmosClient = new(GetCosmosBaseUri(accountName), await GetCredential(tenant, cancellationToken), clientOptions);
                break;
        }

        // Validate the client by performing a lightweight operation
        await ValidateCosmosClientAsync(cosmosClient, cancellationToken);

        return cosmosClient;
    }

    private string GetCosmosBaseUri(string accountName)
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => $"https://{accountName}.documents.azure.com:443/",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => $"https://{accountName}.documents.azure.us:443/",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => $"https://{accountName}.documents.azure.cn:443/",
            _ => $"https://{accountName}.documents.azure.com:443/"
        };
    }

    private async Task ValidateCosmosClientAsync(CosmosClient client, CancellationToken cancellationToken = default)
    {
        // Perform a lightweight operation to validate the client
        await client.ReadAccountAsync().WaitAsync(cancellationToken);
    }

    private async Task<CosmosClient> GetCosmosClientAsync(
        string accountName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(subscription), subscription));

        var key = CacheKeyBuilder.Build(CosmosClientsCacheKeyPrefix, accountName, subscription, tenant ?? string.Empty, authMethod.ToString());
        var cosmosClient = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key, s_cacheDurationClients, cancellationToken);
        if (cosmosClient != null)
            return cosmosClient;

        cosmosClient = await CreateCosmosClientWithAuth(
            accountName,
            subscription,
            authMethod,
            tenant,
            resourceGroup,
            retryPolicy,
            cancellationToken);

        await _cacheService.SetAsync(CacheGroup, key, cosmosClient, s_cacheDurationClients, cancellationToken);
        return cosmosClient;
    }

    public async Task<List<string>> GetCosmosAccounts(string subscription, string? resourceGroup = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var accounts = new List<string>();

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            // Scope the listing to a single resource group so the service only returns
            // the accounts within it instead of enumerating the whole subscription.
            ResourceGroupResource resourceGroupResource;
            try
            {
                var resourceGroupResponse = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                resourceGroupResource = resourceGroupResponse.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
            }

            await foreach (var account in resourceGroupResource.GetCosmosDBAccounts().GetAllAsync(cancellationToken))
            {
                if (account?.Data?.Name != null)
                {
                    accounts.Add(account.Data.Name);
                }
            }
        }
        else
        {
            await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync(cancellationToken))
            {
                if (account?.Data?.Name != null)
                {
                    accounts.Add(account.Data.Name);
                }
            }
        }

        return accounts;
    }

    public async Task<List<string>> ListDatabases(
        string accountName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(subscription), subscription));

        var cacheKey = CacheKeyBuilder.Build(CosmosDatabasesCacheKeyPrefix, accountName, subscription, tenant ?? string.Empty, authMethod.ToString());

        var cachedDatabases = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources, cancellationToken);
        if (cachedDatabases != null)
        {
            return cachedDatabases;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, resourceGroup, retryPolicy, cancellationToken);
        var databases = new List<string>();

        var iterator = client.GetDatabaseQueryStreamIterator();
        while (iterator.HasMoreResults)
        {
            using ResponseMessage dbResponse = await iterator.ReadNextAsync(cancellationToken);
            dbResponse.EnsureSuccessStatusCode();

            using JsonDocument dbsQueryResultDoc = JsonDocument.Parse(dbResponse.Content);
            if (dbsQueryResultDoc.RootElement.TryGetProperty("Databases", out JsonElement documentsElement))
            {
                foreach (JsonElement databaseElement in documentsElement.EnumerateArray())
                {
                    string? databaseId = databaseElement.GetProperty("id").GetString();
                    if (!string.IsNullOrEmpty(databaseId))
                    {
                        databases.Add(databaseId);
                    }
                }
            }
        }

        await _cacheService.SetAsync(CacheGroup, cacheKey, databases, s_cacheDurationResources, cancellationToken);
        return databases;
    }

    public async Task<List<string>> ListContainers(
        string accountName,
        string databaseName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(databaseName), databaseName), (nameof(subscription), subscription));

        var cacheKey = CacheKeyBuilder.Build(CosmosContainersCacheKeyPrefix, accountName, databaseName, subscription, tenant ?? string.Empty, authMethod.ToString());

        var cachedContainers = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources, cancellationToken);
        if (cachedContainers != null)
        {
            return cachedContainers;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, resourceGroup, retryPolicy, cancellationToken);
        var containers = new List<string>();

        var database = client.GetDatabase(databaseName);
        var iterator = database.GetContainerQueryStreamIterator();
        while (iterator.HasMoreResults)
        {
            using ResponseMessage containerRResponse = await iterator.ReadNextAsync(cancellationToken);
            containerRResponse.EnsureSuccessStatusCode();

            using JsonDocument containersQueryResultDoc = JsonDocument.Parse(containerRResponse.Content);
            if (containersQueryResultDoc.RootElement.TryGetProperty("DocumentCollections", out JsonElement containersElement))
            {
                foreach (JsonElement containerElement in containersElement.EnumerateArray())
                {
                    string? containerId = containerElement.GetProperty("id").GetString();
                    if (!string.IsNullOrEmpty(containerId))
                    {
                        containers.Add(containerId);
                    }
                }
            }
        }

        await _cacheService.SetAsync(CacheGroup, cacheKey, containers, s_cacheDurationResources, cancellationToken);
        return containers;
    }

    public async Task<List<JsonElement>> QueryItems(
        string accountName,
        string databaseName,
        string containerName,
        string? query,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(databaseName), databaseName), (nameof(containerName), containerName), (nameof(subscription), subscription));

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, null, retryPolicy, cancellationToken);

        var container = client.GetContainer(databaseName, containerName);
        var baseQuery = string.IsNullOrEmpty(query) ? "SELECT * FROM c" : query;

        var (parameterizedQuery, queryParameters) = ParameterizeStringLiterals(baseQuery);
        var queryDef = new QueryDefinition(parameterizedQuery);

        foreach (var (name, value) in queryParameters)
        {
            queryDef = queryDef.WithParameter(name, value);
        }

        var items = new List<JsonElement>();
        var queryIterator = container.GetItemQueryStreamIterator(
            queryDef,
            requestOptions: new() { MaxItemCount = -1 }
        );

        while (queryIterator.HasMoreResults)
        {
            using ResponseMessage response = await queryIterator.ReadNextAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(response.Content);
            items.Add(document.RootElement.Clone());
        }

        return items;
    }

    public async Task<ContainerSchema> GetApproximateSchema(
        string accountName,
        string databaseName,
        string containerName,
        int sampleSize,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(accountName), accountName),
            (nameof(databaseName), databaseName),
            (nameof(containerName), containerName),
            (nameof(subscription), subscription));

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, null, retryPolicy, cancellationToken);
        var container = client.GetContainer(databaseName, containerName);

        var queryDef = new QueryDefinition("SELECT TOP @sampleSize * FROM c")
            .WithParameter("@sampleSize", sampleSize);
        var iterator = container.GetItemQueryStreamIterator(
            queryDef,
            requestOptions: new QueryRequestOptions { MaxItemCount = sampleSize });

        var properties = new Dictionary<string, (HashSet<string> Types, int Count)>(StringComparer.Ordinal);
        var sampled = 0;

        while (iterator.HasMoreResults && sampled < sampleSize)
        {
            using var response = await iterator.ReadNextAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("Documents", out var docs))
            {
                continue;
            }

            foreach (var item in docs.EnumerateArray())
            {
                sampled++;

                foreach (var prop in item.EnumerateObject())
                {
                    var typeName = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => "string",
                        JsonValueKind.Number => "number",
                        JsonValueKind.True or JsonValueKind.False => "boolean",
                        JsonValueKind.Object => "object",
                        JsonValueKind.Array => "array",
                        JsonValueKind.Null => "null",
                        _ => "unknown",
                    };

                    if (!properties.TryGetValue(prop.Name, out var entry))
                    {
                        entry = (new HashSet<string>(StringComparer.Ordinal), 0);
                    }
                    entry.Types.Add(typeName);
                    properties[prop.Name] = (entry.Types, entry.Count + 1);
                }

                if (sampled >= sampleSize)
                {
                    break;
                }
            }
        }

        var schemaProperties = properties
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => new SchemaProperty(
                kvp.Key,
                string.Join(" | ", kvp.Value.Types.OrderBy(t => t, StringComparer.Ordinal)),
                kvp.Value.Count,
                sampled))
            .ToList();

        return new ContainerSchema(sampled, schemaProperties);
    }

    public async Task<List<JsonElement>> GetRecentItems(
        string accountName,
        string databaseName,
        string containerName,
        int count,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(accountName), accountName),
            (nameof(databaseName), databaseName),
            (nameof(containerName), containerName),
            (nameof(subscription), subscription));

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, null, retryPolicy, cancellationToken);
        var container = client.GetContainer(databaseName, containerName);

        var queryDef = new QueryDefinition("SELECT TOP @topN * FROM c ORDER BY c._ts DESC")
            .WithParameter("@topN", count);
        var iterator = container.GetItemQueryStreamIterator(
            queryDef,
            requestOptions: new QueryRequestOptions { MaxItemCount = count });

        var results = new List<JsonElement>(count);
        while (iterator.HasMoreResults && results.Count < count)
        {
            using var response = await iterator.ReadNextAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("Documents", out var docs))
            {
                continue;
            }

            foreach (var item in docs.EnumerateArray())
            {
                results.Add(item.Clone());
                if (results.Count >= count)
                {
                    break;
                }
            }
        }

        return results;
    }

    public async Task<JsonElement?> GetItem(
        string accountName,
        string databaseName,
        string containerName,
        string id,
        string? partitionKey,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(accountName), accountName),
            (nameof(databaseName), databaseName),
            (nameof(containerName), containerName),
            (nameof(id), id),
            (nameof(subscription), subscription));

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, null, retryPolicy, cancellationToken);
        var container = client.GetContainer(databaseName, containerName);

        // TODO: When Microsoft.Azure.Cosmos.Aot covers ReadItemStreamAsync + CosmosException, restore the
        // cheaper point-read branch (ReadItemStreamAsync with new PartitionKey(partitionKey) and a typed
        // CosmosException catch for 404). Both currently drag the legacy FeedResponseBinder/HybridRow/Newtonsoft
        // call graph into the trim closure and break AOT. Until then we use the query-stream API for both
        // single-partition and cross-partition reads.
        var requestOptions = new QueryRequestOptions { MaxItemCount = 1 };
        if (!string.IsNullOrEmpty(partitionKey))
        {
            requestOptions.PartitionKey = new PartitionKey(partitionKey);
        }

        var queryDef = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", id);
        var iterator = container.GetItemQueryStreamIterator(queryDef, requestOptions: requestOptions);

        while (iterator.HasMoreResults)
        {
            using var response = await iterator.ReadNextAsync(cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken);
            if (doc.RootElement.TryGetProperty("Documents", out var docs) && docs.GetArrayLength() > 0)
            {
                return docs[0].Clone();
            }
        }

        return null;
    }

    public async Task<List<JsonElement>> TextSearch(
        string accountName,
        string databaseName,
        string containerName,
        string property,
        string searchPhrase,
        IReadOnlyList<string>? propertiesToSelect,
        int count,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(accountName), accountName),
            (nameof(databaseName), databaseName),
            (nameof(containerName), containerName),
            (nameof(property), property),
            (nameof(searchPhrase), searchPhrase),
            (nameof(subscription), subscription));

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, null, retryPolicy, cancellationToken);
        var container = client.GetContainer(databaseName, containerName);

        var selectClause = propertiesToSelect is { Count: > 0 }
            ? string.Join(", ", propertiesToSelect.Select(p => $"c.{p}"))
            : "*";

        var queryDef = new QueryDefinition(
                $"SELECT TOP @topN {selectClause} FROM c WHERE FullTextContains(c.{property}, @searchPhrase)")
            .WithParameter("@topN", count)
            .WithParameter("@searchPhrase", searchPhrase);

        var iterator = container.GetItemQueryStreamIterator(
            queryDef,
            requestOptions: new QueryRequestOptions { MaxItemCount = count });

        var results = new List<JsonElement>(count);
        while (iterator.HasMoreResults && results.Count < count)
        {
            using var response = await iterator.ReadNextAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("Documents", out var docs))
            {
                continue;
            }

            foreach (var item in docs.EnumerateArray())
            {
                results.Add(item.Clone());
                if (results.Count >= count)
                {
                    break;
                }
            }
        }

        return results;
    }

    public async Task<List<JsonElement>> VectorSearch(
        string accountName,
        string databaseName,
        string containerName,
        string vectorProperty,
        IReadOnlyList<string>? propertiesToSelect,
        IReadOnlyList<float> embedding,
        int count,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(accountName), accountName),
            (nameof(databaseName), databaseName),
            (nameof(containerName), containerName),
            (nameof(vectorProperty), vectorProperty),
            (nameof(subscription), subscription));

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, null, retryPolicy, cancellationToken);
        var container = client.GetContainer(databaseName, containerName);

        // Inline the embedding as a JSON array literal. The Cosmos AOT serializer
        // context does not include Single[] / float[], so passing it via
        // WithParameter throws NotSupportedException at query-plan serialization.
        var embeddingLiteral = "[" + string.Join(
            ",",
            embedding.Select(f => f.ToString("R", System.Globalization.CultureInfo.InvariantCulture))) + "]";

        // Two query shapes:
        //  - With explicit propertiesToSelect: project those columns + the score directly. Smaller RU and bandwidth.
        //  - Without: wrap the whole document via `c AS doc` so we can strip the vector property client-side
        //    and still return every other field alongside the server-computed score.
        var hasProjection = propertiesToSelect is { Count: > 0 };
        var selectClause = hasProjection
            ? string.Join(", ", propertiesToSelect!.Select(p => $"c.{p}"))
            : "c AS doc";

        var queryDef = new QueryDefinition(
                $"SELECT TOP @topN {selectClause}, VectorDistance(c.{vectorProperty}, {embeddingLiteral}) AS _score "
                + $"FROM c ORDER BY VectorDistance(c.{vectorProperty}, {embeddingLiteral})")
            .WithParameter("@topN", count);

        var iterator = container.GetItemQueryStreamIterator(
            queryDef,
            requestOptions: new QueryRequestOptions { MaxItemCount = count });

        var results = new List<JsonElement>(count);
        while (iterator.HasMoreResults && results.Count < count)
        {
            using var response = await iterator.ReadNextAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("Documents", out var docs))
            {
                continue;
            }

            foreach (var item in docs.EnumerateArray())
            {
                results.Add(hasProjection ? item.Clone() : FlattenResultAndStripVector(item, vectorProperty));
                if (results.Count >= count)
                {
                    break;
                }
            }
        }

        return results;
    }

    // Rewrites { "doc": {...}, "_score": x } into { "_score": x, ...doc fields except vectorProperty }.
    private static JsonElement FlattenResultAndStripVector(JsonElement wrapped, string vectorProperty)
    {
        var pathSegments = vectorProperty.Split('.', StringSplitOptions.RemoveEmptyEntries);
        // Rewrite via Utf8JsonWriter — documents are arbitrary user JSON, so there is no model type
        // we can attach a JsonSerializerContext to, and reflection-based JsonSerializer is not AOT-safe.
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            writer.WriteStartObject();

            if (wrapped.TryGetProperty("_score", out var score))
            {
                writer.WritePropertyName("_score");
                score.WriteTo(writer);
            }

            if (wrapped.TryGetProperty("doc", out var docElement) && docElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in docElement.EnumerateObject())
                {
                    if (prop.NameEquals(pathSegments[0]))
                    {
                        if (pathSegments.Length == 1)
                        {
                            // Leaf — drop the vector property entirely.
                            continue;
                        }

                        writer.WritePropertyName(prop.Name);
                        WriteWithoutPath(writer, prop.Value, pathSegments, 1);
                        continue;
                    }

                    prop.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        ms.Position = 0;
        using var rewritten = JsonDocument.Parse(ms);
        return rewritten.RootElement.Clone();
    }

    private static void WriteWithoutPath(Utf8JsonWriter writer, JsonElement element, string[] segments, int depth)
    {
        if (depth >= segments.Length || element.ValueKind != JsonValueKind.Object)
        {
            element.WriteTo(writer);
            return;
        }

        writer.WriteStartObject();
        var target = segments[depth];
        var isLeaf = depth == segments.Length - 1;

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.NameEquals(target))
            {
                if (isLeaf)
                {
                    continue;
                }

                writer.WritePropertyName(prop.Name);
                WriteWithoutPath(writer, prop.Value, segments, depth + 1);
                continue;
            }

            prop.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    public async Task<float[]> GenerateEmbedding(
        string text,
        EmbeddingRequest request,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(text), text),
            (nameof(request.Endpoint), request?.Endpoint),
            (nameof(request.DeploymentName), request?.DeploymentName));

        ValidateOpenAIEndpoint(request!.Endpoint!);

        var credential = await GetCredential(tenant, cancellationToken);
        var clientOptions = new AzureOpenAIClientOptions
        {
            Transport = new System.ClientModel.Primitives.HttpClientPipelineTransport(_httpClientFactory.CreateClient()),
        };

        var openAi = new AzureOpenAIClient(new Uri(request!.Endpoint!), credential, clientOptions);
        var embeddingClient = openAi.GetEmbeddingClient(request.DeploymentName);

        var response = request.Dimensions.HasValue
            ? await embeddingClient.GenerateEmbeddingAsync(
                text,
                new OpenAI.Embeddings.EmbeddingGenerationOptions { Dimensions = request.Dimensions.Value },
                cancellationToken)
            : await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);

        return response.Value.ToFloats().ToArray();
    }

    private static readonly string[] s_openAIEndpointServiceTypes = ["azure-openai", "foundry"];

    private void ValidateOpenAIEndpoint(string endpoint)
    {
        var armEnvironment = _tenantService.CloudConfiguration.ArmEnvironment;
        Exception? lastError = null;

        foreach (var serviceType in s_openAIEndpointServiceTypes)
        {
            try
            {
                EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, armEnvironment);
                return;
            }
            catch (Exception ex) when (ex is SecurityException or ArgumentException)
            {
                lastError = ex;
            }
        }

        throw new ArgumentException(
            "The provided Azure OpenAI endpoint is not a trusted Azure OpenAI, Cognitive Services, or AI Foundry endpoint for the configured Azure cloud.",
            nameof(EmbeddingRequest.Endpoint),
            lastError);
    }

    internal static (string Query, List<(string Name, string Value)> Parameters) ParameterizeStringLiterals(string query) =>
        SqlQueryParameterizer.Parameterize(query, SqlQueryParameterizer.SqlDialect.Standard);

    private static readonly TimeSpan s_disposeTimeout = TimeSpan.FromSeconds(2);

    private async ValueTask DisposeAsyncCore()
    {
        // Use a bounded timeout so disposal can never hang indefinitely.
        // We do not use CancellationToken.None (unbounded) nor any IHostApplicationLifetime
        // token (already cancelled by the time DisposeAsync runs).
        using var cts = new CancellationTokenSource(s_disposeTimeout);

        IEnumerable<string> keys;
        try
        {
            // Get all cached client keys
            keys = await _cacheService.GetGroupKeysAsync(CacheGroup, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cached CosmosClient keys during disposal");
            return;
        }

        // If no keys were returned, there's nothing to dispose
        if (keys == null)
        {
            return;
        }

        // Filter for client keys only (those that start with the client prefix)
        var clientKeys = keys.Where(k => k.StartsWith(CosmosClientsCacheKeyPrefix));

        // Retrieve and dispose each client
        foreach (var key in clientKeys)
        {
            try
            {
                var client = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key, cancellationToken: cts.Token);
                client?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispose CosmosClient for cache key {CacheKey}", key);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    internal class UserPolicyRequestHandler : RequestHandler
    {
        private readonly string userAgent;

        internal UserPolicyRequestHandler(string userAgent) => this.userAgent = userAgent;

        public override Task<ResponseMessage> SendAsync(RequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Set(UserAgentPolicy.UserAgentHeader, userAgent);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
