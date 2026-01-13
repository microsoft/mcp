// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Metrics;

public class MonitorMetricsServiceTests
{
    private readonly IResourceResolverService _resourceResolverService;
    private readonly ITenantService _tenantService;
    private readonly MonitorMetricsService _service;

    private const string TestSubscription = "12345678-1234-1234-1234-123456789012";
    private const string TestResourceGroup = "test-rg";
    private const string TestResourceType = "Microsoft.Storage/storageAccounts";
    private const string TestResourceName = "test";
    private const string TestResourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/test";
    private const string TestTenant = "tenant-123";

    public MonitorMetricsServiceTests()
    {
        _resourceResolverService = Substitute.For<IResourceResolverService>();
        _tenantService = Substitute.For<ITenantService>();
        _service = new MonitorMetricsService(_resourceResolverService, _tenantService);

        // Setup default behaviors
        _resourceResolverService.ResolveResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResourceIdentifier(TestResourceId)));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_Succeeds()
    {
        // Act & Assert - Constructor should not throw
        var service = new MonitorMetricsService(_resourceResolverService, _tenantService);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullResourceResolverService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MonitorMetricsService(null!, _tenantService));
    }
    #endregion

    #region QueryMetricsAsync Tests

    [Theory(Skip = "Requires ARM client - move to live tests")]
    [InlineData("invalid-date")]
    [InlineData("2023-13-01T00:00:00Z")]
    [InlineData("not-a-date")]
    public async Task QueryMetricsAsync_WithInvalidStartTime_ThrowsException(string invalidStartTime)
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                metricNamespace,
                metricNames,
                startTime: invalidStartTime,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid start time format", exception.Message);
    }

    [Theory(Skip = "Requires ARM client - move to live tests")]
    [InlineData("invalid-date")]
    [InlineData("2023-13-01T00:00:00Z")]
    [InlineData("not-a-date")]
    public async Task QueryMetricsAsync_WithInvalidEndTime_ThrowsArgumentException(string invalidEndTime)
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                metricNamespace,
                metricNames,
                endTime: invalidEndTime,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid end time format", exception.Message);
    }

    [Theory(Skip = "Requires ARM client - move to live tests")]
    [InlineData("invalid-interval")]
    [InlineData("5M")]
    [InlineData("1 hour")]
    public async Task QueryMetricsAsync_WithInvalidInterval_ThrowsException(string invalidInterval)
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                metricNamespace,
                metricNames,
                interval: invalidInterval,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid interval format", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task QueryMetricsAsync_WithNullOrEmptySubscription_ThrowsException(string? subscription)
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
        _service.QueryMetricsAsync(
            subscription!,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames,
            cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task QueryMetricsAsync_WithNullOrEmptyResourceName_ThrowsArgumentException(string? resourceName)
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                resourceName!,
                metricNamespace,
                metricNames,
                cancellationToken: TestContext.Current.CancellationToken)
            );
    }

    [Fact]
    public async Task QueryMetricsAsync_WithNullMetricNames_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                "test-namespace",
                null!,
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryMetricsAsync_WithNullMetricNamespace_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                null!,
                ["Transactions"],
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryMetricsAsync_WithResourceResolutionFailure_ThrowsException()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        _resourceResolverService.ResolveResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Resource not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                metricNamespace,
                metricNames,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Resource not found", exception.Message);
    }

    [Fact(Skip = "No longer uses IMetricsQueryClientService - ARM client creation can't be mocked in unit tests")]
    public async Task QueryMetricsAsync_WithClientCreationFailure_ThrowsException()
    {
        // This test is no longer applicable as the service uses CreateArmClientAsync from base class
        // which requires actual infrastructure and can't be mocked in unit tests
        await Task.CompletedTask;
    }

    #endregion

    #region ListMetricDefinitionsAsync Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricDefinitionsAsync_WithNullOrEmptySubscription_ThrowsArgumentException(string? subscription)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListMetricDefinitionsAsync(
                subscription!,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                cancellationToken: TestContext.Current.CancellationToken));

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricDefinitionsAsync_WithNullOrEmptyResourceName_ThrowsArgumentException(string? resourceName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListMetricDefinitionsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                resourceName!,
                cancellationToken: TestContext.Current.CancellationToken));

    }

    #endregion

    #region ListMetricNamespacesAsync Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricNamespacesAsync_WithNullOrEmptySubscription_ThrowsArgumentException(string? subscription)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListMetricNamespacesAsync(
                subscription!,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricNamespacesAsync_WithNullOrEmptyResourceName_ThrowsArgumentException(string? resourceName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListMetricNamespacesAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                resourceName!,
                cancellationToken: TestContext.Current.CancellationToken));
    }

    #endregion

    [Fact(Skip = "Requires ARM client - move to live tests")]
    public async Task QueryMetricsAsync_WithValidResponse_ReturnsTransformedResults()
    {
        // This test requires actual ARM client which can't be mocked in unit tests
        // Move this to live tests for integration testing
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires ARM client - move to live tests")]
    public async Task QueryMetricsAsync_ConfiguresQueryOptions_WithTimeRange()
    {
        // This test requires actual ARM client which can't be mocked in unit tests
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires ARM client - move to live tests")]
    public async Task QueryMetricsAsync_ConfiguresQueryOptions_WithInterval()
    {
        // This test requires actual ARM client which can't be mocked in unit tests
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires ARM client - move to live tests")]
    public async Task QueryMetricsAsync_ConfiguresQueryOptions_WithAggregation()
    {
        // This test requires actual ARM client which can't be mocked in unit tests
        await Task.CompletedTask;
    }

    #region Service Dependency Tests

    [Fact(Skip = "Requires ARM client - move to live tests")]
    public async Task QueryMetricsAsync_CallsResourceResolverService_WithCorrectParameters()
    {
        // This test requires actual ARM client which can't be mocked in unit tests
        await Task.CompletedTask;
    }

    [Fact(Skip = "No longer uses IMetricsQueryClientService - ARM client creation can't be mocked in unit tests")]
    public async Task QueryMetricsAsync_CallsMetricsQueryClientService_WithCorrectParameters()
    {
        // This test is no longer applicable as the service uses CreateArmClientAsync from base class
        await Task.CompletedTask;
    }

    #endregion


}
