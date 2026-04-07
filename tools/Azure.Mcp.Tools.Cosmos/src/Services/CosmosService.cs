// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.CosmosDB;
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(accountName), accountName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync(cancellationToken))
        {
            if (account.Data.Name == accountName)
            {
                return account;
            }
        }
        throw new Exception($"Cosmos DB account '{accountName}' not found in subscription '{subscription}'");
    }

    private async Task<CosmosClient> CreateCosmosClientWithAuth(
        string accountName,
        string subscription,
        AuthMethod authMethod,
        string? tenant = null,
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
            clientOptions.MaxRetryAttemptsOnRateLimitedRequests = retryPolicy.MaxRetries;
            clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
        }

        clientOptions.HttpClientFactory = () => _httpClientFactory.CreateClient();

        CosmosClient cosmosClient;
        switch (authMethod)
        {
            case AuthMethod.Key:
                var cosmosAccount = await GetCosmosAccountAsync(subscription, accountName, tenant, cancellationToken: cancellationToken);
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(subscription), subscription));

        var key = CacheKeyBuilder.Build(CosmosClientsCacheKeyPrefix, accountName, authMethod.ToString());
        var cosmosClient = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key, s_cacheDurationClients, cancellationToken);
        if (cosmosClient != null)
            return cosmosClient;

        cosmosClient = await CreateCosmosClientWithAuth(
            accountName,
            subscription,
            authMethod,
            tenant,
            retryPolicy,
            cancellationToken);

        await _cacheService.SetAsync(CacheGroup, key, cosmosClient, s_cacheDurationClients, cancellationToken);
        return cosmosClient;
    }

    public async Task<List<string>> GetCosmosAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var accounts = new List<string>();
        await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync(cancellationToken))
        {
            if (account?.Data?.Name != null)
            {
                accounts.Add(account.Data.Name);
            }
        }

        return accounts;
    }

    public async Task<List<string>> ListDatabases(
        string accountName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(subscription), subscription));

        var cacheKey = CacheKeyBuilder.Build(CosmosDatabasesCacheKeyPrefix, accountName);

        var cachedDatabases = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources, cancellationToken);
        if (cachedDatabases != null)
        {
            return cachedDatabases;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy, cancellationToken);
        var databases = new List<string>();

        var iterator = client.GetDatabaseQueryStreamIterator();
        while (iterator.HasMoreResults)
        {
            using ResponseMessage dbResponse = await iterator.ReadNextAsync(cancellationToken);
            if (!dbResponse.IsSuccessStatusCode)
            {
                throw new Exception(dbResponse.ErrorMessage);
            }
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(accountName), accountName), (nameof(databaseName), databaseName), (nameof(subscription), subscription));

        var cacheKey = CacheKeyBuilder.Build(CosmosContainersCacheKeyPrefix, accountName, databaseName);

        var cachedContainers = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources, cancellationToken);
        if (cachedContainers != null)
        {
            return cachedContainers;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy, cancellationToken);
        var containers = new List<string>();

        var database = client.GetDatabase(databaseName);
        var iterator = database.GetContainerQueryStreamIterator();
        while (iterator.HasMoreResults)
        {
            using ResponseMessage containerRResponse = await iterator.ReadNextAsync(cancellationToken);
            if (!containerRResponse.IsSuccessStatusCode)
            {
                throw new Exception(containerRResponse.ErrorMessage);
            }
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

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy, cancellationToken);

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
            using var document = JsonDocument.Parse(response.Content);
            items.Add(document.RootElement.Clone());
        }

        return items;
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
