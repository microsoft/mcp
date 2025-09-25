// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Metrics;

public class MetricsDefinitionsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorMetricsService _service;
    private readonly ILogger<MetricsDefinitionsCommand> _logger;
    private readonly MetricsDefinitionsCommand _command;

    public MetricsDefinitionsCommandTests()
    {
        _service = Substitute.For<IMonitorMetricsService>();
        _logger = Substitute.For<ILogger<MetricsDefinitionsCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    #region Constructor and Command Setup Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("definitions", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("List available metric definitions", command.Description);
    }

    [Fact]
    public void Name_ReturnsDefinitions()
    {
        Assert.Equal("definitions", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectTitle()
    {
        Assert.Equal("List Azure Monitor Metric Definitions", _command.Title);
    }

    [Fact]
    public void GetCommand_RegistersAllRequiredOptions()
    {
        var command = _command.GetCommand();

        // Check that all required options are present
        var optionNames = command.Options.Select(o => o.Name).ToList();

        Assert.Contains("--subscription", optionNames);
        Assert.Contains("--resource-type", optionNames);
        Assert.Contains("--resource", optionNames);
        Assert.Contains("--metric-namespace", optionNames);
        Assert.Contains("--search-string", optionNames);
        Assert.Contains("--limit", optionNames);
        Assert.Contains("--tenant", optionNames);
        // Note: resource-group may not be registered as a separate option if resource-id parsing is used
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("--resource test --subscription sub1", true)]
    [InlineData("--subscription sub1", false)]
    [InlineData("--resource test", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _service.ListMetricDefinitionsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(
                [
                    new()
                    {
                        Name = "CPU",
                        Description = "CPU Percentage",
                        Category = "Performance",
                        Unit = "Percent",
                        SupportedAggregationTypes = ["Average", "Maximum", "Minimum"],
                        IsDimensionRequired = true,
                        Dimensions = ["Instance"]
                    }
                ]);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    #endregion

    #region Service Interaction Tests

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedResults = new List<MetricDefinition>
        {
            new()
            {
                Name = "CPU Percentage",
                Description = "Average CPU usage",
                Category = "Performance",
                Unit = "Percent",
                SupportedAggregationTypes = ["Average"],
                IsDimensionRequired = false,
                Dimensions = []
            }
        };

        _service.ListMetricDefinitionsAsync(
            "sub1",
            null, // resource-group may be null if not provided or not parsed from resource-id
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            null,
            "tenant1",
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResults);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(
            "--resource test --subscription sub1 --resource-type Microsoft.Storage/storageAccounts --metric-namespace Microsoft.Storage/storageAccounts --tenant tenant1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 1 metric definitions returned.", response.Message);
        await _service.Received(1).ListMetricDefinitionsAsync(
            "sub1",
            null,
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            null,
            "tenant1",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithSearchString_CallsServiceWithSearchParameter()
    {
        // Arrange
        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            "cpu",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(
            [
                new()
                {
                    Name = "CPU Percentage",
                    Description = "Average CPU usage",
                    Category = "Performance",
                    Unit = "Percent"
                }
            ]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1 --search-string cpu");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with the search string
        await _service.Received(1).ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            "cpu",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAllOptionalParameters_CallsServiceCorrectly()
    {
        // Arrange
        var expectedResults = new List<MetricDefinition>
        {
            new()
            {
                Name = "Memory Usage",
                Description = "Memory usage metrics",
                Category = "Memory",
                Unit = "Bytes"
            }
        };

        _service.ListMetricDefinitionsAsync(
            "sub1",
            null, // resource-group may be null if not provided or not parsed from resource-id
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            "memory",
            "tenant1",
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResults);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(
            "--resource test --subscription sub1 --resource-type Microsoft.Storage/storageAccounts --metric-namespace Microsoft.Storage/storageAccounts --search-string memory --tenant tenant1 --limit 20");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 1 metric definitions returned.", response.Message);
        await _service.Received(1).ListMetricDefinitionsAsync(
            "sub1",
            null,
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            "memory",
            "tenant1",
            Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<List<MetricDefinition>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException_LogsError()
    {
        // Arrange
        var exception = new Exception("Service unavailable");
        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<List<MetricDefinition>>(exception));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1 --resource-group rg1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error listing metric definitions")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region Result Processing Tests

    [Fact]
    public async Task ExecuteAsync_WithResults_ReturnsCorrectStructure()
    {
        // Arrange
        var metricDefinitions = new List<MetricDefinition>
        {
            new()
            {
                Name = "CPU Percentage",
                Description = "Average CPU usage",
                Category = "Performance",
                Unit = "Percent",
                SupportedAggregationTypes = ["Average", "Maximum"],
                IsDimensionRequired = false,
                Dimensions = []
            },
            new()
            {
                Name = "Memory Usage",
                Description = "Memory usage in bytes",
                Category = "Memory",
                Unit = "Bytes",
                SupportedAggregationTypes = ["Average", "Maximum", "Total"],
                IsDimensionRequired = true,
                Dimensions = ["Instance", "Role"]
            }
        };

        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(metricDefinitions);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 2 metric definitions returned.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoResults_ReturnsNullResults()
    {
        // Arrange
        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullResults_ReturnsNullResults()
    {
        // Arrange
        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult<List<MetricDefinition>>(null!));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Null(response.Results);
    }

    #endregion

    #region Limit Processing Tests

    [Fact]
    public async Task ExecuteAsync_WithDefaultLimit_TruncatesResultsTo10()
    {
        // Arrange
        var metricDefinitions = GenerateMetricDefinitions(15); // More than default limit of 10

        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(metricDefinitions);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that results were truncated - message should indicate truncation
        Assert.Contains("Results truncated to 10 of 15", response.Message);
        Assert.Contains("metric definitions", response.Message);
        // Verify service receives all data but command applies limit internally
        await _service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomLimit_TruncatesResultsCorrectly()
    {
        // Arrange
        var metricDefinitions = GenerateMetricDefinitions(20);

        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(metricDefinitions);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1 --limit 5");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that results were truncated to the custom limit
        Assert.Contains("Results truncated to 5 of 20", response.Message);
        Assert.Contains("metric definitions", response.Message);
        // Verify service is called correctly
        await _service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResultsExceedingLimit_ShowsTruncationMessage()
    {
        // Arrange - Create more results than the limit
        var metricDefinitions = GenerateMetricDefinitions(25);

        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(metricDefinitions);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1 --limit 8");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that the message indicates truncation with correct counts
        Assert.Contains("Results truncated to 8 of 25", response.Message);
        Assert.Contains("Use --search-string to filter results", response.Message);
        // Verify the service was called
        await _service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResultsUnderLimit_ShowsAllResultsMessage()
    {
        // Arrange - Create fewer results than the limit
        var metricDefinitions = GenerateMetricDefinitions(3);

        _service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(metricDefinitions);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--resource test --subscription sub1 --limit 10");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that all results are returned without truncation
        Assert.Equal("All 3 metric definitions returned.", response.Message);
        // Verify the service was called
        await _service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region Option Binding Tests

    [Fact]
    public async Task ExecuteAsync_BindsOptionsCorrectly()
    {
        // Arrange
        var expectedResults = new List<MetricDefinition>
        {
            new()
            {
                Name = "Performance Counter",
                Description = "VM performance metrics",
                Category = "Performance",
                Unit = "Count"
            }
        };

        _service.ListMetricDefinitionsAsync(
            "test-subscription",
            null, // resource-group may be null if not provided or not parsed from resource-id
            "Microsoft.Compute/virtualMachines",
            "test-vm",
            "Microsoft.Compute/virtualMachines",
            "performance",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResults);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(
            "--subscription test-subscription --resource-type Microsoft.Compute/virtualMachines --resource test-vm --metric-namespace Microsoft.Compute/virtualMachines --search-string performance --tenant test-tenant --limit 25");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 1 metric definitions returned.", response.Message);
        await _service.Received(1).ListMetricDefinitionsAsync(
            "test-subscription",
            null,
            "Microsoft.Compute/virtualMachines",
            "test-vm",
            "Microsoft.Compute/virtualMachines",
            "performance",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region Helper Methods

    private static List<MetricDefinition> GenerateMetricDefinitions(int count)
    {
        var definitions = new List<MetricDefinition>();
        for (int i = 0; i < count; i++)
        {
            definitions.Add(new MetricDefinition
            {
                Name = $"Metric{i}",
                Description = $"Description for metric {i}",
                Category = "Performance",
                Unit = "Count",
                SupportedAggregationTypes = ["Average"],
                IsDimensionRequired = false,
                Dimensions = []
            });
        }
        return definitions;
    }

    #endregion
}
