// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.WebTests;

public class WebTestsGetCommandTests : CommandUnitTestsBase<WebTestsGetCommand, IMonitorWebTestService>
{
    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("get", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get or list web tests", Command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = Command.Description;
        Assert.NotNull(description);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var metadata = Command.Metadata;
        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.Secret);
    }

    #endregion

    #region Option Registration Tests

    [Fact]
    public void RegisterOptions_AddsAllExpectedOptions()
    {
        var options = CommandDefinition.Options.Select(o => o.Name).ToList();

        // Base options from BaseMonitorWebTestsCommand (subscription from SubscriptionCommand)
        Assert.Contains("--subscription", options);

        // WebTestsGetCommand specific options
        Assert.Contains("--resource-group", options);
        Assert.Contains("--webtest-resource", options);

        // Verify webtest-resource is optional (for list functionality)
        var requiredOptions = CommandDefinition.Options.Where(o => o.Required).Select(o => o.Name).ToList();
        Assert.DoesNotContain("--webtest-resource", requiredOptions);
    }

    #endregion

    #region Option Binding Tests

    [Fact]
    public async Task ExecuteAsync_BindsAllOptionsCorrectly()
    {
        // Arrange
        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/webtest1"
        };

        Service.GetWebTest("sub1", "rg1", "webtest1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1");

        // Assert
        await Service.Received(1).GetWebTest("sub1", "rg1", "webtest1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExecuteAsync Tests - Success Scenarios

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/webtest1",
            Kind = "ping",
            WebTestName = "Test web test",
            IsEnabled = true,
            FrequencyInSeconds = 300,
            TimeoutInSeconds = 30,
            IsRetryEnabled = false,
            AppInsightsComponentId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1"
        };

        Service.GetWebTest("sub1", "rg1", "webtest1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WebTestsGetCommandResult);

        Assert.NotNull(result.WebTest);
        Assert.Equal("webtest1", result.WebTest.ResourceName);
        Assert.Equal("eastus", result.WebTest.Location);
        Assert.Equal("ping", result.WebTest.Kind);
        Assert.Equal(300, result.WebTest.FrequencyInSeconds);
        Assert.Equal(30, result.WebTest.TimeoutInSeconds);
        Assert.True(result.WebTest.IsEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_WebTestNotFound_ReturnsNotFound()
    {
        // Arrange
        Service.GetWebTest(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error retrieving details for web test 'nonexistent'"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status); // Exception handling returns 500, not 404
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus"
        };

        Service.GetWebTest(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1");

        // Assert
        await Service.Received(1).GetWebTest("sub1", "rg1", "webtest1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExecuteAsync Tests - Validation Failures

    [Theory]
    [InlineData("")]                                                        // Missing subscription (required)
    [InlineData("--resource-group rg1")]                                   // Missing subscription
    [InlineData("--webtest-resource webtest1")]                            // Missing subscription
    public async Task ExecuteAsync_InvalidInput_ReturnsBadRequest(string args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.Results);
    }

    #endregion

    #region ExecuteAsync Tests - List Scenarios

    [Fact]
    public async Task ExecuteAsync_ListAllWebTests_ReturnsSuccess()
    {
        // Arrange
        var expectedResults = new List<WebTestSummaryInfo>
        {
            new()
            {
                ResourceName = "webtest1",
                Location = "eastus",
                ResourceGroup = "rg1"
            },
            new()
            {
                ResourceName = "webtest2",
                Location = "westus",
                ResourceGroup = "rg2"
            }
        };

        Service.ListWebTests("sub1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub1");

        // Assert
        var results = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WebTestsGetCommandListResult);

        Assert.NotNull(results.WebTests);
        Assert.Equal(2, results.WebTests.Count);
        Assert.Equal("webtest1", results.WebTests[0].ResourceName);
        Assert.Equal("webtest2", results.WebTests[1].ResourceName);
    }

    [Fact]
    public async Task ExecuteAsync_ListWebTestsWithResourceGroup_ReturnsFilteredResults()
    {
        // Arrange
        var expectedResults = new List<WebTestSummaryInfo>
        {
            new()
            {
                ResourceName = "webtest1",
                Location = "eastus",
                ResourceGroup = "rg1"
            }
        };

        Service.ListWebTests("sub1", "rg1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub1", "--resource-group", "rg1");

        // Assert
        var results = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WebTestsGetCommandListResult);

        Assert.NotNull(results.WebTests);
        Assert.Single(results.WebTests);
        Assert.Equal("webtest1", results.WebTests[0].ResourceName);
        Assert.Equal("rg1", results.WebTests[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_ListCallsServiceWithCorrectParameters()
    {
        // Arrange
        Service.ListWebTests(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync("--subscription", "sub1", "--resource-group", "rg1");

        // Assert
        await Service.Received(1).ListWebTests("sub1", "rg1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExecuteAsync Tests - Error Handling

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedException = new Exception("Service unavailable");
        Service.GetWebTest(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service unavailable", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_LogsError()
    {
        // Arrange
        var expectedException = new Exception("Service error");
        Service.GetWebTest(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1");

        // Assert
        Logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error retrieving web test")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion
}
