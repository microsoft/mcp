// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests;

public class CosmosServiceTests : IAsyncDisposable
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CosmosService> _logger;
    private readonly CosmosService _service;

    public CosmosServiceTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _cacheService = Substitute.For<ICacheService>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _logger = Substitute.For<ILogger<CosmosService>>();

        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        _tenantService.CloudConfiguration.Returns(cloudConfig);

        var credential = Substitute.For<TokenCredential>();
        _tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));

        _cacheService.GetGroupKeysAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<IEnumerable<string>>(Enumerable.Empty<string>()));

        _service = new CosmosService(_subscriptionService, _tenantService, _cacheService, _httpClientFactory, _logger);
    }

    public async ValueTask DisposeAsync()
    {
        await _service.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData("https://other-server.com/")]
    [InlineData("https://aoai.openai.azure.com.other.com/")]
    [InlineData("http://aoai.openai.azure.com/")]
    [InlineData("https://attacker.com#.openai.azure.com")]
    [InlineData("https://attacker.com#openai.azure.com")]
    [InlineData("https://attacker.com/#.openai.azure.com")]
    [InlineData("https://attacker.com?x=.openai.azure.com")]
    public async Task GenerateEmbedding_RejectsUntrustedEndpoint(string endpoint)
    {
        var request = new EmbeddingRequest(endpoint, "my-deployment", null);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GenerateEmbedding("hello", request, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Azure OpenAI endpoint", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListDatabases_CredentialAuthFails_DoesNotFallBackToKeyAuth()
    {
        // Arrange: HTTP handler returns 401 so credential-based CosmosClient validation fails
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act & Assert: exception should propagate, not be silently caught
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Credential, cancellationToken: TestContext.Current.CancellationToken));

        // Verify no fallback to key auth: GetSubscription is only called for key-based auth
        // (to look up the account and retrieve master keys)
        await _subscriptionService.DidNotReceive()
            .GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListDatabases_CredentialAuthFailsWith403_DoesNotFallBackToKeyAuth()
    {
        // Arrange: HTTP handler returns 403 so credential-based CosmosClient validation fails
        var handler = new MockHttpHandler(HttpStatusCode.Forbidden);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act & Assert: exception should propagate
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Credential, cancellationToken: TestContext.Current.CancellationToken));

        // Verify no fallback to key auth
        await _subscriptionService.DidNotReceive()
            .GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCosmosClientAsync_CredentialAuthRequest_QueriesCacheWithCredentialKey()
    {
        // Arrange
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: client creation fails, but cache lookup has already happened
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Credential, cancellationToken: TestContext.Current.CancellationToken));

        // Assert: cache was queried with the credential-specific key
        await _cacheService.Received().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", string.Empty, "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        // Assert: the key-auth cache key was NOT queried (no cross-contamination)
        await _cacheService.DidNotReceive().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", string.Empty, "Key"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCosmosClientAsync_KeyAuthRequest_QueriesCacheWithKeyAuthKey()
    {
        // Arrange: _subscriptionService returns null/throws by default, causing key-based creation to fail
        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: client creation fails after the cache miss, but cache lookup has already happened
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Key, cancellationToken: TestContext.Current.CancellationToken));

        // Assert: cache was queried with the key-auth-specific key
        await _cacheService.Received().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", string.Empty, "Key"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        // Assert: the credential cache key was NOT queried (no cross-contamination)
        await _cacheService.DidNotReceive().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", string.Empty, "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCosmosClientAsync_SameAccount_DifferentAuthMethods_UseSeparateCacheKeys()
    {
        // Arrange: simulate a server that always returns 401 so client creation always fails
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: both calls fail on client creation, but both perform a cache lookup first
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Credential, cancellationToken: TestContext.Current.CancellationToken));

        _cacheService.ClearReceivedCalls();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Key, cancellationToken: TestContext.Current.CancellationToken));

        // Assert: the Key request queries the key-auth client key, NOT the credential client key
        // This proves a Key-cached client can never be served to a Credential request and vice versa.
        await _cacheService.Received().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", string.Empty, "Key"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        await _cacheService.DidNotReceive().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", string.Empty, "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCosmosAccounts_ResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: the resource group lookup returns a 404, which should surface as a not-found.
        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription("sub123", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(subscriptionResource);
        subscriptionResource.GetResourceGroupAsync("missing-rg", Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Resource group not found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetCosmosAccounts("sub123", "missing-rg", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Resource group 'missing-rg' not found", ex.Message);
    }

    [Fact]
    public async Task ListDatabases_KeyAuthResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: key auth resolves the account through the resource group fast path; a missing
        // resource group should surface as a KeyNotFoundException (HTTP 404) rather than a raw 404.
        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription("sub123", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(subscriptionResource);
        subscriptionResource.GetResourceGroupAsync("missing-rg", Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Resource group not found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Key, resourceGroup: "missing-rg", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Resource group 'missing-rg' not found", ex.Message);
    }

    [Fact]
    public async Task GetCosmosClientAsync_SameAccountAndAuth_DifferentSubscriptions_UseSeparateCacheKeys()
    {
        // Arrange
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: same account and auth method, but two different subscriptions
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub-A", AuthMethod.Credential, cancellationToken: TestContext.Current.CancellationToken));

        _cacheService.ClearReceivedCalls();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub-B", AuthMethod.Credential, cancellationToken: TestContext.Current.CancellationToken));

        // Assert: the second subscription queries its own client key, never sub-A's cached client
        await _cacheService.Received().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub-B", string.Empty, "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        await _cacheService.DidNotReceive().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub-A", string.Empty, "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCosmosClientAsync_SameAccountAndAuth_DifferentTenants_UseSeparateCacheKeys()
    {
        // Arrange
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: same account, subscription, and auth method, but two different tenants
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Credential, "tenant-A", cancellationToken: TestContext.Current.CancellationToken));

        _cacheService.ClearReceivedCalls();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub123", AuthMethod.Credential, "tenant-B", cancellationToken: TestContext.Current.CancellationToken));

        // Assert: tenant-B queries its own client key, never tenant-A's cached client
        await _cacheService.Received().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", "tenant-B", "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        await _cacheService.DidNotReceive().GetAsync<CosmosClient>(
            "cosmos",
            CacheKeyBuilder.Build("clients", "myaccount", "sub123", "tenant-A", "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListDatabases_DifferentSubscriptionTenantOrAuth_UseSeparateCacheKeys()
    {
        // Arrange
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: first caller for account "myaccount" on sub-A / tenant-A via Credential
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub-A", AuthMethod.Credential, "tenant-A", cancellationToken: TestContext.Current.CancellationToken));

        // Assert: the database-list cache key is scoped to subscription, tenant, and auth method
        await _cacheService.Received().GetAsync<List<string>>(
            "cosmos",
            CacheKeyBuilder.Build("databases", "myaccount", "sub-A", "tenant-A", "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        _cacheService.ClearReceivedCalls();

        // Act: a different caller for the SAME account but different sub/tenant/auth
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListDatabases("myaccount", "sub-B", AuthMethod.Key, "tenant-B", cancellationToken: TestContext.Current.CancellationToken));

        // Assert: it queries its own scoped key and never the first caller's cached database list
        await _cacheService.Received().GetAsync<List<string>>(
            "cosmos",
            CacheKeyBuilder.Build("databases", "myaccount", "sub-B", "tenant-B", "Key"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        await _cacheService.DidNotReceive().GetAsync<List<string>>(
            "cosmos",
            CacheKeyBuilder.Build("databases", "myaccount", "sub-A", "tenant-A", "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListContainers_DifferentSubscriptionTenantOrAuth_UseSeparateCacheKeys()
    {
        // Arrange
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(CosmosClient));
        _cacheService.GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(default(List<string>));

        // Act: first caller for account/database on sub-A / tenant-A via Credential
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListContainers("myaccount", "mydb", "sub-A", AuthMethod.Credential, "tenant-A", cancellationToken: TestContext.Current.CancellationToken));

        // Assert: the container-list cache key is scoped to subscription, tenant, and auth method
        await _cacheService.Received().GetAsync<List<string>>(
            "cosmos",
            CacheKeyBuilder.Build("containers", "myaccount", "mydb", "sub-A", "tenant-A", "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        _cacheService.ClearReceivedCalls();

        // Act: a different caller for the SAME account/database but different sub/tenant/auth
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.ListContainers("myaccount", "mydb", "sub-B", AuthMethod.Key, "tenant-B", cancellationToken: TestContext.Current.CancellationToken));

        // Assert: it queries its own scoped key and never the first caller's cached container list
        await _cacheService.Received().GetAsync<List<string>>(
            "cosmos",
            CacheKeyBuilder.Build("containers", "myaccount", "mydb", "sub-B", "tenant-B", "Key"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        await _cacheService.DidNotReceive().GetAsync<List<string>>(
            "cosmos",
            CacheKeyBuilder.Build("containers", "myaccount", "mydb", "sub-A", "tenant-A", "Credential"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryItems_NonSuccessResponse_Throws()
    {
        // Arrange: route the CosmosClient's transport through the mocked IHttpClientFactory so it always returns 403.
        var handler = new MockHttpHandler(HttpStatusCode.Forbidden);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
        var clientOptions = new CosmosClientOptions { HttpClientFactory = () => _httpClientFactory.CreateClient() };

        var credential = Substitute.For<TokenCredential>();
        var token = new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1));
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(token));
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(token);

        using var cosmosClient = new CosmosClient("https://myaccount.documents.azure.com", credential, clientOptions);

        _cacheService.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(cosmosClient);

        // Act & Assert: a non-success response must throw rather than being returned as a data item
        await Assert.ThrowsAnyAsync<CosmosException>(() =>
            _service.QueryItems("myaccount", "mydb", "mycontainer", "SELECT * FROM c", "sub123", AuthMethod.Key, cancellationToken: TestContext.Current.CancellationToken));
    }

    private sealed class MockHttpHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
