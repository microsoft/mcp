// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Extensions;
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

public class WebTestsUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorWebTestService _service;
    private readonly ILogger<WebTestsUpdateCommand> _logger;
    private readonly WebTestsUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebTestsUpdateCommandTests()
    {
        _service = Substitute.For<IMonitorWebTestService>();
        _logger = Substitute.For<ILogger<WebTestsUpdateCommand>>();

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
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Updates an existing standard web test", command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("update", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Update a web test in Azure Monitor", _command.Title);
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
        Assert.True(metadata.Idempotent); // Update is idempotent
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

        // Required options (only resource name and resource group for update)
        Assert.Contains("--webtest-resource", options);
        Assert.Contains("--resource-group", options);

        // Optional options (most options become optional for update)
        Assert.Contains("--appinsights-component", options);
        Assert.Contains("--location", options);
        Assert.Contains("--webtest-locations", options);
        Assert.Contains("--request-url", options);
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
        Assert.Contains("--resource-group", requiredOptions);

        // These should NOT be required for update
        Assert.DoesNotContain("--appinsights-component", requiredOptions);
        Assert.DoesNotContain("--location", requiredOptions);
        Assert.DoesNotContain("--webtest-locations", requiredOptions);
        Assert.DoesNotContain("--request-url", requiredOptions);
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
    public async Task Validate_RequestUrl_ValidatesCorrectly(string requestUrl, bool shouldBeValid)
    {
        // Arrange
        var args = new[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
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

            _service.UpdateWebTest(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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
            Assert.Contains("must be a valid absolute URL", result.Message);
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
    public async Task Validate_Locations_ValidatesCorrectly(string locations, bool shouldBeValid)
    {
        // Arrange
        var args = new[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--webtest-locations", locations
        };
        var parseResult = _commandDefinition.Parse(args);

        if (shouldBeValid)
        {
            var expectedResult = new WebTestDetailedInfo
            {
                ResourceName = "webtest1",
                Location = "eastus"
            };
            _service.UpdateWebTest(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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
            Assert.Contains("at least one location must be provided", result.Message);
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
            _service.UpdateWebTest(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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
        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1" };
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

        _service.UpdateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "--location", "eastus",
            "--webtest-locations", "US East,US West",
            "--request-url", "https://example.com",
            "--webtest", "My Updated Web Test",
            "--description", "Updated test description",
            "--enabled", "false",
            "--expected-status-code", "204",
            "--follow-redirects", "false",
            "--frequency", "900",
            "--headers", "Content-Type=application/xml,Authorization=Bearer newtoken",
            "--http-verb", "PUT",
            "--ignore-status-code", "true",
            "--parse-requests", "false",
            "--request-body", "{\"updated\":\"data\"}",
            "--retry-enabled", "false",
            "--ssl-check", "false",
            "--ssl-lifetime-check", "30",
            "--timeout", "90"
        };

        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            WebTestName = "My Updated Web Test",
            IsEnabled = false,
            FrequencyInSeconds = 900,
            TimeoutInSeconds = 90,
            IsRetryEnabled = false
        };

        _service.UpdateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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
        await _service.Received(1).UpdateWebTest(
            "sub1",
            "rg1",
            "webtest1",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "eastus",
            Arg.Is<string[]>(x => x != null && x.SequenceEqual(new[] { "US East", "US West" })),
            "https://example.com",
            "My Updated Web Test",
            "Updated test description",
            false,
            204,
            false,
            900,
            Arg.Is<IReadOnlyDictionary<string, string>>(h =>
                h != null && h.Count == 2 &&
                h["Content-Type"] == "application/xml" &&
                h["Authorization"] == "Bearer newtoken"),
            "PUT",
            true,
            false,
            "{\"updated\":\"data\"}",
            false,
            false,
            30,
            90,
            null,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_OnlyExplicitValues_PassesNullForNonProvidedOptions()
    {
        // Arrange - Only provide subscription, resource group, resource name, and frequency
        var args = new string[] {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--frequency", "600"  // Only provide this one optional field
        };

        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            FrequencyInSeconds = 600
        };

        _service.UpdateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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

        // Verify that only explicitly provided values are passed to the service
        // All other optional parameters should be null
        await _service.Received(1).UpdateWebTest(
            "sub1",                    // subscription
            "rg1",                     // resourceGroup
            "webtest1",                // resourceName
            null,                      // appInsightsComponentId - not provided, should be null
            null,                      // location - not provided, should be null
            null,                      // locations - not provided, should be null
            null,                      // requestUrl - not provided, should be null
            null,                      // webTestName - not provided, should be null
            null,                      // description - not provided, should be null
            null,                      // enabled - not provided, should be null
            null,                      // expectedStatusCode - not provided, should be null
            null,                      // followRedirects - not provided, should be null
            600,                       // frequencyInSeconds - explicitly provided
            null,                      // headers - not provided, should be null
            null,                      // httpVerb - not provided, should be null
            null,                      // ignoreStatusCode - not provided, should be null
            null,                      // parseRequests - not provided, should be null
            null,                      // requestBody - not provided, should be null
            null,                      // retryEnabled - not provided, should be null
            null,                      // sslCheck - not provided, should be null
            null,                      // sslLifetimeCheckInDays - not provided, should be null
            null,                      // timeoutInSeconds - not provided, should be null
            null,                      // tenant
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_MixedExplicitAndDefaultValues_OnlyPassesExplicitValues()
    {
        // Arrange - Mix of explicitly provided and default values
        var args = new string[] {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "webtest1",
            "--enabled", "true",       // Explicitly set to true
            "--timeout", "30"          // Explicitly set to 30
            // Leave out other options to test that defaults are not used
        };

        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "webtest1",
            Location = "eastus",
            ResourceGroup = "rg1",
            IsEnabled = true,
            TimeoutInSeconds = 30
        };

        _service.UpdateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
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

        // Verify that only explicitly provided values are passed to the service
        await _service.Received(1).UpdateWebTest(
            "sub1",                    // subscription
            "rg1",                     // resourceGroup
            "webtest1",                // resourceName
            null,                      // appInsightsComponentId - not provided, should be null
            null,                      // location - not provided, should be null
            null,                      // locations - not provided, should be null
            null,                      // requestUrl - not provided, should be null
            null,                      // webTestName - not provided, should be null
            null,                      // description - not provided, should be null
            true,                      // enabled - explicitly provided as true
            null,                      // expectedStatusCode - not provided, should be null
            null,                      // followRedirects - not provided, should be null
            null,                      // frequencyInSeconds - not provided, should be null
            null,                      // headers - not provided, should be null
            null,                      // httpVerb - not provided, should be null
            null,                      // ignoreStatusCode - not provided, should be null
            null,                      // parseRequests - not provided, should be null
            null,                      // requestBody - not provided, should be null
            null,                      // retryEnabled - not provided, should be null
            null,                      // sslCheck - not provided, should be null
            null,                      // sslLifetimeCheckInDays - not provided, should be null
            30,                        // timeoutInSeconds - explicitly provided as 30
            null,                      // tenant
            Arg.Any<RetryPolicyOptions?>());
    }

    #endregion

    #region ExecuteAsync Tests - Validation Failures

    [Theory]
    [InlineData("")]                                                        // Missing all required options
    [InlineData("--subscription sub1")]                                    // Missing other required options
    [InlineData("--subscription sub1 --webtest-resource webtest1")]       // Missing resource-group
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
        _service.When(x => x.UpdateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()))
            .Do(x => throw expectedException);

        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1" };
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
        _service.When(x => x.UpdateWebTest(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<int?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<string?>(),
            Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()))
            .Do(x => throw expectedException);

        var args = new string[] { "--subscription", "sub1", "--resource-group", "rg1", "--webtest-resource", "webtest1" };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        await _command.ExecuteAsync(_context, parseResult);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error updating web test")),
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
        return JsonSerializer.Deserialize<WebTestsUpdateCommandResult>(json)?.webTest;
    }

    private record WebTestsUpdateCommandResult(WebTestDetailedInfo webTest);

    #endregion
}
