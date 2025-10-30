// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        _azureService = new TestAzureService(_tenantService);
        _tenantService.GetTenantId(TenantName).Returns(TenantId);
    }

    [Fact]
    public async Task CreateArmClientAsync_CreatesAndUsesCachedClient()
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

        Assert.Equal(client, client2);

        var otherClient = await _azureService.GetArmClientAsync(tenantName2, retryPolicyArgs);

        Assert.NotEqual(client, otherClient);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ReturnsValueNoService()
    {
        var testAzureService = new TestAzureService(null);

        string? actual = await testAzureService.ResolveTenantId(TenantName);
        Assert.Equal(TenantName, actual);

        string? actual2 = await testAzureService.ResolveTenantId(null);
        Assert.Null(actual2);
    }

    [Fact]
    public void EscapeKqlString_EscapesSingleQuotes()
    {
        // Arrange
        var testAzureService = new TestAzureService();
        var input = "resource'with'quotes";
        var expected = "resource''with''quotes";

        // Act
        var result = testAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_EscapesBackslashes()
    {
        // Arrange
        var testAzureService = new TestAzureService();
        var input = @"resource\with\backslashes";
        var expected = @"resource\\with\\backslashes";

        // Act
        var result = testAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_EscapesBothQuotesAndBackslashes()
    {
        // Arrange
        var testAzureService = new TestAzureService();
        var input = @"resource\'with\'mixed";
        var expected = @"resource\\''with\\''mixed";

        // Act
        var result = testAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeKqlString_HandlesNullAndEmptyStrings()
    {
        // Arrange
        var testAzureService = new TestAzureService();

        // Act & Assert
        Assert.Equal(string.Empty, testAzureService.EscapeKqlStringTest(null!));
        Assert.Equal(string.Empty, testAzureService.EscapeKqlStringTest(string.Empty));
    }

    [Fact]
    public void EscapeKqlString_HandlesRegularStringsWithoutEscaping()
    {
        // Arrange
        var testAzureService = new TestAzureService();
        var input = "regular-resource-name";

        // Act
        var result = testAzureService.EscapeKqlStringTest(input);

        // Assert
        Assert.Equal(input, result);
    }

    private sealed class TestAzureService(ITenantService? tenantService = null) : BaseAzureService(tenantService)
    {
        public Task<ArmClient> GetArmClientAsync(string? tenant = null, RetryPolicyOptions? retryPolicy = null) =>
            CreateArmClientAsync(tenant, retryPolicy);

        public Task<string?> ResolveTenantId(string? tenant) => ResolveTenantIdAsync(tenant);

        public string EscapeKqlStringTest(string value) => EscapeKqlString(value);
    }
}
