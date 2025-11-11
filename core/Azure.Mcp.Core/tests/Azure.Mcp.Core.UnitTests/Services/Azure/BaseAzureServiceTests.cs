// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
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
        // Initialize the user agent policy before creating any service instances
        BaseAzureService.InitializeUserAgentPolicy("stdio");

        _azureService = new TestAzureService(_tenantService);
        _tenantService.GetTenantId(TenantName).Returns(TenantId);
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

        _tenantService.GetTenantId(tenantName2).Returns(tenantId2);

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
        string? actual = await _azureService.ResolveTenantId(null);
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
    public void InitializeUserAgentPolicy_UserAgentIsNotNull()
    {
        Assert.NotNull(_azureService.GetUserAgent());
    }

    [Fact]
    public void InitializeUserAgentPolicy_UserAgentContainsTransportType()
    {
        Assert.Contains("azmcp-stdio", _azureService.GetUserAgent());
    }

    private sealed class TestAzureService(ITenantService tenantService) : BaseAzureService(tenantService)
    {
        public Task<ArmClient> GetArmClientAsync(string? tenant = null, RetryPolicyOptions? retryPolicy = null) =>
            CreateArmClientAsync(tenant, retryPolicy);

        public Task<string?> ResolveTenantId(string? tenant) => ResolveTenantIdAsync(tenant);

        public string EscapeKqlStringTest(string value) => EscapeKqlString(value);

        public string GetUserAgent() => UserAgent;
    }
}
