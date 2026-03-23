// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.ResourceManager;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

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
