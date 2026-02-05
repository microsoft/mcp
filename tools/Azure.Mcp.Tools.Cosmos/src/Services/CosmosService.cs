// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.ResourceManager.CosmosDB;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Cosmos.Services;

public sealed class CosmosService(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService, IHttpClientFactory httpClientFactory, ILogger<CosmosService> logger)
    : BaseAzureService(tenantService), ICosmosService, IAsyncDisposable
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<CosmosService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string CosmosBaseUri = "https://{0}.documents.azure.com:443/";
    private const string CacheGroup = "cosmos";
    private const string CosmosClientsCacheKeyPrefix = "clients_";
    private const string CosmosDatabasesCacheKeyPrefix = "databases_";
    private const string CosmosContainersCacheKeyPrefix = "containers_";
    private static readonly TimeSpan s_cacheDurationResources = TimeSpan.FromMinutes(15);
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
                cosmosClient = new CosmosClient(
                    string.Format(CosmosBaseUri, accountName),
                    keys.Value.PrimaryMasterKey,
                    clientOptions);
                break;

            case AuthMethod.Credential:
            default:
                cosmosClient = new CosmosClient(
                    string.Format(CosmosBaseUri, accountName),
                    await GetCredential(cancellationToken),
                    clientOptions);
                break;
        }

        // Validate the client by performing a lightweight operation
        await ValidateCosmosClientAsync(cosmosClient, cancellationToken);

        return cosmosClient;
    }

    private async Task ValidateCosmosClientAsync(CosmosClient client, CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform a lightweight operation to validate the client
            await client.ReadAccountAsync();
        }
        catch (CosmosException ex)
        {
            throw new Exception($"Failed to validate CosmosClient: {ex.StatusCode} - {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error while validating CosmosClient: {ex.Message}", ex);
        }
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

        var key = CosmosClientsCacheKeyPrefix + accountName;
        var cosmosClient = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key, s_cacheDurationResources, cancellationToken);
        if (cosmosClient != null)
            return cosmosClient;

        try
        {
            // First attempt with requested auth method
            cosmosClient = await CreateCosmosClientWithAuth(
                accountName,
                subscription,
                authMethod,
                tenant,
                retryPolicy,
                cancellationToken);

            await _cacheService.SetAsync(CacheGroup, key, cosmosClient, s_cacheDurationResources, cancellationToken);
            return cosmosClient;
        }
        catch (Exception ex) when (
            authMethod == AuthMethod.Credential &&
            (ex.Message.Contains(((int)HttpStatusCode.Unauthorized).ToString()) || ex.Message.Contains(((int)HttpStatusCode.Forbidden).ToString())))
        {
            // If credential auth fails with 401/403, try key auth
            cosmosClient = await CreateCosmosClientWithAuth(
                accountName,
                subscription,
                AuthMethod.Key,
                tenant,
                retryPolicy,
                cancellationToken);

            await _cacheService.SetAsync(CacheGroup, key, cosmosClient, s_cacheDurationResources, cancellationToken);
            return cosmosClient;
        }

        throw new Exception($"Failed to create Cosmos client for account '{accountName}' with any authentication method");
    }

    public async Task<List<string>> GetCosmosAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var accounts = new List<string>();
        try
        {
            await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync(cancellationToken))
            {
                if (account?.Data?.Name != null)
                {
                    accounts.Add(account.Data.Name);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Cosmos DB accounts: {ex.Message}", ex);
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

        var cacheKey = CosmosDatabasesCacheKeyPrefix + accountName;

        var cachedDatabases = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources, cancellationToken);
        if (cachedDatabases != null)
        {
            return cachedDatabases;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy, cancellationToken);
        var databases = new List<string>();

        try
        {
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
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing databases in the account '{accountName}': {ex.Message}", ex);
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

        var cacheKey = CosmosContainersCacheKeyPrefix + accountName + "_" + databaseName;

        var cachedContainers = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources, cancellationToken);
        if (cachedContainers != null)
        {
            return cachedContainers;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy, cancellationToken);
        var containers = new List<string>();

        try
        {
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
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing containers in database '{databaseName}' of account '{accountName}': {ex.Message}", ex);
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

        try
        {
            var container = client.GetContainer(databaseName, containerName);
            var baseQuery = string.IsNullOrEmpty(query) ? "SELECT * FROM c" : query;
            var queryDef = new QueryDefinition(baseQuery);

            var items = new List<JsonElement>();
            var queryIterator = container.GetItemQueryStreamIterator(
                queryDef,
                requestOptions: new QueryRequestOptions { MaxItemCount = -1 }
            );

            while (queryIterator.HasMoreResults)
            {
                using ResponseMessage response = await queryIterator.ReadNextAsync(cancellationToken);
                using var document = JsonDocument.Parse(response.Content);
                items.Add(document.RootElement.Clone());
            }

            return items;
        }
        catch (CosmosException ex)
        {
            throw new Exception($"Cosmos DB error occurred while querying items: {ex.StatusCode} - {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error querying items: {ex.Message}", ex);
        }
    }

    private async ValueTask DisposeAsyncCore()
    {
        IEnumerable<string> keys;
        try
        {
            // Get all cached client keys
            keys = await _cacheService.GetGroupKeysAsync(CacheGroup, CancellationToken.None);
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
                var client = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key);
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
