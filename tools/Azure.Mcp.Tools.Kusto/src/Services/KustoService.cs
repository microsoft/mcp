// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.ResourceManager.Kusto;

namespace Azure.Mcp.Tools.Kusto.Services;


public sealed class KustoService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IHttpClientService httpClientService) : BaseAzureService(tenantService), IKustoService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

    private const string CacheGroup = "kusto";
    private const string KustoClustersCacheKey = "clusters";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan s_providerCacheDuration = TimeSpan.FromHours(2);

    // Provider cache key generator
    private static string GetProviderCacheKey(string clusterUri)
        => $"{clusterUri}";

    public async Task<List<string>> ListClusters(
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId);

        // Create cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{KustoClustersCacheKey}_{subscriptionId}"
            : $"{KustoClustersCacheKey}_{subscriptionId}_{tenant}";

        // Try to get from cache first
        var cachedClusters = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedClusters != null)
        {
            return cachedClusters;
        }

        var subscription = await _subscriptionService.GetSubscription(subscriptionId, tenant, retryPolicy);
        var clusters = new List<string>();

        await foreach (var cluster in subscription.GetKustoClustersAsync())
        {
            if (cluster?.Data?.Name != null)
            {
                clusters.Add(cluster.Data.Name);
            }
        }
        await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration);

        return clusters;
    }

    public async Task<KustoClusterResourceProxy?> GetCluster(
            string subscriptionId,
            string clusterName,
            string? tenant = null,
            RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId);

        var subscription = await _subscriptionService.GetSubscription(subscriptionId, tenant, retryPolicy);
        await foreach (var cluster in subscription.GetKustoClustersAsync())
        {
            if (string.Equals(cluster.Data.Name, clusterName, StringComparison.OrdinalIgnoreCase))
            {
                return new KustoClusterResourceProxy(cluster);
            }
        }

        return null;
    }

    public async Task<List<string>> ListDatabases(
        string subscriptionId,
        string clusterName,
        string? tenant = null,
        AuthMethod? authMethod =
        AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId, clusterName);

        string clusterUri = await GetClusterUri(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListDatabases(clusterUri, tenant, authMethod, retryPolicy);
    }

    public async Task<List<string>> ListDatabases(
        string clusterUri,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri);

        var kustoClient = await GetOrCreateKustoClient(clusterUri, tenant).ConfigureAwait(false);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            "NetDefaultDB",
            ".show databases | project DatabaseName",
            CancellationToken.None);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<List<string>> ListTables(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId, clusterName, databaseName);

        string clusterUri = await GetClusterUri(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListTables(clusterUri, databaseName, tenant, authMethod, retryPolicy);
    }

    public async Task<List<string>> ListTables(
        string clusterUri,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri, databaseName);

        var kustoClient = await GetOrCreateKustoClient(clusterUri, tenant);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            databaseName,
            ".show tables",
            CancellationToken.None);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<string> GetTableSchema(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        string clusterUri = await GetClusterUri(subscriptionId, clusterName, tenant, retryPolicy);
        return await GetTableSchema(clusterUri, databaseName, tableName, tenant, authMethod, retryPolicy);
    }

    public async Task<string> GetTableSchema(
        string clusterUri,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri, databaseName, tableName);

        var kustoClient = await GetOrCreateKustoClient(clusterUri, tenant);
        var kustoResult = await kustoClient.ExecuteQueryCommandAsync(
            databaseName,
            $".show table {tableName} cslschema", CancellationToken.None);
        var result = KustoResultToStringList(kustoResult);
        var schema = result.FirstOrDefault();
        if (schema is not null)
        {
            return schema;
        }
        throw new Exception($"No schema found for table '{tableName}' in database '{databaseName}'.");
    }

    public async Task<List<JsonElement>> QueryItems(
            string subscriptionId,
            string clusterName,
            string databaseName,
            string query,
            string? tenant = null,
            AuthMethod? authMethod = AuthMethod.Credential,
            RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId, clusterName, databaseName, query);

        string clusterUri = await GetClusterUri(subscriptionId, clusterName, tenant, retryPolicy);
        return await QueryItems(clusterUri, databaseName, query, tenant, authMethod, retryPolicy);
    }

    public async Task<List<JsonElement>> QueryItems(
        string clusterUri,
        string databaseName,
        string query,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri, databaseName, query);

        var cslQueryProvider = await GetOrCreateCslQueryProvider(clusterUri, tenant);
        var result = new List<JsonElement>();
        var kustoResult = await cslQueryProvider.ExecuteQueryCommandAsync(databaseName, query, CancellationToken.None);
        if (kustoResult.JsonDocument is null)
        {
            return result;
        }
        var root = kustoResult.JsonDocument.RootElement;
        if (!root.TryGetProperty("Tables", out var tablesElement) || tablesElement.ValueKind != JsonValueKind.Array || tablesElement.GetArrayLength() == 0)
        {
            return result;
        }
        var table = tablesElement[0];
        if (!table.TryGetProperty("Columns", out var columnsElement) || columnsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        var columnsDict = columnsElement.EnumerateArray()
            .ToDictionary(
                column => column.GetProperty("ColumnName").GetString()!,
                column => column.GetProperty("ColumnType").GetString()!
            );

        var columnsDictJson = "{" + string.Join(",", columnsDict.Select(kvp =>
                    $"\"{JsonEncodedText.Encode(kvp.Key)}\":\"{JsonEncodedText.Encode(kvp.Value)}\"")) + "}";
        result.Add(JsonDocument.Parse(columnsDictJson).RootElement);

        if (!table.TryGetProperty("Rows", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        foreach (var item in items.EnumerateArray())
        {
            var json = item.ToString();
            result.Add(JsonDocument.Parse(json).RootElement);
        }

        return result;
    }

    private List<string> KustoResultToStringList(KustoResult kustoResult)
    {
        var result = new List<string>();
        if (kustoResult.JsonDocument is null)
        {
            return result;
        }
        var root = kustoResult.JsonDocument.RootElement;
        if (!root.TryGetProperty("Tables", out var tablesElement) || tablesElement.ValueKind != JsonValueKind.Array || tablesElement.GetArrayLength() == 0)
        {
            return result;
        }
        var table = tablesElement[0];
        if (!table.TryGetProperty("Columns", out var columnsElement) || columnsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        var columns = columnsElement.EnumerateArray()
            .Select(column => ($"{column.GetProperty("ColumnName").GetString()}:{column.GetProperty("ColumnType").GetString()}"));
        var columnsAsString = string.Join(",", columns);
        result.Add(columnsAsString);
        if (!table.TryGetProperty("Rows", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        foreach (var item in items.EnumerateArray())
        {
            var jsonAsString = item.ToString();
            result.Add(jsonAsString);
        }
        return result;
    }

    private async Task<KustoClient> GetOrCreateKustoClient(string clusterUri, string? tenant)
    {
        var providerCacheKey = GetProviderCacheKey(clusterUri) + "_command";
        var kustoClient = await _cacheService.GetAsync<KustoClient>(CacheGroup, providerCacheKey, s_providerCacheDuration);
        if (kustoClient == null)
        {
            var tokenCredential = await GetCredential(tenant);
            kustoClient = new KustoClient(clusterUri, tokenCredential, UserAgent, _httpClientService);
            await _cacheService.SetAsync(CacheGroup, providerCacheKey, kustoClient, s_providerCacheDuration);
        }

        return kustoClient;
    }

    private async Task<KustoClient> GetOrCreateCslQueryProvider(string clusterUri, string? tenant)
    {
        var providerCacheKey = GetProviderCacheKey(clusterUri) + "_query";
        var kustoClient = await _cacheService.GetAsync<KustoClient>(CacheGroup, providerCacheKey, s_providerCacheDuration);
        if (kustoClient == null)
        {
            var tokenCredential = await GetCredential(tenant);
            kustoClient = new KustoClient(clusterUri, tokenCredential, UserAgent, _httpClientService);
            await _cacheService.SetAsync(CacheGroup, providerCacheKey, kustoClient, s_providerCacheDuration);
        }

        return kustoClient;
    }

    private async Task<string> GetClusterUri(
        string subscriptionId,
        string clusterName,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        var cluster = await GetCluster(subscriptionId, clusterName, tenant, retryPolicy);
        var value = cluster?.ClusterUri;

        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"Could not retrieve ClusterUri for cluster '{clusterName}'");
        }

        return value!;
    }
}
