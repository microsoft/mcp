// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Metrics;

public class MetricsQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorMetricsService _service;
    private readonly ILogger<MetricsQueryCommand> _logger;
    private readonly MetricsQueryCommand _command;

    public MetricsQueryCommandTests()
    {
        _service = Substitute.For<IMonitorMetricsService>();
        _logger = Substitute.For<ILogger<MetricsQueryCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act
        var command = _command.GetCommand();

        // Assert
        Assert.Equal("query", command.Name);
        Assert.Equal("Query Azure Monitor Metrics", _command.Title);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Query Azure Monitor metrics for a resource", command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("query", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Query Azure Monitor Metrics", _command.Title);
    }
    #endregion

    #region Option Registration Tests

    [Fact]
    public void RegisterOptions_AddsAllExpectedOptions()
    {
        // Act
        var command = _command.GetCommand();

        // Assert - Check that all expected options are registered
        var options = command.Options.Select(o => o.Name).ToList();

        // Base options from BaseMetricsCommand
        Assert.Contains("--resource-group", options);
        Assert.Contains("--resource-type", options);
        Assert.Contains("--resource", options);

        // MetricsQueryCommand specific options
        Assert.Contains("--metric-names", options);
        Assert.Contains("--start-time", options);
        Assert.Contains("--end-time", options);
        Assert.Contains("--interval", options);
        Assert.Contains("--aggregation", options);
        Assert.Contains("--filter", options);
        Assert.Contains("--metric-namespace", options);
        Assert.Contains("--max-buckets", options);

        // Verify required options are marked as required
        var requiredOptions = command.Options.Where(o => o.Required).Select(o => o.Name).ToList();
        Assert.Contains("--resource", requiredOptions);
        Assert.Contains("--metric-names", requiredOptions);
    }

    #endregion

    #region Option Binding Tests

    [Fact]
    public async Task ExecuteAsync_BindsAllOptionsCorrectly()
    {
        // Arrange
        var args = "--subscription sub1 --resource-group rg1 --resource-type Microsoft.Storage/storageAccounts --resource sa1 " +
                   "--metric-names CPU,Memory --start-time 2023-01-01T00:00:00Z --end-time 2023-01-02T00:00:00Z " +
                   "--interval PT1M --aggregation Average --filter \"dimension eq 'value'\" --metric-namespace Microsoft.Storage " +
                   "--max-buckets 100";

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert - Verify all parameters were passed correctly to the service
        await _service.Received(1).QueryMetricsAsync(
            "sub1", // subscription
            "rg1", // resource group
            "Microsoft.Storage/storageAccounts", // resource type
            "sa1", // resource name
            "Microsoft.Storage", // metric namespace
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "CPU", "Memory" })), // metric names
            "2023-01-01T00:00:00Z", // start time
            "2023-01-02T00:00:00Z", // end time
            "PT1M", // interval
            "Average", // aggregation
            "dimension eq 'value'", // filter
            null, // tenant
            Arg.Any<RetryPolicyOptions?>()); // retry policy
    }

    [Fact]
    public async Task ExecuteAsync_HandlesOptionalParameters()
    {
        // Arrange
        var args = "--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines";

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert - Verify optional parameters are null when not provided
        await _service.Received(1).QueryMetricsAsync(
            Arg.Is<string>(t => t == "sub1"), // subscription
            Arg.Is<string?>(t => t == null), // resource group (not provided)
            Arg.Is<string?>(t => t == null), // resource type (not provided)
            Arg.Is<string>(t => t == "sa1"), // resource name
            Arg.Is<string>(t => t == "microsoft.compute/virtualmachines"), // metric namespace (not provided)
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "CPU" })), // metric names
            Arg.Any<string>(), // start time (default)
            Arg.Any<string>(), // end time (default)
            Arg.Is<string?>(t => t == null), // interval (not provided)
            Arg.Is<string?>(t => t == null), // aggregation (not provided)
            Arg.Is<string?>(t => t == null), // filter (not provided)
            Arg.Is<string?>(t => t == null), // tenant
            Arg.Any<RetryPolicyOptions?>()); // retry policy
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("CPU", true)]
    [InlineData("CPU,Memory", true)]
    [InlineData("CPU, Memory, Disk", true)]
    [InlineData(",", false)]
    [InlineData("CPU,", false)]
    [InlineData(",CPU", false)]
    public async Task Validate_MetricNames_ValidatesCorrectly(string metricNames, bool shouldBeValid)
    {
        // Arrange
        var args = $"--subscription sub1 --resource sa1 --metric-namespace microsoft.compute/virtualmachines --metric-names \"{metricNames}\"";
        var parseResult = _command.GetCommand().Parse(args);
        var commandResult = parseResult.CommandResult;

        var context = new CommandContext(_serviceProvider);
        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        if (!shouldBeValid)
        {
            Assert.NotNull(result.Message);
            Assert.Contains("Invalid format for --metric-names", result.Message);
            Assert.Equal(400, result.Status);
        }
        else
        {
            Assert.Equal("Success", result.Message);
            Assert.Equal(200, result.Status); // Default status should remain unchanged for valid cases
        }
    }

    #endregion

    #region ExecuteAsync Tests - Success Scenarios

    [Theory]
    [InlineData("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines")]
    [InlineData("--subscription sub1 --resource-group rg1 --resource-type Microsoft.Storage/storageAccounts --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines")]
    [InlineData("--subscription sub1 --resource sa1 --metric-names CPU,Memory --metric-namespace microsoft.compute/virtualmachines")]
    [InlineData("--subscription sub1 --resource sa1 --metric-namespace microsoft.compute/virtualmachines --metric-names CPU --start-time 2023-01-01T00:00:00Z --end-time 2023-01-02T00:00:00Z")]
    public async Task ExecuteAsync_ValidInput_ReturnsSuccess(string args)
    {
        // Arrange
        var expectedResults = new List<MetricResult>
        {
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
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResults);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the actual content of the results
        var results = GetResult(response.Results);
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("CPU", results[0].Name);
        Assert.Equal("Percent", results[0].Unit);
        Assert.Single(results[0].TimeSeries);
        Assert.Equal([45.5, 50.2, 48.1], results[0].TimeSeries[0].AvgBuckets!);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResults_ReturnsSuccessWithEmptyResults()
    {
        // Arrange
        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var results = GetResult(response.Results);
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new List<MetricResult>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(
            "--subscription sub1 --resource-group rg1 --resource-type Microsoft.Storage/storageAccounts --metric-namespace microsoft.compute/virtualmachines " +
            "--resource sa1 --metric-names CPU,Memory --start-time 2023-01-01T00:00:00Z " +
            "--end-time 2023-01-02T00:00:00Z --interval PT1M --aggregation Average");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).QueryMetricsAsync(
            "sub1",
            "rg1",
            "Microsoft.Storage/storageAccounts",
            "sa1",
            "microsoft.compute/virtualmachines",
            Arg.Is<IEnumerable<string>>(m => m.SequenceEqual(new[] { "CPU", "Memory" })),
            "2023-01-01T00:00:00Z",
            "2023-01-02T00:00:00Z",
            "PT1M",
            "Average",
            null,
            null,
            Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region ExecuteAsync Tests - Validation Failures

    [Theory]
    [InlineData("--subscription sub1 --metric-names CPU")] // Missing resource
    [InlineData("--subscription sub1 --resource sa1")] // Missing metric-names
    public async Task ExecuteAsync_InvalidInput_ReturnsBadRequest(string args)
    {
        // Arrange
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.Results);
    }

    #endregion

    #region ExecuteAsync Tests - Bucket Limit Validation

    [Fact]
    public async Task ExecuteAsync_ExceedsBucketLimit_ReturnsBadRequest()
    {
        // Arrange
        var resultsWithTooManyBuckets = new List<MetricResult>
        {
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
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(resultsWithTooManyBuckets);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("exceeds the maximum allowed limit of 50", response.Message);
        Assert.Contains("CPU", response.Message);
        Assert.Contains("51 time buckets", response.Message);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsCustomBucketLimit_ReturnsBadRequest()
    {
        // Arrange
        var resultsWithTooManyBuckets = new List<MetricResult>
        {
            new()
            {
                Name = "Memory",
                Unit = "Bytes",
                TimeSeries =
                [
                    new()
                    {
                        Metadata = [],
                        Start = DateTime.UtcNow.AddHours(-1),
                        End = DateTime.UtcNow,
                        Interval = "PT1M",
                        MaxBuckets = new double[26] // Exceeds custom limit of 25
                    }
                ]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(resultsWithTooManyBuckets);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names Memory --max-buckets 25 --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("exceeds the maximum allowed limit of 25", response.Message);
        Assert.Contains("Memory", response.Message);
        Assert.Contains("26 time buckets", response.Message);
    }

    [Theory]
    [InlineData("AvgBuckets")]
    [InlineData("MinBuckets")]
    [InlineData("MaxBuckets")]
    [InlineData("TotalBuckets")]
    [InlineData("CountBuckets")]
    public async Task ExecuteAsync_ChecksAllBucketTypes_ForLimitExceeded(string bucketType)
    {
        // Arrange
        var timeSeries = new MetricTimeSeries
        {
            Metadata = [],
            Start = DateTime.UtcNow.AddHours(-1),
            End = DateTime.UtcNow,
            Interval = "PT1M"
        };

        // Set the specific bucket type to exceed limit
        var largeBucketArray = new double[51];
        switch (bucketType)
        {
            case "AvgBuckets":
                timeSeries.AvgBuckets = largeBucketArray;
                break;
            case "MinBuckets":
                timeSeries.MinBuckets = largeBucketArray;
                break;
            case "MaxBuckets":
                timeSeries.MaxBuckets = largeBucketArray;
                break;
            case "TotalBuckets":
                timeSeries.TotalBuckets = largeBucketArray;
                break;
            case "CountBuckets":
                timeSeries.CountBuckets = largeBucketArray;
                break;
        }

        var results = new List<MetricResult>
        {
            new()
            {
                Name = "TestMetric",
                Unit = "Count",
                TimeSeries = [timeSeries]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(results);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names TestMetric --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("exceeds the maximum allowed limit", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithinBucketLimit_ReturnsSuccess()
    {
        // Arrange
        var resultsWithinLimit = new List<MetricResult>
        {
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
                        AvgBuckets = [50] // Exactly at the limit
                    }
                ]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(resultsWithinLimit);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_BucketLimitExceeded_LogsWarning()
    {
        // Arrange
        var resultsWithTooManyBuckets = new List<MetricResult>
        {
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
                        AvgBuckets = new double[51]
                    }
                ]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(resultsWithTooManyBuckets);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Bucket limit exceeded")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region ExecuteAsync Tests - Error Handling

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedException = new Exception("Service unavailable");
        _service.When(x => x.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()))
            .Do(x => throw expectedException);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service unavailable", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_LogsError()
    {
        // Arrange
        var expectedException = new Exception("Service error");
        _service.When(x => x.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()))
            .Do(x => throw expectedException);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error querying metrics")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task ExecuteAsync_MultipleMetricsWithMixedBucketCounts_ValidatesEach()
    {
        // Arrange
        var results = new List<MetricResult>
        {
            new()
            {
                Name = "CPU",
                Unit = "Percent",
                TimeSeries =
                [
                    new()
                    {
                        AvgBuckets = new double[30] // Within limit
                    }
                ]
            },
            new()
            {
                Name = "Memory",
                Unit = "Bytes",
                TimeSeries =
                [
                    new()
                    {
                        AvgBuckets = new double[51] // Exceeds limit
                    }
                ]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(results);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU,Memory --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Memory", response.Message);
        Assert.Contains("51 time buckets", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleTimeSeriesPerMetric_ValidatesAll()
    {
        // Arrange
        var results = new List<MetricResult>
        {
            new()
            {
                Name = "CPU",
                Unit = "Percent",
                TimeSeries =
                [
                    new()
                    {
                        AvgBuckets = new double[30] // Within limit
                    },
                    new()
                    {
                        AvgBuckets = new double[51] // Exceeds limit
                    }
                ]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(results);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("CPU", response.Message);
        Assert.Contains("51 time buckets", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_NullBuckets_DoesNotCountTowardsLimit()
    {
        // Arrange
        var results = new List<MetricResult>
        {
            new()
            {
                Name = "CPU",
                Unit = "Percent",
                TimeSeries =
                [
                    new()
                    {
                        AvgBuckets = null,
                        MinBuckets = null,
                        MaxBuckets = null,
                        TotalBuckets = null,
                        CountBuckets = null
                    }
                ]
            }
        };

        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(results);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_NullResults_ReturnsSuccessWithEmptyResults()
    {
        // Arrange
        _service.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult((List<MetricResult>)null!));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource sa1 --metric-names CPU --metric-namespace microsoft.compute/virtualmachines");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    #endregion

    private List<MetricResult>? GetResult(ResponseResult? result)
    {
        if (result == null)
        {
            return null;
        }
        var json = JsonSerializer.Serialize(result);
        return JsonSerializer.Deserialize(json, MonitorJsonContext.Default.MetricsQueryCommandResult)?.Results;
    }
}
