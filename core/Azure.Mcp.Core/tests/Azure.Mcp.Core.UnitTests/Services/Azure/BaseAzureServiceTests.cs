// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.ResourceManager;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure;

public class BaseAzureServiceTests
{
    private const string TenantId = "test-tenant-id";
    private const string TenantName = "test-tenant-name";

    private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
    private readonly TestAzureService _azureService;

    public BaseAzureServiceTests()
    {
        _azureService = new TestAzureService(_tenantService);
        _tenantService.GetTenantId(TenantName, Arg.Any<CancellationToken>()).Returns(TenantId);
        _tenantService.GetTokenCredentialAsync(
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Substitute.For<TokenCredential>());
    }

    [Fact]
    public async Task CreateArmClientAsync_DoesNotReuseClient()
    {
        // Act
        var tenantName2 = "Other-Tenant-Name";
        var tenantId2 = "Other-Tenant-Id";

        _tenantService.GetTenantId(tenantName2, Arg.Any<CancellationToken>()).Returns(tenantId2);

        var retryPolicyArgs = new RetryPolicyOptions
        {
            DelaySeconds = 5,
            MaxDelaySeconds = 15,
            MaxRetries = 3
        };

        var client = await _azureService.GetArmClientAsync(TenantName, retryPolicyArgs);
        var client2 = await _azureService.GetArmClientAsync(TenantName, retryPolicyArgs);

        Assert.NotEqual(client, client2);

        var otherClient = await _azureService.GetArmClientAsync(tenantName2, retryPolicyArgs);

        Assert.NotEqual(client, otherClient);

        // Not tested: we'd like to, but can't, verify the TokenCredential is reused
        // between client and client2 but NOT with otherClient. ArmClient doesn't expose
        // the credential nor the HttpPipeline the credential is included within.
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ReturnsNullOnNull()
    {
        string? actual = await _azureService.ResolveTenantId(null, TestContext.Current.CancellationToken);
        Assert.Null(actual);
    }

    [Fact]
    public void EscapeKqlString_EscapesSingleQuotes()
    {
        // Arrange
        var input = "resource'with'quotes";
        var expected = "resource''with''quotes";

        // Act
        var result = _azureService.EscapeKqlStringTest(input);

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
        var result = _azureService.EscapeKqlStringTest(input);

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
        var result = _azureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_HandlesNullAndEmptyStrings()
    {
        // Act & Assert
        Assert.Equal(string.Empty, _azureService.EscapeKqlStringTest(null!));
        Assert.Equal(string.Empty, _azureService.EscapeKqlStringTest(string.Empty));
    }

    [Fact]
    public void EscapeKqlString_HandlesRegularStringsWithoutEscaping()
    {
        // Arrange
        var input = "regular-resource-name";

        // Act
        var result = _azureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(input, result);
    }

    [Fact]
    public void InitializeUserAgentPolicy_UserAgentContainsTransportType()
    {
        // Initialize the user agent policy before creating test service
        BaseAzureService.InitializeUserAgentPolicy(TransportTypes.StdIo);
        TestAzureService testAzureService = new TestAzureService(_tenantService);
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
    public void InitializeHttpClientTransport_ConfiguresSharedTransport()
    {
        var httpClientService = Substitute.For<IHttpClientService>();
        httpClientService.DefaultClient.Returns(new HttpClient(new HttpClientHandler()));

        BaseAzureService.InitializeHttpClientTransport(httpClientService);

        var clientOptions = new TestClientOptions();
        _azureService.ApplyDefaultPolicies(clientOptions);

        Assert.IsType<HttpClientTransport>(clientOptions.Transport);
    }

    [Fact]
    public void AddDefaultPolicies_DoesNotOverrideExistingTransport()
    {
        var clientOptions = new TestClientOptions();
        var customTransport = new NoopTransport();
        clientOptions.Transport = customTransport;

        _azureService.ApplyDefaultPolicies(clientOptions);

        Assert.Same(customTransport, clientOptions.Transport);
    }

    private sealed class TestAzureService(ITenantService tenantService) : BaseAzureService(tenantService)
    {
        public Task<ArmClient> GetArmClientAsync(string? tenant = null, RetryPolicyOptions? retryPolicy = null) =>
            CreateArmClientAsync(tenant, retryPolicy);

        // Expose the protected ResolveTenantIdAsync method for testing
        public Task<string?> ResolveTenantId(string? tenant, CancellationToken cancellationToken) => ResolveTenantIdAsync(tenant, cancellationToken);

        public string EscapeKqlStringTest(string value) => EscapeKqlString(value);

        public string GetUserAgent() => UserAgent;

        public ClientOptions ApplyDefaultPolicies(ClientOptions options) => AddDefaultPolicies(options);
    }

    private sealed class TestClientOptions : ClientOptions
    {
    }

    private sealed class NoopTransport : HttpPipelineTransport
    {
        private static readonly HttpClientTransport Inner = HttpClientTransport.Shared;

        public override Request CreateRequest() => Inner.CreateRequest();

        public override void Process(HttpMessage message) => throw new NotImplementedException();

        public override ValueTask ProcessAsync(HttpMessage message) => throw new NotImplementedException();
    }
}
