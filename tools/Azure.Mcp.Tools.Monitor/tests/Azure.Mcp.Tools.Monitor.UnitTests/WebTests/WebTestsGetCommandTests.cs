// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.WebTests;

public class WebTestsGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorWebTestService _service;
    private readonly ILogger<WebTestsGetCommand> _logger;
    private readonly WebTestsGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebTestsGetCommandTests()
    {
        _service = Substitute.For<IMonitorWebTestService>();
        _logger = Substitute.For<ILogger<WebTestsGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Gets details for a specific web test", command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("get", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get or list web tests", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.Contains("specific web test or lists all web tests", description);
        Assert.Contains("--webtest-resource is provided", description);
        Assert.Contains("--webtest-resource is omitted", description);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var metadata = _command.Metadata;
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
        var command = _command.GetCommand();
        var options = command.Options.Select(o => o.Name).ToList();

        // Base options from BaseMonitorWebTestsCommand (subscription from SubscriptionCommand)
        Assert.Contains("--subscription", options);

        // WebTestsGetCommand specific options
        Assert.Contains("--resource-group", options);
        Assert.Contains("--webtest-resource", options);

        // Verify webtest-resource is optional (for list functionality)
        var requiredOptions = command.Options.Where(o => o.Required).Select(o => o.Name).ToList();
        Assert.DoesNotContain("--webtest-resource", requiredOptions);
    }

    #endregion

    #region Option Binding Tests

    [Fact]
    public async Task ExecuteAsync_BindsAllOptionsCorrectly()
    {
        // Arrange
        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1" };
        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/webtest1"
        };

        _service.GetWebTest("sub1", "rg1", "webtest1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).GetWebTest("sub1", "rg1", "webtest1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExecuteAsync Tests - Success Scenarios

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1" };
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

        _service.GetWebTest("sub1", "rg1", "webtest1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the actual content of the results
        var result = GetResult(response.Results);
        Assert.NotNull(result);
        Assert.Equal("webtest1", result.ResourceName);
        Assert.Equal("eastus", result.Location);
        Assert.Equal("ping", result.Kind);
        Assert.Equal(300, result.FrequencyInSeconds);
        Assert.Equal(30, result.TimeoutInSeconds);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_WebTestNotFound_ReturnsNotFound()
    {
        // Arrange
        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "nonexistent" };

        // The service throws an exception when a web test is not found (as seen in implementation)
        _service.GetWebTest(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error retrieving details for web test 'nonexistent'"));

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        _service.GetWebTest(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1"]);

        // Act
        await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).GetWebTest("sub1", "rg1", "webtest1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExecuteAsync Tests - Validation Failures

    [Theory]
    [InlineData("")]                                                        // Missing subscription (required)
    [InlineData("--resource-group rg1")]                                   // Missing subscription
    [InlineData("--webtest-resource webtest1")]                            // Missing subscription
    public async Task ExecuteAsync_InvalidInput_ReturnsBadRequest(string args)
    {
        // Arrange
        var argArray = string.IsNullOrEmpty(args) ? Array.Empty<string>() : args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parseResult = _commandDefinition.Parse(argArray);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
        var args = new string[] { "--subscription", "sub1" };
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

        _service.ListWebTests("sub1", null, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the actual content of the results
        var results = GetListResult(response.Results);
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal("webtest1", results[0].ResourceName);
        Assert.Equal("webtest2", results[1].ResourceName);
    }

    [Fact]
    public async Task ExecuteAsync_ListWebTestsWithResourceGroup_ReturnsFilteredResults()
    {
        // Arrange
        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1" };
        var expectedResults = new List<WebTestSummaryInfo>
        {
            new()
            {
                ResourceName = "webtest1",
                Location = "eastus",
                ResourceGroup = "rg1"
            }
        };

        _service.ListWebTests("sub1", "rg1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var results = GetListResult(response.Results);
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("webtest1", results[0].ResourceName);
        Assert.Equal("rg1", results[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_ListCallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedResults = new List<WebTestSummaryInfo>();

        _service.ListWebTests(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1", "--resource-group", "rg1"]);

        // Act
        await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).ListWebTests("sub1", "rg1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExecuteAsync Tests - Error Handling

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedException = new Exception("Service unavailable");
        _service.GetWebTest(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
        _service.GetWebTest(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1"]);

        // Act
        await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error retrieving web test")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region Helper Methods

    private WebTestDetailedInfo? GetResult(ResponseResult? result)
    {
        if (result == null)
        {
            return null;
        }
        var json = JsonSerializer.Serialize(result);
        return JsonSerializer.Deserialize<WebTestsGetCommandResult>(json)?.webTest;
    }

    private List<WebTestSummaryInfo>? GetListResult(ResponseResult? result)
    {
        if (result == null)
        {
            return null;
        }
        var json = JsonSerializer.Serialize(result);
        return JsonSerializer.Deserialize<WebTestsGetCommandListResult>(json)?.webTests;
    }

    private record WebTestsGetCommandResult(WebTestDetailedInfo webTest);
    private record WebTestsGetCommandListResult(List<WebTestSummaryInfo> webTests);

    #endregion
}
