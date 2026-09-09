// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Helpers;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

public class BaseAzureServiceTests
{
    private const string TenantId = "test-tenant-id";
    private const string TenantName = "test-tenant-name";

    private readonly IAzureService _azureService = Substitute.For<IAzureService>();
    private readonly TestAzureService _testAzureService;

    public BaseAzureServiceTests()
    {
        // Mock CloudConfiguration to return a valid ArmEnvironment
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        _azureService.CloudConfiguration.Returns(cloudConfig);

        _testAzureService = new TestAzureService(_azureService);
        _azureService.GetTenantId(TenantName, Arg.Any<CancellationToken>()).Returns(TenantId);
        _azureService.GetTokenCredentialAsync(
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Substitute.For<TokenCredential>());
        _azureService.GetClient().Returns(_ => new HttpClient(new HttpClientHandler()));
    }

    [Fact]
    public async Task CreateArmClientAsync_DoesNotReuseClient()
    {
        // Act
        var tenantName2 = "Other-Tenant-Name";
        var tenantId2 = "Other-Tenant-Id";

        _azureService.GetTenantId(tenantName2, Arg.Any<CancellationToken>()).Returns(tenantId2);

        var client = await _testAzureService.GetArmClientAsync(TenantName);
        var client2 = await _testAzureService.GetArmClientAsync(TenantName);

        Assert.NotEqual(client, client2);

        var otherClient = await _testAzureService.GetArmClientAsync(tenantName2);

        Assert.NotEqual(client, otherClient);

        // Not tested: we'd like to, but can't, verify the TokenCredential is reused
        // between client and client2 but NOT with otherClient. ArmClient doesn't expose
        // the credential nor the HttpPipeline the credential is included within.
    }

