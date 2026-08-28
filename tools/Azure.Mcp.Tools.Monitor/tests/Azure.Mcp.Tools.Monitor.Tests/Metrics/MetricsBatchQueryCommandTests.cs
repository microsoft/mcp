// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Metrics;

public class MetricsBatchQueryCommandTests : SubscriptionCommandUnitTestsBase<MetricsBatchQueryCommand, IMonitorMetricsService>
{
    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("batchquery", CommandDefinition.Name);
        Assert.Equal("Query Azure Monitor Metrics for Multiple Resources", Command.Title);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("batchquery", Command.Name);
    }

    #endregion

    #region Option Registration Tests

    [Fact]
    public void RegisterOptions_AddsAllExpectedOptions()
    {
        var options = CommandDefinition.Options.Select(o => o.Name).ToList();

        Assert.Contains("--resource-group", options);
        Assert.Contains("--resource-type", options);
        Assert.Contains("--resources", options);
        Assert.Contains("--metric-names", options);
        Assert.Contains("--metric-namespace", options);
        Assert.Contains("--start-time", options);
        Assert.Contains("--end-time", options);
        Assert.Contains("--interval", options);
        Assert.Contains("--aggregation", options);
        Assert.Contains("--filter", options);
        Assert.Contains("--order-by", options);
        Assert.Contains("--top", options);
        Assert.Contains("--max-buckets", options);

        var requiredOptions = CommandDefinition.Options.Where(o => o.Required).Select(o => o.Name).ToList();
        Assert.Contains("--resources", requiredOptions);
        Assert.Contains("--metric-names", requiredOptions);
        Assert.Contains("--metric-namespace", requiredOptions);
    }

    #endregion

    #region Option Binding Tests

    [Fact]
    public async Task ExecuteAsync_BindsAllOptionsCorrectly()
    {
        // Arrange
        Service.QueryMetricsBatchAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--resources", "sa1,sa2",
            "--metric-names", "CPU,Memory",
            "--metric-namespace", "Microsoft.Storage",
            "--start-time", "2023-01-01T00:00:00Z",
            "--end-time", "2023-01-02T00:00:00Z",
            "--interval", "PT1M",
            "--aggregation", "Average",
            "--filter", "dimension eq 'value'",
            "--order-by", "total asc",
            "--top", "5",
            "--max-buckets", "100");

        // Assert
        await Service.Received(1).QueryMetricsBatchAsync(
            "sub1",
            "rg1",
            "Microsoft.Storage/storageAccounts",
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "sa1", "sa2" })),
            "Microsoft.Storage",
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "CPU", "Memory" })),
            "2023-01-01T00:00:00Z",
            "2023-01-02T00:00:00Z",
            "PT1M",
            "Average",
            "dimension eq 'value'",
            "total asc",
            5,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesOptionalParameters()
    {
        // Arrange
        Service.QueryMetricsBatchAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", "sa1",
            "--metric-names", "CPU",
            "--metric-namespace", "microsoft.compute/virtualmachines");

        // Assert
        await Service.Received(1).QueryMetricsBatchAsync(
            Arg.Is<string>(t => t == "sub1"),
            Arg.Is<string?>(t => t == null),
            Arg.Is<string?>(t => t == null),
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "sa1" })),
            Arg.Is<string>(t => t == "microsoft.compute/virtualmachines"),
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "CPU" })),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string?>(t => t == null),
            Arg.Is<string?>(t => t == null),
            Arg.Is<string?>(t => t == null),
            Arg.Is<string?>(t => t == null),
            Arg.Is<int?>(t => t == null),
            Arg.Is<string?>(t => t == null),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("sa1", true)]
    [InlineData("sa1,sa2", true)]
    [InlineData("sa1, sa2, sa3", true)]
    [InlineData(",", false)]
    [InlineData("sa1,", false)]
    [InlineData(",sa1", false)]
    public async Task Validate_Resources_ValidatesCorrectly(string resources, bool shouldBeValid)
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", resources,
            "--metric-namespace", "microsoft.compute/virtualmachines",
            "--metric-names", "CPU");

        // Assert
        if (!shouldBeValid)
        {
            Assert.NotNull(result.Message);
            Assert.Contains("Invalid format for '--resources'", result.Message);
            Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        }
        else
        {
            Assert.Equal("Success", result.Message);
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
    }

    [Theory]
    [InlineData("CPU", true)]
    [InlineData("CPU,Memory", true)]
    [InlineData(",", false)]
    [InlineData("CPU,", false)]
    public async Task Validate_MetricNames_ValidatesCorrectly(string metricNames, bool shouldBeValid)
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", "sa1",
            "--metric-namespace", "microsoft.compute/virtualmachines",
            "--metric-names", metricNames);

        // Assert
        if (!shouldBeValid)
        {
            Assert.NotNull(result.Message);
            Assert.Contains("Invalid format for '--metric-names'", result.Message);
            Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        }
        else
        {
            Assert.Equal("Success", result.Message);
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
    }

    #endregion

    #region ExecuteAsync Tests - Success Scenarios

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var expectedResults = new List<ResourceMetricsResult>
        {
            new()
            {
                ResourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/sa1",
                Metrics =
                [
                    new()
                    {
                        Name = "CPU",
                        Unit = "Percent",
                        TimeSeries =
                        [
                            new()
                            {
                                Metadata = [],
                                Start = DateTime.UtcNow.AddHours(-1),
                                End = DateTime.UtcNow,
                                Interval = "PT1M",
                                AvgBuckets = [45.5, 50.2, 48.1]
                            }
                        ]
                    }
                ]
            }
        };

        Service.QueryMetricsBatchAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", "sa1",
            "--metric-names", "CPU",
            "--metric-namespace", "microsoft.compute/virtualmachines");

        // Assert
        var results = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.MetricsBatchQueryCommandResult);
        Assert.Single(results.Results);
        var resourceResult = results.Results[0];
        Assert.Equal(expectedResults[0].ResourceId, resourceResult.ResourceId);
        Assert.Single(resourceResult.Metrics);
        Assert.Equal("CPU", resourceResult.Metrics[0].Name);
        Assert.Equal([45.5, 50.2, 48.1], resourceResult.Metrics[0].TimeSeries[0].AvgBuckets!);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResults_ReturnsSuccessWithEmptyResults()
    {
        // Arrange
        Service.QueryMetricsBatchAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", "sa1",
            "--metric-names", "CPU",
            "--metric-namespace", "microsoft.compute/virtualmachines");

        // Assert
        var results = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.MetricsBatchQueryCommandResult);
        Assert.Empty(results.Results);
    }

    #endregion

    #region ExecuteAsync Tests - Validation Failures

    [Theory]
    [InlineData("--subscription sub1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines")] // Missing resources
    [InlineData("--subscription sub1 --resources sa1 --metric-namespace microsoft.compute/virtualmachines")] // Missing metric-names
    [InlineData("--subscription sub1 --resources sa1 --metric-names CPU")] // Missing metric-namespace
    public async Task ExecuteAsync_InvalidInput_ReturnsBadRequest(string args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.Results);
    }

    #endregion

    #region ExecuteAsync Tests - Bucket Limit Validation

    [Fact]
    public async Task ExecuteAsync_ExceedsBucketLimit_ReturnsBadRequest()
    {
        // Arrange
        var resultsWithTooManyBuckets = new List<ResourceMetricsResult>
        {
            new()
            {
                ResourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/sa1",
                Metrics =
                [
                    new()
                    {
                        Name = "CPU",
                        Unit = "Percent",
                        TimeSeries =
                        [
                            new()
                            {
                                Metadata = [],
                                Start = DateTime.UtcNow.AddHours(-1),
                                End = DateTime.UtcNow,
                                Interval = "PT1M",
                                AvgBuckets = new double[51] // Exceeds default limit of 50
                            }
                        ]
                    }
                ]
            }
        };

        Service.QueryMetricsBatchAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(resultsWithTooManyBuckets);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", "sa1",
            "--metric-names", "CPU",
            "--metric-namespace", "microsoft.compute/virtualmachines");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("exceeds the maximum allowed limit", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_HandlesError()
    {
        // Arrange
        Service.QueryMetricsBatchAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resources", "sa1",
            "--metric-names", "CPU",
            "--metric-namespace", "microsoft.compute/virtualmachines");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    #endregion
}
