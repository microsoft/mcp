// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure.Subscription;

public class SubscriptionServiceTests : IDisposable
{
    private const string TestSubscriptionId = "12345678-1234-1234-1234-123456789012";
    private const string TestSubscriptionName = "Test Subscription";
    private const string EnvVarSubscriptionId = "87654321-4321-4321-4321-210987654321";

    private readonly ICacheService _cacheService;
    private readonly ITenantService _tenantService;
    private readonly SubscriptionService _subscriptionService;
    private readonly string? _originalEnvValue;

    public SubscriptionServiceTests()
    {
        // Save original environment variable
        _originalEnvValue = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        _cacheService = Substitute.For<ICacheService>();
        _tenantService = Substitute.For<ITenantService>();
        _subscriptionService = new SubscriptionService(_cacheService, _tenantService);

        // Setup default cache behavior to return null (cache miss)
        _cacheService.GetAsync<object>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns((object?)null);
    }

    public void Dispose()
    {
        // Restore original environment variable
        if (_originalEnvValue != null)
        {
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", _originalEnvValue);
        }
        else
        {
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", null);
        }
    }

    [Fact]
    public void IsSubscriptionId_ValidGuid_ReturnsTrue()
    {
        // Act
        var result = _subscriptionService.IsSubscriptionId(TestSubscriptionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSubscriptionId_InvalidGuid_ReturnsFalse()
    {
        // Act
        var result = _subscriptionService.IsSubscriptionId("not-a-guid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSubscriptionId_SubscriptionName_ReturnsFalse()
    {
        // Act
        var result = _subscriptionService.IsSubscriptionId(TestSubscriptionName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnvironmentVariableFallback_IsAvailable()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", EnvVarSubscriptionId);

        // Act
        var envSubscription = EnvironmentHelpers.GetAzureSubscriptionId();

        // Assert
        Assert.Equal(EnvVarSubscriptionId, envSubscription);
    }

    [Fact]
    public void EnvironmentVariableFallback_WhenNotSet_ReturnsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", null);

        // Act
        var envSubscription = EnvironmentHelpers.GetAzureSubscriptionId();

        // Assert
        Assert.Null(envSubscription);
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("subscription", false)]
    [InlineData("default", false)]
    [InlineData("My Subscription Name", false)]
    [InlineData("test-subscription", false)]
    public void IsSubscriptionId_VariousInputs_ReturnsExpected(string input, bool expected)
    {
        // Act
        var result = _subscriptionService.IsSubscriptionId(input);

        // Assert
        Assert.Equal(expected, result);
    }
}

