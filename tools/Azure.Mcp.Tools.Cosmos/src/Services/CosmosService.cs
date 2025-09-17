// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
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

    // Maximum number of results to prevent DoS attacks and performance issues
    private const int MaxResultLimit = 1000;

    // Maximum query length to prevent DoS attacks
    private const int MaxQueryLength = 8000;

    // Regex timeout to prevent ReDoS attacks
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(3);

    // Static arrays for security validation - initialized once per class
    private static readonly string[] DangerousKeywords =
    [
        // Cosmos DB system functions that could be misused for accessing system metadata
        "UDF(", "AGGREGATE(",
        // Potentially dangerous operations (though Cosmos DB SQL is read-only by nature, these might be attempts at injection)
        "DELETE", "DROP", "CREATE", "ALTER", "INSERT", "UPDATE", "REPLACE", "UPSERT",
        // System procedure calls that shouldn't be accessible through queries
        "EXEC", "EXECUTE", "SP_", "STORED"
    ];

    private static readonly string[] SuspiciousPatterns =
    [
        // Potential injection patterns
        "UNION", "OR 1=1", "AND 1=1", "--", "/*", "*/",
        // Functions that might be used for expensive operations or potential misuse
        // Note: Removed some legitimate Cosmos DB functions that are commonly used
        "REGEXMATCH("
    ];

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
            var iterator = client.GetDatabaseQueryStreamIterator();
            while (iterator.HasMoreResults)
            {
                ResponseMessage dbResponse = await iterator.ReadNextAsync();
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
            var iterator = database.GetContainerQueryStreamIterator();
            while (iterator.HasMoreResults)
            {
                ResponseMessage containerRResponse = await iterator.ReadNextAsync();
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

        await _cacheService.SetAsync(CacheGroup, cacheKey, containers, s_cacheDurationResources);
        return containers;
    }

    private static void ValidateQuerySafety(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        }

        // Prevent DoS attacks by limiting query length
        if (query.Length > MaxQueryLength)
        {
            throw new InvalidOperationException($"Query length exceeds the maximum allowed limit of {MaxQueryLength:N0} characters to prevent potential DoS attacks.");
        }

        // Clean the query: remove comments, normalize whitespace, and trim
        var cleanedQuery = query;

        // Remove line comments (-- comment)
        cleanedQuery = Regex.Replace(cleanedQuery, @"--.*?$", "", RegexOptions.Multiline, RegexTimeout);

        // Remove block comments (/* comment */)
        cleanedQuery = Regex.Replace(cleanedQuery, @"/\*.*?\*/", "", RegexOptions.Singleline, RegexTimeout);

        // Normalize whitespace: replace multiple whitespace characters with single space
        cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ", RegexOptions.Multiline, RegexTimeout);

        // Trim the result
        cleanedQuery = cleanedQuery.Trim();

        // Ensure the cleaned query is not empty
        if (string.IsNullOrWhiteSpace(cleanedQuery))
        {
            throw new ArgumentException("Query cannot be empty after removing comments and whitespace.", nameof(query));
        }

        // Check for multiple statements (semicolons followed by non-whitespace)
        var multipleStatementsPattern = new Regex(
            @";\s*\w",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            RegexTimeout
        );

        if (multipleStatementsPattern.IsMatch(cleanedQuery))
        {
            throw new InvalidOperationException("Multiple SQL statements are not allowed. Use only a single SELECT statement.");
        }

        var queryUpper = cleanedQuery.ToUpperInvariant();

        // Check for dangerous keywords
        foreach (var keyword in DangerousKeywords)
        {
            if (queryUpper.Contains(keyword))
            {
                throw new InvalidOperationException($"Query contains dangerous keyword '{keyword}' which is not allowed for security reasons.");
            }
        }

        // Check for suspicious patterns that might indicate injection attempts
        foreach (var pattern in SuspiciousPatterns)
        {
            if (queryUpper.Contains(pattern))
            {
                throw new InvalidOperationException($"Query contains suspicious pattern '{pattern}' which is not allowed for security reasons.");
            }
        }

        // Additional validation: Only allow SELECT statements for Cosmos DB
        var trimmedQuery = queryUpper.Trim();
        if (!trimmedQuery.StartsWith("SELECT"))
        {
            throw new InvalidOperationException("Only SELECT statements are allowed for security reasons.");
        }

        // Check for excessive use of wildcards which could cause performance issues
        var wildcardCount = Regex.Matches(queryUpper, @"\*", RegexOptions.None, RegexTimeout).Count;
        if (wildcardCount > 5)
        {
            throw new InvalidOperationException("Query contains too many wildcard operators (*) which could cause performance issues.");
        }

        // Limit nested function calls to prevent complex queries that could impact performance
        var nestedFunctionCount = Regex.Matches(queryUpper, @"\w+\s*\(", RegexOptions.None, RegexTimeout).Count;
        if (nestedFunctionCount > 10)
        {
            throw new InvalidOperationException("Query contains too many function calls which could cause performance issues.");
        }
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

            // Validate query safety before execution
            ValidateQuerySafety(baseQuery);

            var queryDef = new QueryDefinition(baseQuery);

            var items = new List<JsonElement>();
            var queryIterator = container.GetItemQueryStreamIterator(
                queryDef,
                requestOptions: new QueryRequestOptions { MaxItemCount = MaxResultLimit }
            );

            int totalItemsProcessed = 0;

            while (queryIterator.HasMoreResults && totalItemsProcessed < MaxResultLimit)
            {
                var response = await queryIterator.ReadNextAsync();
                using var document = JsonDocument.Parse(response.Content);

                if (document.RootElement.TryGetProperty("Documents", out var documentsElement))
                {
                    foreach (var item in documentsElement.EnumerateArray())
                    {
                        if (totalItemsProcessed >= MaxResultLimit)
                        {
                            break;
                        }
                        items.Add(item.Clone());
                        totalItemsProcessed++;
                    }
                }
                else
                {
                    // Fallback for cases where response structure might differ
                    items.Add(document.RootElement.Clone());
                    totalItemsProcessed++;
                }
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
