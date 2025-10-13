// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.WebTests;

public class WebTestsCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorWebTestService _service;
    private readonly ILogger<WebTestsCreateCommand> _logger;
    private readonly WebTestsCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebTestsCreateCommandTests()
    {
        _service = Substitute.For<IMonitorWebTestService>();
        _logger = Substitute.For<ILogger<WebTestsCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Creates a new standard web test", command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("create", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create a web test in Azure Monitor", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.Contains("standard web test", description);
        Assert.Contains("Azure Monitor", description);
        Assert.Contains("deprecated", description);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var metadata = _command.Metadata;
        Assert.True(metadata.Destructive);
        Assert.False(metadata.Idempotent); // Create is not idempotent
        Assert.False(metadata.ReadOnly);
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

        // Required options
        Assert.Contains("--webtest-resource", options);
        Assert.Contains("--resource-group", options);
        Assert.Contains("--appinsights-component", options);
        Assert.Contains("--location", options);
        Assert.Contains("--webtest-locations", options);
        Assert.Contains("--request-url", options);

        // Optional options
        Assert.Contains("--webtest", options);
        Assert.Contains("--description", options);
        Assert.Contains("--enabled", options);
        Assert.Contains("--expected-status-code", options);
        Assert.Contains("--follow-redirects", options);
        Assert.Contains("--frequency", options);
        Assert.Contains("--headers", options);
        Assert.Contains("--http-verb", options);
        Assert.Contains("--ignore-status-code", options);
        Assert.Contains("--parse-requests", options);
        Assert.Contains("--request-body", options);
        Assert.Contains("--retry-enabled", options);
        Assert.Contains("--ssl-check", options);
        Assert.Contains("--ssl-lifetime-check", options);
        Assert.Contains("--timeout", options);

        // Verify required options are marked as required
        var requiredOptions = command.Options.Where(o => o.Required).Select(o => o.Name).ToList();
        Assert.Contains("--webtest-resource", requiredOptions);
        Assert.Contains("--appinsights-component", requiredOptions);
        Assert.Contains("--location", requiredOptions);
        Assert.Contains("--webtest-locations", requiredOptions);
        Assert.Contains("--request-url", requiredOptions);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://sub.example.com/path", true)]
    [InlineData("example.com", false)]
    [InlineData("ftp://example.com", true)]
    [InlineData("invalid-url", false)]
    [InlineData("", false)]
    public async Task Validate_RequestUrl_ValidatesCorrectly(string requestUrl, bool shouldBeValid)
    {
        // Arrange
        var args = new[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "--location", "eastus",
            "--webtest-locations", "East US",
            "--request-url", requestUrl
        };
        var parseResult = _commandDefinition.Parse(args);

        if (shouldBeValid)
        {
            var expectedResult = new WebTestDetailedInfo
            {
                ResourceName = "webtest1",
                Location = "eastus"
            };

            _service.CreateWebTest(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
                Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
                Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(expectedResult);
        }

        // Act
        var result = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (!shouldBeValid)
        {
            Assert.Equal(HttpStatusCode.BadRequest, result.Status);
            if (requestUrl == "")
            {
                Assert.Contains("Missing Required options: --request-url", result.Message);
            }
            else
            {
                Assert.Contains("must be a valid absolute URL", result.Message);
            }
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
    }

    [Theory]
    [InlineData("US East", true)]
    [InlineData("US East,US West", true)]
    [InlineData("US East, US West, US Central", true)]
    [InlineData("", false)]
    public async Task Validate_Locations_ValidatesCorrectly(string locations, bool shouldBeValid)
    {
        // Arrange
        var args = new[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "--location", "eastus",
            "--webtest-locations", locations,
            "--request-url", "https://example.com"
        };
        var parseResult = _commandDefinition.Parse(args);

        if (shouldBeValid)
        {
            var expectedResult = new WebTestDetailedInfo
            {
                ResourceName = "webtest1",
                Location = "eastus"
            };
            _service.CreateWebTest(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
                Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
                Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(expectedResult);
        }

        // Act
        var result = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (!shouldBeValid)
        {
            Assert.Equal(HttpStatusCode.BadRequest, result.Status);
            if (locations == "")
            {
                Assert.Contains("Missing Required options: --webtest-locations", result.Message);
            }
            else
            {
                Assert.Contains("must include at least one location", result.Message);
            }
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    [InlineData(300, false)]
    public async Task Validate_TimeoutInSeconds_ValidatesCorrectly(int timeoutInSeconds, bool shouldBeValid)
    {
        // Arrange
        var args = new[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "--location", "eastus",
            "--webtest-locations", "US East",
            "--request-url", "https://example.com",
            "--timeout", timeoutInSeconds.ToString()
        };

        var parseResult = _commandDefinition.Parse(args);
        if (shouldBeValid)
        {
            var expectedResult = new WebTestDetailedInfo
            {
                ResourceName = "webtest1",
                Location = "eastus"
            };
            _service.CreateWebTest(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
                Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
                Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(expectedResult);
        }

        // Act
        var result = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (!shouldBeValid)
        {
            Assert.Equal(HttpStatusCode.BadRequest, result.Status);
            Assert.Contains("cannot be greater than 2 minutes", result.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
    }

    #endregion

    #region ExecuteAsync Tests - Success Scenarios

    [Fact]
    public async Task ExecuteAsync_ValidMinimalInput_ReturnsSuccess()
    {
        // Arrange
        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1", "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1", "--location", "eastus", "--webtest-locations", "US East", "--request-url", "https://example.com" };
        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/webtest1",
            Kind = "standard",
            IsEnabled = true,
            FrequencyInSeconds = 300,
            TimeoutInSeconds = 30
        };

        _service.CreateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the actual content of the results
        var result = GetResult(response.Results);
        Assert.NotNull(result);
        Assert.Equal("webtest1", result.ResourceName);
        Assert.Equal("eastus", result.Location);
        Assert.Equal("standard", result.Kind);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCompleteInput_ReturnsSuccess()
    {
        // Arrange
        var args = new string[] {
            "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1",
            "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "--location", "eastus", "--webtest-locations", "US East,US West", "--request-url", "https://example.com",
            "--webtest", "My Web Test", "--description", "Test description", "--enabled", "true",
            "--expected-status-code", "200", "--follow-redirects", "true", "--frequency", "600",
            "--headers", "Content-Type=application/json,Authorization=Bearer token", "--http-verb", "POST",
            "--ignore-status-code", "false", "--parse-requests", "true", "--request-body", "{\"test\":\"data\"}",
            "--retry-enabled", "true", "--ssl-check", "true", "--ssl-lifetime-check", "30", "--timeout", "60"
        };

        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            WebTestName = "My Web Test",
            IsEnabled = true,
            FrequencyInSeconds = 600,
            TimeoutInSeconds = 60,
            IsRetryEnabled = true
        };

        _service.CreateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify service was called with correct parameters
        await _service.Received(1).CreateWebTest(
            "sub1",
            "rg1",
            "webtest1",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "eastus",
            Arg.Is<string[]>(x => x.SequenceEqual(new[] { "US East", "US West" })),
            "https://example.com",
            "My Web Test",
            "Test description",
            true,
            200,
            true,
            600,
            Arg.Is<IReadOnlyDictionary<string, string>>(h =>
                h.Count == 2 &&
                h["Content-Type"] == "application/json" &&
                h["Authorization"] == "Bearer token"),
            "POST",
            false,
            true,
            "{\"test\":\"data\"}",
            true,
            true,
            30,
            60,
            null,
            Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region ExecuteAsync Tests - Validation Failures

    [Theory]
    [InlineData("")]                                                        // Missing all required options
    [InlineData("--subscription sub1")]                                    // Missing other required options
    [InlineData("--subscription sub1 --resource-group rg1")]              // Missing resource-name and others
    [InlineData("--subscription sub1 --resource-group rg1 --webtest-resource webtest1")] // Missing app-insights-component-id and others
    public async Task ExecuteAsync_InvalidInput_ReturnsBadRequest(string args)
    {
        // Arrange
        var argArray = string.IsNullOrEmpty(args) ? Array.Empty<string>() : args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parseResult = _commandDefinition.Parse(argArray);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.Results);
    }

    #endregion

    #region ExecuteAsync Tests - Error Handling

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedException = new Exception("Service unavailable");
        _service.When(x => x.CreateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()))
            .Do(x => throw expectedException);

        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1", "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1", "--location", "eastus", "--webtest-locations", "US East", "--request-url", "https://example.com" };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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
        _service.When(x => x.CreateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()))
            .Do(x => throw expectedException);

        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1", "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1", "--location", "eastus", "--webtest-locations", "US East", "--request-url", "https://example.com" };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        await _command.ExecuteAsync(_context, parseResult);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error creating web test")),
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
        return JsonSerializer.Deserialize<WebTestsCreateCommandResult>(json)?.webTest;
    }

    private record WebTestsCreateCommandResult(WebTestDetailedInfo webTest);

    #endregion
}
