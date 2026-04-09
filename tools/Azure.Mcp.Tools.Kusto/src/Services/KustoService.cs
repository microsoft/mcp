// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Kusto.Models;
using Azure.Mcp.Tools.Kusto.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Kusto.Services;


public sealed class KustoService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IHttpClientFactory httpClientFactory,
    ILogger<KustoService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IKustoService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<KustoService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "kusto";
    private const string KustoClustersCacheKey = "clusters";
    private static readonly TimeSpan s_cacheDuration = CacheDurations.ServiceData;
    private static readonly TimeSpan s_providerCacheDuration = CacheDurations.AuthenticatedClient;

    /// <summary>
    /// Escapes a KQL identifier (e.g., table name) using bracket notation to prevent injection.
    /// </summary>
    internal static string EscapeKqlIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        // Remove any existing KQL bracket escape sequences to normalize the identifier
        var unescaped = identifier
            .Replace("['", "", StringComparison.Ordinal)
            .Replace("[\"", "", StringComparison.Ordinal)
            .Replace("\"]", "", StringComparison.Ordinal)
            .Replace("']", "", StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(unescaped))
        {
            throw new ArgumentException("Identifier is empty after removing escape characters.", nameof(identifier));
        }

        return KqlSanitizer.EscapeIdentifier(unescaped);
    }

    internal static string SanitizeKqlStringLiterals(string query) =>
        KqlSanitizer.SanitizeStringLiterals(query);

    // Provider cache key generator
    private static string GetProviderCacheKey(string clusterUri, string? tenant, string suffix)
    {
        var tenantKey = string.IsNullOrEmpty(tenant) ? "default" : tenant;
        return CacheKeyBuilder.Build(tenantKey, clusterUri, suffix);
    }

    public async Task<ResourceQueryResults<string>> ListClustersAsync(
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        var clusters = await ExecuteResourceQueryAsync(
            "Microsoft.Kusto/clusters",
            resourceGroup: null, // all resource groups
            subscriptionId,
            retryPolicy,
            item => ConvertToClusterModel(item).ClusterName,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return clusters;
    }

    public async Task<KustoClusterModel> GetClusterAsync(
        string subscriptionId,
        string clusterName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        var cluster = await ExecuteSingleResourceQueryAsync(
            "Microsoft.Kusto/clusters",
            resourceGroup: null, // all resource groups
            subscription: subscriptionId,
            retryPolicy: retryPolicy,
            converter: ConvertToClusterModel,
            additionalFilter: $"name =~ '{EscapeKqlString(clusterName)}'",
            tenant: tenant,
            cancellationToken: cancellationToken);

        if (cluster == null)
        {
            throw new KeyNotFoundException($"Kusto cluster '{clusterName}' not found for subscription '{subscriptionId}'.");
        }
        return cluster;
    }

    public async Task<List<string>> ListDatabasesAsync(
        string subscriptionId,
        string clusterName,
        string? tenant = null,
        AuthMethod? authMethod =
        AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListDatabasesAsync(clusterUri, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<List<string>> ListDatabasesAsync(
        string clusterUri,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(clusterUri), clusterUri));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant, cancellationToken).ConfigureAwait(false);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            "NetDefaultDB",
            ".show databases | project DatabaseName",
            cancellationToken);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<List<string>> ListTablesAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName),
            (nameof(databaseName), databaseName));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListTablesAsync(clusterUri, databaseName, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<List<string>> ListTablesAsync(
        string clusterUri,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(clusterUri), clusterUri), (nameof(databaseName), databaseName));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant, cancellationToken);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            databaseName,
            ".show tables",
            cancellationToken);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<string> GetTableSchemaAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await GetTableSchemaAsync(clusterUri, databaseName, tableName, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<string> GetTableSchemaAsync(
        string clusterUri,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(clusterUri), clusterUri),
            (nameof(databaseName), databaseName),
            (nameof(tableName), tableName));

        // Validate table name to prevent KQL injection — while the query endpoint is read-only
        // (no data modification), injection could still enable information disclosure or resource abuse
        KustoIdentifierValidator.ValidateIdentifier(tableName, nameof(tableName));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant, cancellationToken);
        var kustoResult = await kustoClient.ExecuteQueryCommandAsync(
            databaseName,
            $".show table {EscapeKqlIdentifier(tableName)} cslschema", cancellationToken);
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName),
            (nameof(databaseName), databaseName),
            (nameof(query), query));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await QueryItemsAsync(clusterUri, databaseName, query, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<List<JsonElement>> QueryItemsAsync(
        string clusterUri,
        string databaseName,
        string query,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(clusterUri), clusterUri),
            (nameof(databaseName), databaseName),
            (nameof(query), query));

        KqlQueryValidator.ValidateQuerySafety(query);

        var cslQueryProvider = await GetOrCreateCslQueryProviderAsync(clusterUri, tenant, cancellationToken);
        var result = new List<JsonElement>();
        var sanitizedQuery = SanitizeKqlStringLiterals(query);
        var kustoResult = await cslQueryProvider.ExecuteQueryCommandAsync(databaseName, sanitizedQuery, cancellationToken);
        if (kustoResult.RootElement.ValueKind == JsonValueKind.Null)
        {
            return result;
        }
        var root = kustoResult.RootElement;
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
        using (var jsonDoc = JsonDocument.Parse(columnsDictJson))
        {
            result.Add(jsonDoc.RootElement.Clone());
        }

        if (!table.TryGetProperty("Rows", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        foreach (var item in items.EnumerateArray())
        {
            var json = item.ToString();
            using (var jsonDoc = JsonDocument.Parse(json))
            {
                result.Add(jsonDoc.RootElement.Clone());
            }
        }

        return result;
    }

    private static List<string> KustoResultToStringList(KustoResult kustoResult)
    {
        var result = new List<string>();
        if (kustoResult.RootElement.ValueKind == JsonValueKind.Null)
        {
            return result;
        }
        var root = kustoResult.RootElement;
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
            .Select(column => $"{column.GetProperty("ColumnName").GetString()}:{column.GetProperty("ColumnType").GetString()}");
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

    private async Task<KustoClient> GetOrCreateKustoClientAsync(string clusterUri, string? tenant, CancellationToken cancellationToken = default)
    {
        var providerCacheKey = GetProviderCacheKey(clusterUri, tenant, "command");
        var kustoClient = await _cacheService.GetAsync<KustoClient>(CacheGroup, providerCacheKey, s_providerCacheDuration, cancellationToken);
        if (kustoClient == null)
        {
            var tokenCredential = await GetCredential(tenant, cancellationToken);
            kustoClient = new KustoClient(clusterUri, tokenCredential, UserAgent, _httpClientFactory);
            await _cacheService.SetAsync(CacheGroup, providerCacheKey, kustoClient, s_providerCacheDuration, cancellationToken);
        }

        return kustoClient;
    }

    private async Task<KustoClient> GetOrCreateCslQueryProviderAsync(string clusterUri, string? tenant, CancellationToken cancellationToken = default)
    {
        var providerCacheKey = GetProviderCacheKey(clusterUri, tenant, "query");
        var kustoClient = await _cacheService.GetAsync<KustoClient>(CacheGroup, providerCacheKey, s_providerCacheDuration, cancellationToken);
        if (kustoClient == null)
        {
            var tokenCredential = await GetCredential(tenant, cancellationToken);
            kustoClient = new KustoClient(clusterUri, tokenCredential, UserAgent, _httpClientFactory);
            await _cacheService.SetAsync(CacheGroup, providerCacheKey, kustoClient, s_providerCacheDuration, cancellationToken);
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
        Models.KustoClusterData? kustoCluster = Models.KustoClusterData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Kusto cluster data");

        if (string.IsNullOrEmpty(kustoCluster.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(kustoCluster.ResourceId);

        return new(
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
