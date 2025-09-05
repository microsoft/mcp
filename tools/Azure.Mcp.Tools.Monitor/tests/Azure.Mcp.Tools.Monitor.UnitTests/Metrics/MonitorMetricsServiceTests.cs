// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Metrics;

public class MonitorMetricsServiceTests
{
    private readonly IResourceResolverService _resourceResolverService;
    private readonly IMetricsQueryClientService _metricsQueryClientService;
    private readonly MetricsQueryClient _metricsQueryClient;
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
        _metricsQueryClientService = Substitute.For<IMetricsQueryClientService>();
        _metricsQueryClient = Substitute.For<MetricsQueryClient>();
        _service = new MonitorMetricsService(_resourceResolverService, _metricsQueryClientService);

        // Setup default behaviors
        _resourceResolverService.ResolveResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(new ResourceIdentifier(TestResourceId));

        _metricsQueryClientService.CreateClientAsync(
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(_metricsQueryClient);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_Succeeds()
    {
        // Act & Assert - Constructor should not throw
        var service = new MonitorMetricsService(_resourceResolverService, _metricsQueryClientService);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullResourceResolverService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MonitorMetricsService(null!, _metricsQueryClientService));
    }

    [Fact]
    public void Constructor_WithNullMetricsQueryClientService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MonitorMetricsService(_resourceResolverService, null!));
    }

    #endregion

    #region QueryMetricsAsync Tests

    [Theory]
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
                startTime: invalidStartTime));

        Assert.Contains("Invalid start time format", exception.Message);
    }

    [Theory]
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
                endTime: invalidEndTime));

        Assert.Contains("Invalid end time format", exception.Message);
    }

    [Theory]
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
                interval: invalidInterval));

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
        if (subscription is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.QueryMetricsAsync(
                    subscription!,
                    TestResourceGroup,
                    TestResourceType,
                    TestResourceName,
                    metricNamespace,
                    metricNames));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.QueryMetricsAsync(
                    subscription,
                    TestResourceGroup,
                    TestResourceType,
                    TestResourceName,
                    metricNamespace,
                    metricNames));
        }
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
        if (resourceName is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.QueryMetricsAsync(
                    TestSubscription,
                    TestResourceGroup,
                    TestResourceType,
                    resourceName!,
                    metricNamespace,
                    metricNames));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.QueryMetricsAsync(
                    TestSubscription,
                    TestResourceGroup,
                    TestResourceType,
                    resourceName,
                    metricNamespace,
                    metricNames));
        }
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
                null!));
    }

    [Fact]
    public async Task QueryMetricsAsync_WithNullMetricNamespace_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                null!,
                new[] { "Transactions" }));
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
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("Resource not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                metricNamespace,
                metricNames));

        Assert.Contains("Resource not found", exception.Message);
    }

    [Fact]
    public async Task QueryMetricsAsync_WithClientCreationFailure_ThrowsException()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        _metricsQueryClientService.CreateClientAsync(
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("Authentication failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.QueryMetricsAsync(
                TestSubscription,
                TestResourceGroup,
                TestResourceType,
                TestResourceName,
                metricNamespace,
                metricNames));

        Assert.Contains("Authentication failed", exception.Message);
    }

    #endregion

    #region ListMetricDefinitionsAsync Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricDefinitionsAsync_WithNullOrEmptySubscription_ThrowsArgumentException(string? subscription)
    {
        // Act & Assert
        if (subscription is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ListMetricDefinitionsAsync(
                    subscription!,
                    TestResourceGroup,
                    TestResourceType,
                    TestResourceName));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ListMetricDefinitionsAsync(
                    subscription,
                    TestResourceGroup,
                    TestResourceType,
                    TestResourceName));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricDefinitionsAsync_WithNullOrEmptyResourceName_ThrowsArgumentException(string? resourceName)
    {
        // Act & Assert
        if (resourceName is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ListMetricDefinitionsAsync(
                    TestSubscription,
                    TestResourceGroup,
                    TestResourceType,
                    resourceName!));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ListMetricDefinitionsAsync(
                    TestSubscription,
                    TestResourceGroup,
                    TestResourceType,
                    resourceName));
        }
    }

    #endregion

    #region ListMetricNamespacesAsync Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricNamespacesAsync_WithNullOrEmptySubscription_ThrowsArgumentException(string? subscription)
    {
        // Act & Assert
        if (subscription is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ListMetricNamespacesAsync(
                    subscription!,
                    TestResourceGroup,
                    TestResourceType,
                    TestResourceName));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ListMetricNamespacesAsync(
                    subscription,
                    TestResourceGroup,
                    TestResourceType,
                    TestResourceName));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ListMetricNamespacesAsync_WithNullOrEmptyResourceName_ThrowsArgumentException(string? resourceName)
    {
        // Act & Assert
        if (resourceName is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ListMetricNamespacesAsync(
                    TestSubscription,
                    TestResourceGroup,
                    TestResourceType,
                    resourceName!));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ListMetricNamespacesAsync(
                    TestSubscription,
                    TestResourceGroup,
                    TestResourceType,
                    resourceName));
        }
    }

    #endregion

    [Fact]
    public async Task QueryMetricsAsync_WithValidResponse_ReturnsTransformedResults()
    {
        // Arrange
        var metricNames = new[] { "Transactions", "Availability" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        var mockResponse = CreateMockMetricsQueryResponse();

        _metricsQueryClient.QueryResourceAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<MetricsQueryOptions>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await _service.QueryMetricsAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Transactions", result[0].Name);
        Assert.Equal("Count", result[0].Unit);
        Assert.Equal("Availability", result[1].Name);
        Assert.Equal("Percent", result[1].Unit);
    }

    [Fact]
    public async Task QueryMetricsAsync_ConfiguresQueryOptions_WithTimeRange()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var startTime = "2023-01-01T00:00:00Z";
        var endTime = "2023-01-02T00:00:00Z";
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        var mockResponse = CreateMockMetricsQueryResponse();

        MetricsQueryOptions? capturedOptions = null;
        _metricsQueryClient.QueryResourceAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Do<MetricsQueryOptions>(opts => capturedOptions = opts),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _service.QueryMetricsAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames,
            startTime: startTime,
            endTime: endTime);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.NotNull(capturedOptions.TimeRange);
        Assert.Equal(DateTimeOffset.Parse(startTime), capturedOptions.TimeRange.Value.Start);
        Assert.Equal(DateTimeOffset.Parse(endTime), capturedOptions.TimeRange.Value.End);
    }

    [Fact]
    public async Task QueryMetricsAsync_ConfiguresQueryOptions_WithInterval()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        var interval = "PT5M";
        var mockResponse = CreateMockMetricsQueryResponse();

        MetricsQueryOptions? capturedOptions = null;
        _metricsQueryClient.QueryResourceAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Do<MetricsQueryOptions>(opts => capturedOptions = opts),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _service.QueryMetricsAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames,
            interval: interval);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(TimeSpan.FromMinutes(5), capturedOptions.Granularity);
    }

    [Fact]
    public async Task QueryMetricsAsync_ConfiguresQueryOptions_WithAggregation()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        var aggregation = "Average,Maximum,Count";
        var mockResponse = CreateMockMetricsQueryResponse();

        MetricsQueryOptions? capturedOptions = null;
        _metricsQueryClient.QueryResourceAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Do<MetricsQueryOptions>(opts => capturedOptions = opts),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _service.QueryMetricsAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames,
            aggregation: aggregation);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Contains(MetricAggregationType.Average, capturedOptions.Aggregations);
        Assert.Contains(MetricAggregationType.Maximum, capturedOptions.Aggregations);
        Assert.Contains(MetricAggregationType.Count, capturedOptions.Aggregations);
    }

    #region Service Dependency Tests

    [Fact]
    public async Task QueryMetricsAsync_CallsResourceResolverService_WithCorrectParameters()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var metricNamespace = "Microsoft.Storage/storageAccounts";
        var mockResponse = CreateMockMetricsQueryResponse();

        _metricsQueryClient.QueryResourceAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<MetricsQueryOptions>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _service.QueryMetricsAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames,
            tenant: TestTenant);

        // Assert
        await _resourceResolverService.Received(1).ResolveResourceIdAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            TestTenant,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task QueryMetricsAsync_CallsMetricsQueryClientService_WithCorrectParameters()
    {
        // Arrange
        var metricNames = new[] { "Transactions" };
        var mockResponse = CreateMockMetricsQueryResponse();
        var metricNamespace = "Microsoft.Storage/storageAccounts";

        _metricsQueryClient.QueryResourceAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<MetricsQueryOptions>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _service.QueryMetricsAsync(
            TestSubscription,
            TestResourceGroup,
            TestResourceType,
            TestResourceName,
            metricNamespace,
            metricNames,
            tenant: TestTenant);

        // Assert
        await _metricsQueryClientService.Received(1).CreateClientAsync(
            TestTenant,
            Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region Helper Methods

    private static Response<MetricsQueryResult> CreateMockMetricsQueryResponse()
    {
        // Create metrics using MonitorQueryModelFactory
        var metrics = new List<MetricResult>
        {
            MonitorQueryModelFactory.MetricResult(
                id: "availability-metric",
                resourceType: "Microsoft.Storage/storageAccounts",
                name: "Transactions",
                unit: MetricUnit.Count,
                timeSeries: new List<MetricTimeSeriesElement>()),
            MonitorQueryModelFactory.MetricResult(
                id: "availability-metric",
                resourceType: "Microsoft.Storage/storageAccounts",
                name: "Availability",
                unit: MetricUnit.Percent,
                timeSeries: new List<MetricTimeSeriesElement>())
        };

        var result = MonitorQueryModelFactory.MetricsQueryResult(
            cost: 0,
            @namespace: "Microsoft.Storage/storageAccounts",
            metrics: metrics,
            granularity: TimeSpan.FromMinutes(1),
            timespan: "PT1H",
            resourceRegion: "East US");

        var response = Substitute.For<Response<MetricsQueryResult>>();
        response.Value.Returns(result);
        return response;
    }
    #endregion
}
