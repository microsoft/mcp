// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.Tests;

public sealed class KustoClientTests
{
    private readonly TokenCredential _tokenCredential;
    private readonly IAzureService _azureService;

    public KustoClientTests()
    {
        _tokenCredential = Substitute.For<TokenCredential>();
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("noop-token", DateTimeOffset.UtcNow.AddHours(1)));

        _azureService = Substitute.For<IAzureService>();
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        _azureService.CloudConfiguration.Returns(cloudConfiguration);
    }

    [Fact]
    public async Task ExecuteCommandAsync_SetsTimeoutTo240Seconds()
    {
        // Arrange
        var tokenCredential = Substitute.For<TokenCredential>();
        tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("noop-token", DateTimeOffset.UtcNow.AddHours(1)));

        using var httpClient = new HttpClient(new MockHttpMessageHandler());

        var azureService = Substitute.For<IAzureService>();
        azureService.GetClient(Arg.Any<string>()).Returns(httpClient);

        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        var endpoint = KustoEndpoint.Create("https://test.kusto.windows.net", cloudConfiguration);
        var kustoClient = new KustoClient(endpoint, tokenCredential, "azmcp", azureService);

        // Act
        var result = await kustoClient.ExecuteQueryCommandAsync("testdb", "test query", CancellationToken.None);

        // Assert - verify the timeout was set to 240 seconds
        Assert.Equal(TimeSpan.FromSeconds(240), httpClient.Timeout);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteQueryCommandAsync_CustomCloud_UsesConfiguredScopeAndRequestTimeCredential()
    {
        const string tenant = "tenant-name";
        const string resolvedTenant = "00000000-0000-0000-0000-000000000001";
        const string scope = "https://custom-kusto.contoso.example/.default";
        Uri? requestUri = null;
        var tokens = new List<string?>();
        var azureService = Substitute.For<IAzureService>();
        azureService.GetClient(KustoClient.HttpClientName).Returns(_ =>
            new HttpClient(new CallbackHttpMessageHandler(request =>
            {
                requestUri = request.RequestUri;
                tokens.Add(request.Headers.Authorization?.Parameter);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"Tables": []}""")
                };
            })));
        var firstCredential = CreateCredential("first-token");
        var secondCredential = CreateCredential("second-token");
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.CustomCloud);
        cloudConfiguration.Kusto.Returns(
            new KustoCloudConfiguration(
                ".kusto.windows.net",
                "https://custom-kusto.contoso.example"));
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.ResolveTenantIdAsync(tenant, Arg.Any<CancellationToken>()).Returns(resolvedTenant);
        azureService.GetTokenCredentialAsync(resolvedTenant, Arg.Any<CancellationToken>())
            .Returns(firstCredential, secondCredential);
        var endpoint = KustoEndpoint.Create("https://cluster.kusto.windows.net", cloudConfiguration);
        var client = new KustoClient(endpoint, tenant, "azmcp", azureService);

        await client.ExecuteQueryCommandAsync(
            "database",
            "Table | take 1",
            TestContext.Current.CancellationToken);
        await client.ExecuteQueryCommandAsync(
            "database",
            "Table | take 1",
            TestContext.Current.CancellationToken);

        Assert.Equal(new Uri("https://cluster.kusto.windows.net/v1/rest/query"), requestUri);
        Assert.Equal(new[] { "first-token", "second-token" }, tokens);
        await azureService.Received(2).GetTokenCredentialAsync(
            resolvedTenant,
            TestContext.Current.CancellationToken);
        await firstCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => Assert.Single(context.Scopes) == scope),
            TestContext.Current.CancellationToken);
        await secondCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => Assert.Single(context.Scopes) == scope),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_RequestTimeCredential_ReusesTokenWhenCredentialIsUnchanged()
    {
        const string tenant = "tenant";
        const string resolvedTenant = "resolved-tenant";
        using var handler = new MockHttpMessageHandler();
        _azureService.GetClient(KustoClient.HttpClientName)
            .Returns(_ => new HttpClient(handler, disposeHandler: false));
        _azureService.ResolveTenantIdAsync(tenant, Arg.Any<CancellationToken>()).Returns(resolvedTenant);
        _azureService.GetTokenCredentialAsync(resolvedTenant, Arg.Any<CancellationToken>()).Returns(_tokenCredential);
        var endpoint = KustoEndpoint.Create("https://test.kusto.windows.net", _azureService.CloudConfiguration);
        var client = new KustoClient(endpoint, tenant, "azmcp", _azureService);
        var cancellationToken = TestContext.Current.CancellationToken;

        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await client.ExecuteControlCommandAsync("testdb", ".show tables", cancellationToken);

        await _azureService.Received(2).GetTokenCredentialAsync(resolvedTenant, cancellationToken);
        await _tokenCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => Assert.Single(context.Scopes) == endpoint.Scope),
            cancellationToken);
    }

    [Theory]
    [InlineData("https://test.kusto.windows.net", "https://kusto.kusto.windows.net/.default")]
    [InlineData("https://test.kusto.chinacloudapi.cn", "https://kusto.kusto.chinacloudapi.cn/.default")]
    [InlineData("https://test.kusto.usgovcloudapi.net", "https://kusto.kusto.usgovcloudapi.net/.default")]
    public async Task ExecuteCommandAsync_ReusesTokenForQueryAndControlRequests(string clusterUri, string scope)
    {
        var requestIds = new HashSet<string>();
        using var handler = new MockHttpMessageHandler(request =>
        {
            Assert.Equal("noop-token", request.Headers.Authorization?.Parameter);
            Assert.True(requestIds.Add(Assert.Single(request.Headers.GetValues("x-ms-client-request-id"))));
        });
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var client = CreateClient(clusterUri);
        var cancellationToken = TestContext.Current.CancellationToken;

        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await client.ExecuteQueryCommandAsync("testdb", "print value=2", cancellationToken);
        await client.ExecuteControlCommandAsync("testdb", ".show tables", cancellationToken);

        Assert.Equal(3, requestIds.Count);
        await _tokenCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(new[] { scope })),
            cancellationToken);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(2, false)]
    [InlineData(60, true)]
    public async Task ExecuteCommandAsync_RefreshesExpiredOrExpiringToken(int expiresInMinutes, bool refreshRequested)
    {
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(
                new AccessToken(
                    "old-token",
                    DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes),
                    refreshRequested ? DateTimeOffset.UtcNow.AddMinutes(-1) : null),
                new AccessToken("new-token", DateTimeOffset.UtcNow.AddHours(1)));
        var tokens = new List<string?>();
        using var handler = new MockHttpMessageHandler(request => tokens.Add(request.Headers.Authorization?.Parameter));
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var client = CreateClient("https://test.kusto.windows.net");
        var cancellationToken = TestContext.Current.CancellationToken;

        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);

        Assert.Equal(new[] { "old-token", "new-token", "new-token" }, tokens);
        await _tokenCredential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ConcurrentRequestsShareTokenAcquisition()
    {
        var tokenSource = new TaskCompletionSource<AccessToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ValueTask<AccessToken>(tokenSource.Task));
        using var handler = new MockHttpMessageHandler(request =>
            Assert.Equal("shared-token", request.Headers.Authorization?.Parameter));
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var client = CreateClient("https://test.kusto.windows.net");
        var cancellationToken = TestContext.Current.CancellationToken;

        var requests = Enumerable.Range(0, 10)
            .Select(_ => client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken))
            .ToArray();
        tokenSource.SetResult(new AccessToken("shared-token", DateTimeOffset.UtcNow.AddHours(1)));
        await Task.WhenAll(requests);

        await _tokenCredential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_TokenFailurePropagatesAndAllowsRetry()
    {
        var failure = new InvalidOperationException("Token acquisition failed.");
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(
                _ => throw failure,
                _ => new AccessToken("retry-token", DateTimeOffset.UtcNow.AddHours(1)));
        using var handler = new MockHttpMessageHandler(request =>
            Assert.Equal("retry-token", request.Headers.Authorization?.Parameter));
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var client = CreateClient("https://test.kusto.windows.net");
        var cancellationToken = TestContext.Current.CancellationToken;

        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken)));
        _azureService.DidNotReceive().GetClient(Arg.Any<string>());
        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);

        await _tokenCredential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CancelledWaitDoesNotCancelSharedAcquisition()
    {
        var tokenSource = new TaskCompletionSource<AccessToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ValueTask<AccessToken>(tokenSource.Task));
        using var handler = new MockHttpMessageHandler();
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var client = CreateClient("https://test.kusto.windows.net");
        var cancellationToken = TestContext.Current.CancellationToken;
        using var cancelled = new CancellationTokenSource();

        var firstRequest = client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        var waitingRequest = client.ExecuteQueryCommandAsync("testdb", "print value=1", cancelled.Token);
        await cancelled.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitingRequest);
        tokenSource.SetResult(new AccessToken("shared-token", DateTimeOffset.UtcNow.AddHours(1)));
        await firstRequest;
        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);

        await _tokenCredential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CancelledTokenAcquisitionAllowsRetry()
    {
        using var cancelled = new CancellationTokenSource();
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), cancelled.Token)
            .Returns(async call =>
            {
                await cancelled.CancelAsync();
                call.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return default;
            });
        using var handler = new MockHttpMessageHandler();
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var client = CreateClient("https://test.kusto.windows.net");
        var cancellationToken = TestContext.Current.CancellationToken;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.ExecuteQueryCommandAsync("testdb", "print value=1", cancelled.Token));
        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await client.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);

        await _tokenCredential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancelled.Token);
        await _tokenCredential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_DifferentClientsDoNotShareTokens()
    {
        var otherCredential = Substitute.For<TokenCredential>();
        otherCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("other-token", DateTimeOffset.UtcNow.AddHours(1)));
        var tokens = new List<string?>();
        using var handler = new MockHttpMessageHandler(request => tokens.Add(request.Headers.Authorization?.Parameter));
        _azureService.GetClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var endpoint = KustoEndpoint.Create("https://test.kusto.windows.net", _azureService.CloudConfiguration);
        var firstClient = new KustoClient(endpoint, _tokenCredential, "azmcp", _azureService);
        var secondClient = new KustoClient(endpoint, otherCredential, "azmcp", _azureService);
        var cancellationToken = TestContext.Current.CancellationToken;

        await firstClient.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await secondClient.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);
        await firstClient.ExecuteQueryCommandAsync("testdb", "print value=1", cancellationToken);

        Assert.Equal(new[] { "noop-token", "other-token", "noop-token" }, tokens);
        await _tokenCredential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
        await otherCredential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellationToken);
    }

    [Fact]
    public async Task ExecuteQueryCommandAsync_KeepsPathAndUsesKustoScope()
    {
        // Arrange - any cluster URI that passes the allow-list works here; this one includes a path to show it's preserved
        const string clusterUri = "https://mycluster.arg.core.windows.net/providers/Microsoft.ResourceGraph/connector/kusto/sample";
        var handler = new MockHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        _azureService.GetClient(Arg.Any<string>()).Returns(httpClient);
        var kustoClient = CreateClient(clusterUri);

        // Act
        await kustoClient.ExecuteQueryCommandAsync("testdb", "test query", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(new Uri($"{clusterUri}/v1/rest/query"), handler.LastRequestUri);
        await _tokenCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context =>
                context.Scopes.Length == 1 && context.Scopes[0] == "https://kusto.kusto.windows.net/.default"),
            Arg.Any<CancellationToken>());
    }

    #region SSRF Protection Tests

    [Theory]
    [InlineData("https://example.com/v1/rest/query")]
    [InlineData("https://external-server.com")]
    [InlineData("http://test.kusto.windows.net")] // HTTP instead of HTTPS
    [InlineData("https://kusto.windows.net.example.com")]
    [InlineData("https://test.kusto.windows.net.example.com/path")]
    [InlineData("https://169.254.169.254/metadata/instance")] // Azure IMDS
    [InlineData("https://management.azure.com/subscriptions")]
    [InlineData("https://arg.core.windows.net")] // Resource Graph suffix without an endpoint name
    [InlineData("https://mycluster.arg.core.windows.net.example.com")]
    public void Constructor_RejectsInvalidClusterUris(string invalidClusterUri)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(invalidClusterUri));

        Assert.Contains("Kusto cluster URI", exception.Message);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://test.kusto.windows.net")]
    [InlineData("file:///etc/passwd")]
    public void Constructor_RejectsNonHttpsSchemes(string invalidClusterUri)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateClient(invalidClusterUri));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsNullOrEmptyClusterUri(string? invalidClusterUri)
    {
        // Act & Assert - ArgumentNullException is thrown for null, ArgumentException for empty/whitespace
        Assert.ThrowsAny<ArgumentException>(
            () => CreateClient(invalidClusterUri!));
    }

    [Theory]
    [InlineData("https://mycluster.kusto.windows.net")]
    [InlineData("https://mycluster.westus.kusto.windows.net")]
    [InlineData("https://mycluster.eastus2.kusto.windows.net/")]
    [InlineData("https://test-cluster.northeurope.kusto.windows.net")]
    // China cloud
    [InlineData("https://mycluster.kusto.chinacloudapi.cn")]
    [InlineData("https://mycluster.eastus.kusto.chinacloudapi.cn")]
    // US Government
    [InlineData("https://mycluster.kusto.usgovcloudapi.net")]
    [InlineData("https://mycluster.usgovvirginia.kusto.usgovcloudapi.net")]
    // Dev
    [InlineData("https://mycluster.kustodev.windows.net")]
    // Fabric & Synapse
    [InlineData("https://mycluster.kusto.fabric.microsoft.com")]
    [InlineData("https://mycluster.kusto.azuresynapse.net")]
    // Log Analytics / App Insights
    [InlineData("https://mycluster.adx.loganalytics.azure.com")]
    [InlineData("https://mycluster.adx.applicationinsights.azure.com")]
    // Germany
    [InlineData("https://mycluster.kusto.sovcloud-api.de")]
    // Azure Resource Graph
    [InlineData("https://mycluster.arg.core.windows.net")]
    [InlineData("https://mycluster.arg.core.windows.net/providers/Microsoft.ResourceGraph/connector/kusto/sample")]
    public void Constructor_AcceptsValidKustoClusterUris(string validClusterUri)
    {
        // Act - should not throw
        var client = CreateClient(validClusterUri);

        // Assert - client was created successfully (no exception thrown)
        Assert.NotNull(client);
    }

    [Theory]
    // Public cloud exact hostnames
    [InlineData("https://kusto.aria.microsoft.com")]
    [InlineData("https://eu.kusto.aria.microsoft.com")]
    [InlineData("https://ade.applicationinsights.io")]
    [InlineData("https://ade.loganalytics.io")]
    [InlineData("https://adx.aimon.applicationinsights.azure.com")]
    [InlineData("https://adx.applicationinsights.azure.com")]
    [InlineData("https://adx.loganalytics.azure.com")]
    [InlineData("https://adx.monitor.azure.com")]
    // US Government exact hostnames
    [InlineData("https://adx.applicationinsights.azure.us")]
    [InlineData("https://adx.loganalytics.azure.us")]
    [InlineData("https://adx.monitor.azure.us")]
    // China exact hostnames
    [InlineData("https://adx.applicationinsights.azure.cn")]
    [InlineData("https://adx.loganalytics.azure.cn")]
    [InlineData("https://adx.monitor.azure.cn")]
    // Germany
    [InlineData("https://adx.applicationinsights.azure.de")]
    [InlineData("https://adx.loganalytics.azure.de")]
    [InlineData("https://adx.monitor.azure.de")]
    public void Constructor_AcceptsValidKustoExactHostnames(string validClusterUri)
    {
        // Act - should not throw
        var client = CreateClient(validClusterUri);

        // Assert - client was created successfully (no exception thrown)
        Assert.NotNull(client);
    }

    [Theory]
    // Single segment cluster names
    [InlineData("https://mycluster.kusto.windows.net")]
    [InlineData("https://a.kusto.windows.net")] // Single character
    [InlineData("https://cluster123.kusto.windows.net")] // With numbers
    [InlineData("https://my-cluster.kusto.windows.net")] // With hyphen
    [InlineData("https://my-cluster-name.kusto.windows.net")] // Multiple hyphens
    // Two segment cluster names (cluster.region)
    [InlineData("https://mycluster.westus.kusto.windows.net")]
    [InlineData("https://mycluster.eastus2.kusto.windows.net")]
    [InlineData("https://my-cluster.north-europe.kusto.windows.net")]
    // Three or more segment cluster names
    [InlineData("https://mycluster.sub1.sub2.kusto.windows.net")]
    [InlineData("https://a.b.c.d.kusto.windows.net")]
    [InlineData("https://cluster-1.region-2.zone-3.kusto.windows.net")]
    public void Constructor_AcceptsMultiSegmentClusterNames(string validClusterUri)
    {
        // Act - should not throw
        var client = CreateClient(validClusterUri);

        // Assert - client was created successfully (no exception thrown)
        Assert.NotNull(client);
    }

    [Theory]
    // Empty cluster name (just the suffix)
    [InlineData("https://.kusto.windows.net")]
    // Segment starting with hyphen
    [InlineData("https://-mycluster.kusto.windows.net")]
    [InlineData("https://mycluster.-region.kusto.windows.net")]
    // Empty segment (double dots)
    [InlineData("https://mycluster..kusto.windows.net")]
    [InlineData("https://mycluster.region..kusto.windows.net")]
    // Segment with invalid characters
    [InlineData("https://my_cluster.kusto.windows.net")] // Underscore
    [InlineData("https://my!cluster.kusto.windows.net")] // Exclamation
    // Segment with only hyphens (must start with alphanumeric)
    [InlineData("https://---.kusto.windows.net")]
    public void Constructor_RejectsInvalidClusterNameSegments(string invalidClusterUri)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateClient(invalidClusterUri));
    }

    #endregion

    private KustoClient CreateClient(string clusterUri)
    {
        var endpoint = KustoEndpoint.Create(clusterUri, _azureService.CloudConfiguration);
        return new KustoClient(endpoint, _tokenCredential, "azmcp", _azureService);
    }

    private static TokenCredential CreateCredential(string token)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        return credential;
    }

    private sealed class MockHttpMessageHandler(Action<HttpRequestMessage>? onRequest = null) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onRequest?.Invoke(request);
            LastRequestUri = request.RequestUri;

            // The caller (HttpClient.SendAsync) takes ownership and is responsible for disposal HttpResponseMessage.
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Tables": []}""", System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class CallbackHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> callback)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }
}
