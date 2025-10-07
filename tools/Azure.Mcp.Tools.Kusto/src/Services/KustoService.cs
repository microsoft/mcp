// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Kusto.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Kusto.Services;


public sealed class KustoService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IHttpClientService httpClientService,
    ILogger<KustoService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IKustoService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    private readonly ILogger<KustoService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "kusto";
    private const string KustoClustersCacheKey = "clusters";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan s_providerCacheDuration = TimeSpan.FromHours(2);

    // Provider cache key generator
    private static string GetProviderCacheKey(string clusterUri)
        => $"{clusterUri}";

    public async Task<List<string>> ListClustersAsync(
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        try
        {
            var clusters = await ExecuteResourceQueryAsync(
                "Microsoft.Kusto/clusters",
                resourceGroup: null, // all resource groups
                subscriptionId,
                retryPolicy,
                item => ConvertToClusterModel(item).ClusterName,
                cancellationToken: CancellationToken.None);

            return clusters;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Kusto clusters: {ex.Message}", ex);
        }
    }

    public async Task<KustoClusterModel> GetClusterAsync(
            string subscriptionId,
            string clusterName,
            string? tenant = null,
            RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        try
        {
            var cluster = await ExecuteSingleResourceQueryAsync(
                        "Microsoft.Kusto/clusters",
                        resourceGroup: null, // all resource groups
                        subscription: subscriptionId,
                        retryPolicy: retryPolicy,
                        converter: ConvertToClusterModel,
                        additionalFilter: $"name =~ '{EscapeKqlString(clusterName)}'");

            if (cluster == null)
            {
                throw new KeyNotFoundException($"Kusto cluster '{clusterName}' not found for subscription '{subscriptionId}'.");
            }
            return cluster;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving Kusto cluster '{ClusterName}' for subscription '{Subscription}'",
                clusterName, subscriptionId);
            throw;
        }
    }

    public async Task<List<string>> ListDatabasesAsync(
        string subscriptionId,
        string clusterName,
        string? tenant = null,
        AuthMethod? authMethod =
        AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListDatabasesAsync(clusterUri, tenant, authMethod, retryPolicy);
    }

    public async Task<List<string>> ListDatabasesAsync(
        string clusterUri,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri);

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant).ConfigureAwait(false);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            "NetDefaultDB",
            ".show databases | project DatabaseName",
            CancellationToken.None);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<List<string>> ListTablesAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName),
            (nameof(databaseName), databaseName));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListTablesAsync(clusterUri, databaseName, tenant, authMethod, retryPolicy);
    }

    public async Task<List<string>> ListTablesAsync(
        string clusterUri,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri, databaseName);

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            databaseName,
            ".show tables",
            CancellationToken.None);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<string> GetTableSchemaAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await GetTableSchemaAsync(clusterUri, databaseName, tableName, tenant, authMethod, retryPolicy);
    }

    public async Task<string> GetTableSchemaAsync(
        string clusterUri,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(clusterUri), clusterUri),
            (nameof(databaseName), databaseName),
            (nameof(tableName), tableName));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant);
        var kustoResult = await kustoClient.ExecuteQueryCommandAsync(
            databaseName,
            $".show table {tableName} cslschema", CancellationToken.None);
        var result = KustoResultToStringList(kustoResult);
        if (result.Count > 0)
        {
            return string.Join(Environment.NewLine, result);
        }
        throw new Exception($"No schema found for table '{tableName}' in database '{databaseName}'.");
    }

    public async Task<List<JsonElement>> QueryItemsAsync(
            string subscriptionId,
            string clusterName,
            string databaseName,
            string query,
            string? tenant = null,
            AuthMethod? authMethod = AuthMethod.Credential,
            RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName),
            (nameof(databaseName), databaseName),
            (nameof(query), query));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await QueryItemsAsync(clusterUri, databaseName, query, tenant, authMethod, retryPolicy);
    }

    public async Task<List<JsonElement>> QueryItemsAsync(
        string clusterUri,
        string databaseName,
        string query,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterUri, databaseName, query);

        var cslQueryProvider = await GetOrCreateCslQueryProviderAsync(clusterUri, tenant);
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

    private async Task<KustoClient> GetOrCreateKustoClientAsync(string clusterUri, string? tenant)
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

    private async Task<KustoClient> GetOrCreateCslQueryProviderAsync(string clusterUri, string? tenant)
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

    private async Task<string> GetClusterUriAsync(
        string subscriptionId,
        string clusterName,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        var cluster = await GetClusterAsync(subscriptionId, clusterName, tenant, retryPolicy);
        var value = cluster?.ClusterUri;

        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"Could not retrieve ClusterUri for cluster '{clusterName}'");
        }

        return value!;
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a cluster name string.
    /// </summary>
    /// <param name="item">The JsonElement containing cluster data</param>
    /// <returns>The cluster model</returns>
    private static KustoClusterModel ConvertToClusterModel(JsonElement item)
    {
        Models.KustoClusterData? kustoCluster = Models.KustoClusterData.FromJson(item);
        if (kustoCluster == null)
            throw new InvalidOperationException("Failed to parse Kusto cluster data");

        if (string.IsNullOrEmpty(kustoCluster.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(kustoCluster.ResourceId);

        return new KustoClusterModel(
            ClusterName: kustoCluster.ResourceName ?? "Unknown",
            Location: kustoCluster.Location,
            ResourceGroupName: id.ResourceGroupName,
            SubscriptionId: id.SubscriptionId,
            Sku: kustoCluster.Sku?.Name,
            Zones: kustoCluster.Zones == null ? string.Empty : string.Join(",", kustoCluster.Zones.ToList()),
            Identity: kustoCluster.Identity?.ManagedServiceIdentityType,
            ETag: kustoCluster.ETag?.ToString(),
            State: kustoCluster.Properties?.State,
            ProvisioningState: kustoCluster.Properties?.ProvisioningState,
            ClusterUri: kustoCluster.Properties?.ClusterUri?.ToString(),
            DataIngestionUri: kustoCluster.Properties?.DataIngestionUri?.ToString(),
            StateReason: kustoCluster.Properties?.StateReason,
            IsStreamingIngestEnabled: kustoCluster.Properties?.IsStreamingIngestEnabled ?? false,
            EngineType: kustoCluster.Properties?.EngineType?.ToString(),
            IsAutoStopEnabled: kustoCluster.Properties?.IsAutoStopEnabled ?? false
        );
    }
}