    [Fact]
    public void EscapeKqlString_EscapesSingleQuotes()
    {
        // Arrange
        var input = "resource'with'quotes";
        var expected = "resource''with''quotes";

        // Act
        var result = TestAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_EscapesBackslashes()
    {
        // Arrange
        var input = @"resource\with\backslashes";
        var expected = @"resource\\with\\backslashes";

        // Act
        var result = TestAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_EscapesBothQuotesAndBackslashes()
    {
        // Arrange
        var input = @"resource\'with\'mixed";
        var expected = @"resource\\''with\\''mixed";

        // Act
        var result = TestAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_HandlesNullAndEmptyStrings()
    {
        // Act & Assert
        Assert.Equal(string.Empty, TestAzureService.EscapeKqlStringTest(null!));
        Assert.Equal(string.Empty, TestAzureService.EscapeKqlStringTest(string.Empty));
    }

    [Fact]
    public void EscapeKqlString_HandlesRegularStringsWithoutEscaping()
    {
        // Arrange
        var input = "regular-resource-name";

        // Act
        var result = TestAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(input, result);
    }

    [Fact]
    public void InitializeUserAgentPolicy_UserAgentContainsTransportType()
    {
        // Initialize the user agent policy before creating test service
        BaseAzureService.InitializeUserAgentPolicy(TransportTypes.StdIo);
        TestAzureService testAzureService = new TestAzureService(_azureService);
        Assert.NotNull(testAzureService.GetUserAgent());
        Assert.Contains("azmcp-stdio", testAzureService.GetUserAgent());
    }

    [Fact]
    public void InitializeUserAgentPolicy_ThrowsExceptionWhenTransportTypeIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BaseAzureService.InitializeUserAgentPolicy(null!));
        Assert.Equal("Value cannot be null. (Parameter 'transportType')", exception.Message);
    }

    [Fact]
    public void InitializeUserAgentPolicy_ThrowsExceptionWhenTransportTypeIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() => BaseAzureService.InitializeUserAgentPolicy(string.Empty));
        Assert.Equal("The value cannot be an empty string or composed entirely of whitespace. (Parameter 'transportType')", exception.Message);
    }

    [Fact]
    public async Task GetArmAccessTokenAsync_UsesArmDefaultScope()
    {
        // Arrange
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("token", DateTimeOffset.UtcNow.AddHours(1))));
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential);

        // Act
        await _testAzureService.GetArmAccessTokenPublicAsync(TestContext.Current.CancellationToken);

        // Assert: ARM default scope is passed in the TokenRequestContext
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(ctx => ctx.Scopes.Contains(ArmEnvironment.AzurePublicCloud.DefaultScope)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetArmAccessTokenAsync_ForwardsTenantIdToGetTokenCredentialAsync()
    {
        // Arrange
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("token", DateTimeOffset.UtcNow.AddHours(1)));
        _azureService.ResolveTenantIdAsync(TenantName, Arg.Any<CancellationToken>())
            .Returns(TenantId);
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential);

        // Act
        await _testAzureService.GetArmAccessTokenPublicAsync(TenantName, TestContext.Current.CancellationToken);

        // Assert: TenantName is resolved to TenantId and forwarded to GetTokenCredentialAsync
        await _azureService.Received(1).GetTokenCredentialAsync(TenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetArmAccessTokenAsync_NullTenant_PassesNullToGetTokenCredentialAsync()
    {
        // Arrange
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("token", DateTimeOffset.UtcNow.AddHours(1)));
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential);

        // Act
        await _testAzureService.GetArmAccessTokenPublicAsync(TestContext.Current.CancellationToken);

        // Assert: null tenant is passed through as null
        await _azureService.Received(1).GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(5, true, 5)]   // below cap, should remain unchanged
    [InlineData(10, true, 10)] // at cap, should remain unchanged
    [InlineData(20, true, 10)] // above cap, should be capped
    [InlineData(20, false, null)] // null MaxRetries, should not override default
    public void ConfigureRetryPolicy_RespectsAndCapsMaxRetries(int maxRetries, bool hasMaxRetries, int? expectedMaxRetries)
    {
        // Arrange
        var retryPolicy = new RetryPolicyOptions { MaxRetries = hasMaxRetries ? maxRetries : null };
        var clientOptions = new ArmClientOptions();
        var defaultClientOptions = new ArmClientOptions();
        // Act
        TestAzureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);
        // Assert
        var expected = expectedMaxRetries ?? defaultClientOptions.Retry.MaxRetries;
        Assert.Equal(expected, clientOptions.Retry.MaxRetries);
    }

    [Theory]
    [InlineData(0.5, true, 0.5)]    // within bounds, should remain unchanged
    [InlineData(60.0, true, 60.0)]       // at upper cap, should remain unchanged
    [InlineData(0.1, true, 0.1)]    // at lower cap, should remain unchanged
    [InlineData(120.0, true, 60.0)]      // above upper cap, should be capped to 60
    [InlineData(0.01, true, 0.1)]   // below lower cap, should be raised to 0.1
    [InlineData(120.0, false, null)]   // null DelaySeconds, should not override default
    public void ConfigureRetryPolicy_RespectsAndClampsDelay(double delaySeconds, bool hasDelay, double? expectedDelay)
    {
        // Arrange
        var retryPolicy = new RetryPolicyOptions { DelaySeconds = hasDelay ? delaySeconds : null };
        var clientOptions = new ArmClientOptions();
        var defaultClientOptions = new ArmClientOptions();
        // Act
        TestAzureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);
        // Assert
        var expected = expectedDelay != null ? TimeSpan.FromSeconds(expectedDelay.Value) : defaultClientOptions.Retry.Delay;
        Assert.Equal(expected, clientOptions.Retry.Delay);
    }

    [Theory]
    [InlineData(5.0, true, 5.0)]        // within bounds, should remain unchanged
    [InlineData(60.0, true, 60.0)]       // at upper cap, should remain unchanged
    [InlineData(0.1, true, 0.1)]    // at lower cap, should remain unchanged
    [InlineData(120.0, true, 60.0)]      // above upper cap, should be capped to 60
    [InlineData(0.01, true, 0.1)]   // below lower cap, should be raised to 0.1
    [InlineData(120.0, false, null)]   // null MaxDelaySeconds, should not override default
    public void ConfigureRetryPolicy_RespectsAndClampsMaxDelay(double maxDelaySeconds, bool hasMaxDelay, double? expectedMaxDelay)
    {
        // Arrange
        var retryPolicy = new RetryPolicyOptions { MaxDelaySeconds = hasMaxDelay ? maxDelaySeconds : null };
        var clientOptions = new ArmClientOptions();
        var defaultClientOptions = new ArmClientOptions();
        // Act
        TestAzureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);
        // Assert
        var expected = expectedMaxDelay != null ? TimeSpan.FromSeconds(expectedMaxDelay.Value) : defaultClientOptions.Retry.MaxDelay;
        Assert.Equal(expected, clientOptions.Retry.MaxDelay);
    }

    [Theory]
    [InlineData(30.0, true, 30.0)]       // within bounds, should remain unchanged
    [InlineData(300.0, true, 300.0)]     // at cap, should remain unchanged
    [InlineData(600.0, true, 300.0)]     // above cap, should be capped to 300
    [InlineData(600.0, false, null)]   // null NetworkTimeoutSeconds, should not override default
    public void ConfigureRetryPolicy_RespectsAndCapsNetworkTimeout(double networkTimeoutSeconds, bool hasNetworkTimeout, double? expectedTimeout)
    {
        // Arrange
        var retryPolicy = new RetryPolicyOptions { NetworkTimeoutSeconds = hasNetworkTimeout ? networkTimeoutSeconds : null };
        var clientOptions = new ArmClientOptions();
        var defaultClientOptions = new ArmClientOptions();
        // Act
        TestAzureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);
        // Assert
        var expected = expectedTimeout != null ? TimeSpan.FromSeconds(expectedTimeout.Value) : defaultClientOptions.Retry.NetworkTimeout;
        Assert.Equal(expected, clientOptions.Retry.NetworkTimeout);
    }

    private sealed class TestAzureService(IAzureService azureService) : BaseAzureService(azureService)
    {
        public Task<ArmClient> GetArmClientAsync(string? tenant = null) =>
            CreateArmClientAsync(tenant);

        public Task<AccessToken> GetArmAccessTokenPublicAsync(CancellationToken cancellationToken) =>
            GetArmAccessTokenAsync(null, cancellationToken);

        public Task<AccessToken> GetArmAccessTokenPublicAsync(string? tenant, CancellationToken cancellationToken) =>
            GetArmAccessTokenAsync(tenant, cancellationToken);

        public static string EscapeKqlStringTest(string value) => EscapeKqlString(value);

        public string GetUserAgent() => UserAgent;

        public static T ConfigureRetryPolicyPublic<T>(T clientOptions, RetryPolicyOptions? retryPolicy) where T : ClientOptions =>
            AzureHelper.ConfigureRetryPolicy(clientOptions, retryPolicy);
    }
}
