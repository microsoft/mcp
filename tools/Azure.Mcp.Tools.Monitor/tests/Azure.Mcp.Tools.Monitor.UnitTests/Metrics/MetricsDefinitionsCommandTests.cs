// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Metrics;

public class MetricsDefinitionsCommandTests : CommandUnitTestsBase<MetricsDefinitionsCommand, IMonitorMetricsService>
{
    #region Constructor and Command Setup Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("definitions", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void Name_ReturnsDefinitions()
    {
        Assert.Equal("definitions", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectTitle()
    {
        Assert.Equal("List Azure Monitor Metric Definitions", Command.Title);
    }

    [Fact]
    public void GetCommand_RegistersAllRequiredOptions()
    {
        // Check that all required options are present
        var optionNames = CommandDefinition.Options.Select(o => o.Name).ToList();

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
            Service.ListMetricDefinitionsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
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

        // Act
        var response = await ExecuteCommandAsync(args);

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

        Service.ListMetricDefinitionsAsync(
            "sub1",
            null, // resource-group may be null if not provided or not parsed from resource-id
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            null,
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--resource", "test",
            "--subscription", "sub1",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--metric-namespace", "Microsoft.Storage/storageAccounts",
            "--tenant", "tenant1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 1 metric definitions returned.", response.Message);
        await Service.Received(1).ListMetricDefinitionsAsync(
            "sub1",
            null,
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            null,
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithSearchString_CallsServiceWithSearchParameter()
    {
        // Arrange
        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            "cpu",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
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

        // Act
        var response = await ExecuteCommandAsync(
            "--resource", "test",
            "--subscription", "sub1",
            "--search-string", "cpu");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with the search string
        await Service.Received(1).ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            "cpu",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
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

        Service.ListMetricDefinitionsAsync(
            "sub1",
            null, // resource-group may be null if not provided or not parsed from resource-id
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            "memory",
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--resource", "test",
            "--subscription", "sub1",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--metric-namespace", "Microsoft.Storage/storageAccounts",
            "--search-string", "memory",
            "--tenant", "tenant1",
            "--limit", "20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 1 metric definitions returned.", response.Message);
        await Service.Received(1).ListMetricDefinitionsAsync(
            "sub1",
            null,
            "Microsoft.Storage/storageAccounts",
            "test",
            "Microsoft.Storage/storageAccounts",
            "memory",
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1");

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
        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var response = await ExecuteCommandAsync(
            "--resource", "test",
            "--subscription", "sub1",
            "--resource-group", "rg1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Logger.Received(1).Log(
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

        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(metricDefinitions);

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 2 metric definitions returned.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoResults_ReturnsNullResults()
    {
        // Arrange
        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullResults_ReturnsNullResults()
    {
        // Arrange
        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<MetricDefinition>>(null!));

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1");

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

        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(metricDefinitions);

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that results were truncated - message should indicate truncation
        Assert.Contains("Results truncated to 10 of 15", response.Message);
        Assert.Contains("metric definitions", response.Message);
        // Verify service receives all data but command applies limit internally
        await Service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomLimit_TruncatesResultsCorrectly()
    {
        // Arrange
        var metricDefinitions = GenerateMetricDefinitions(20);

        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(metricDefinitions);

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1", "--limit", "5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that results were truncated to the custom limit
        Assert.Contains("Results truncated to 5 of 20", response.Message);
        Assert.Contains("metric definitions", response.Message);
        // Verify service is called correctly
        await Service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResultsExceedingLimit_ShowsTruncationMessage()
    {
        // Arrange - Create more results than the limit
        var metricDefinitions = GenerateMetricDefinitions(25);

        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(metricDefinitions);

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1", "--limit", "8");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that the message indicates truncation with correct counts
        Assert.Contains("Results truncated to 8 of 25", response.Message);
        Assert.Contains("Use --search-string to filter results", response.Message);
        // Verify the service was called
        await Service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResultsUnderLimit_ShowsAllResultsMessage()
    {
        // Arrange - Create fewer results than the limit
        var metricDefinitions = GenerateMetricDefinitions(3);

        Service.ListMetricDefinitionsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(metricDefinitions);

        // Act
        var response = await ExecuteCommandAsync("--resource", "test", "--subscription", "sub1", "--limit", "10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        // Verify that all results are returned without truncation
        Assert.Equal("All 3 metric definitions returned.", response.Message);
        // Verify the service was called
        await Service.Received(1).ListMetricDefinitionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
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

        Service.ListMetricDefinitionsAsync(
            "test-subscription",
            null, // resource-group may be null if not provided or not parsed from resource-id
            "Microsoft.Compute/virtualMachines",
            "test-vm",
            "Microsoft.Compute/virtualMachines",
            "performance",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-type", "Microsoft.Compute/virtualMachines",
            "--resource", "test-vm",
            "--metric-namespace", "Microsoft.Compute/virtualMachines",
            "--search-string", "performance",
            "--tenant", "test-tenant",
            "--limit", "25");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("All 1 metric definitions returned.", response.Message);
        await Service.Received(1).ListMetricDefinitionsAsync(
            "test-subscription",
            null,
            "Microsoft.Compute/virtualMachines",
            "test-vm",
            "Microsoft.Compute/virtualMachines",
            "performance",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Helper Methods

    private static List<MetricDefinition> GenerateMetricDefinitions(int count)
    {
        var definitions = new List<MetricDefinition>();
        for (int i = 0; i < count; i++)
        {
            definitions.Add(new()
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
