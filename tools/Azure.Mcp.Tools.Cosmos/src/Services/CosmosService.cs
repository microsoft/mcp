// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.ResourceManager.CosmosDB;
using Microsoft.Azure.Cosmos;

namespace Azure.Mcp.Tools.Cosmos.Services;

public class CosmosService(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService)
    : BaseAzureService(tenantService), ICosmosService, IDisposable
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, accountName);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync())
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
        RetryPolicyOptions? retryPolicy = null)
    {
        var clientOptions = new CosmosClientOptions { AllowBulkExecution = true };
        clientOptions.CosmosClientTelemetryOptions.DisableDistributedTracing = false;
        clientOptions.CustomHandlers.Add(new UserPolicyRequestHandler(UserAgent));

        if (retryPolicy != null)
        {
            clientOptions.MaxRetryAttemptsOnRateLimitedRequests = retryPolicy.MaxRetries;
            clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
        }

        CosmosClient cosmosClient;
        switch (authMethod)
        {
            case AuthMethod.Key:
                var cosmosAccount = await GetCosmosAccountAsync(subscription, accountName, tenant);
                var keys = await cosmosAccount.GetKeysAsync();
                cosmosClient = new CosmosClient(
                    string.Format(CosmosBaseUri, accountName),
                    keys.Value.PrimaryMasterKey,
                    clientOptions);
                break;

            case AuthMethod.Credential:
            default:
                cosmosClient = new CosmosClient(
                    string.Format(CosmosBaseUri, accountName),
                    await GetCredential(tenant),
                    clientOptions);
                break;
        }

        // Validate the client by performing a lightweight operation
        await ValidateCosmosClientAsync(cosmosClient);

        return cosmosClient;
    }

    private async Task ValidateCosmosClientAsync(CosmosClient client)
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(accountName, subscription);

        var key = CosmosClientsCacheKeyPrefix + accountName;
        var cosmosClient = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key, s_cacheDurationResources);
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
                retryPolicy);

            await _cacheService.SetAsync(CacheGroup, key, cosmosClient, s_cacheDurationResources);
            return cosmosClient;
        }
        catch (Exception ex) when (
            authMethod == AuthMethod.Credential &&
            (ex.Message.Contains("401") || ex.Message.Contains("403")))
        {
            // If credential auth fails with 401/403, try key auth
            cosmosClient = await CreateCosmosClientWithAuth(
                accountName,
                subscription,
                AuthMethod.Key,
                tenant,
                retryPolicy);

            await _cacheService.SetAsync(CacheGroup, key, cosmosClient, s_cacheDurationResources);
            return cosmosClient;
        }

        throw new Exception($"Failed to create Cosmos client for account '{accountName}' with any authentication method");
    }

    public async Task<List<string>> GetCosmosAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var accounts = new List<string>();
        try
        {
            await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync())
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(accountName, subscription);

        var cacheKey = CosmosDatabasesCacheKeyPrefix + accountName;

        var cachedDatabases = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources);
        if (cachedDatabases != null)
        {
            return cachedDatabases;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy);
        var databases = new List<string>();

        try
        {
            var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                databases.AddRange(results.Select(r => r.Id));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing databases: {ex.Message}", ex);
        }

        await _cacheService.SetAsync(CacheGroup, cacheKey, databases, s_cacheDurationResources);
        return databases;
    }

    public async Task<List<string>> ListContainers(
        string accountName,
        string databaseName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(accountName, databaseName, subscription);

        var cacheKey = CosmosContainersCacheKeyPrefix + accountName + "_" + databaseName;

        var cachedContainers = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDurationResources);
        if (cachedContainers != null)
        {
            return cachedContainers;
        }

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy);
        var containers = new List<string>();

        try
        {
            var database = client.GetDatabase(databaseName);
            var iterator = database.GetContainerQueryIterator<ContainerProperties>();
            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                containers.AddRange(results.Select(r => r.Id));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing containers: {ex.Message}", ex);
        }

        await _cacheService.SetAsync(CacheGroup, cacheKey, containers, s_cacheDurationResources);
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(accountName, databaseName, containerName, subscription);

        var client = await GetCosmosClientAsync(accountName, subscription, authMethod, tenant, retryPolicy);

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
                var response = await queryIterator.ReadNextAsync();
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

    protected virtual async void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Get all cached client keys
                var keys = await _cacheService.GetGroupKeysAsync(CacheGroup);

                // Filter for client keys only (those that start with the client prefix)
                var clientKeys = keys.Where(k => k.StartsWith(CosmosClientsCacheKeyPrefix));

                // Retrieve and dispose each client
                foreach (var key in clientKeys)
                {
                    var client = await _cacheService.GetAsync<CosmosClient>(CacheGroup, key);
                    client?.Dispose();
                }
                _disposed = true;
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
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
