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

public class WebTestsCreateOrUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorWebTestService _service;
    private readonly ILogger<WebTestsCreateOrUpdateCommand> _logger;
    private readonly WebTestsCreateOrUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebTestsCreateOrUpdateCommandTests()
    {
        _service = Substitute.For<IMonitorWebTestService>();
        _logger = Substitute.For<ILogger<WebTestsCreateOrUpdateCommand>>();

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
        Assert.Equal("createorupdate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Create or update", command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("createorupdate", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create or update a web test in Azure Monitor", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.Contains("Create or update", description);
        Assert.Contains("standard web test", description);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var metadata = _command.Metadata;
        Assert.True(metadata.Destructive);
        Assert.True(metadata.Idempotent);
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

        // Required base options
        Assert.Contains("--subscription", options);
        Assert.Contains("--resource-group", options);
        Assert.Contains("--webtest-resource", options);

        // Configuration options
        Assert.Contains("--location", options);
        Assert.Contains("--appinsights-component", options);
        Assert.Contains("--request-url", options);
        Assert.Contains("--webtest-locations", options);
        Assert.Contains("--webtest", options);
        Assert.Contains("--description", options);
        Assert.Contains("--enabled", options);
        Assert.Contains("--frequency", options);
        Assert.Contains("--timeout", options);

        // Verify required options are marked as required
        var requiredOptions = command.Options.Where(o => o.Required).Select(o => o.Name).ToList();
        Assert.Contains("--resource-group", requiredOptions);
        Assert.Contains("--webtest-resource", requiredOptions);
    }

    #endregion

    #region ExecuteAsync Tests - Create Scenarios

    [Fact]
    public async Task ExecuteAsync_CreateNewWebTest_ReturnsSuccess()
    {
        // Arrange
        var args = new string[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "newwebtest",
            "--location", "eastus",
            "--appinsights-component", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "--request-url", "https://example.com",
            "--webtest-locations", "us-il-ch1-azr,us-ca-sjc-azr"
        };

        var expectedResult = new WebTestDetailedInfo
        {
            ResourceName = "newwebtest",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/newwebtest",
            AppInsightsComponentId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1"
        };

        // Setup GetWebTest to throw (resource doesn't exist - CREATE scenario)
        _service.GetWebTest("sub1", "rg1", "newwebtest", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Resource not found"));

        _service.CreateWebTest(
            "sub1",
            "rg1",
            "newwebtest",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/components/appinsights1",
            "eastus",
            Arg.Any<string[]>(),
            "https://example.com",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        var result = GetResult(response.Results);
        Assert.NotNull(result);
        Assert.Equal("newwebtest", result.ResourceName);
        Assert.Equal("eastus", result.Location);
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithoutRequiredParameters_ReturnsError()
    {
        // Arrange - missing required create parameters like location, appinsights-component, request-url
        var args = new string[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "newwebtest"
        };

        // Setup GetWebTest to throw (resource doesn't exist - CREATE scenario)
        _service.GetWebTest("sub1", "rg1", "newwebtest", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Resource not found"));

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert - Command catches validation errors and returns InternalServerError
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    #endregion

    #region ExecuteAsync Tests - Update Scenarios

    [Fact]
    public async Task ExecuteAsync_UpdateExistingWebTest_ReturnsSuccess()
    {
        // Arrange
        var args = new string[]
        {
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--webtest-resource", "existingwebtest",
            "--enabled", "false",
            "--frequency", "600"
        };

        var existingWebTest = new WebTestDetailedInfo
        {
            ResourceName = "existingwebtest",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/existingwebtest",
            IsEnabled = true,
            FrequencyInSeconds = 300
        };

        var updatedWebTest = new WebTestDetailedInfo
        {
            ResourceName = "existingwebtest",
            Location = "eastus",
            ResourceGroup = "rg1",
            Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Insights/webtests/existingwebtest",
            IsEnabled = false,
            FrequencyInSeconds = 600
        };

        // Setup GetWebTest to return existing resource (UPDATE scenario)
        _service.GetWebTest("sub1", "rg1", "existingwebtest", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(existingWebTest);

        _service.UpdateWebTest(
            "sub1",
            "rg1",
            "existingwebtest",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            false,
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            600,
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWebTest);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = GetResult(response.Results);
        Assert.NotNull(result);
        Assert.Equal("existingwebtest", result.ResourceName);
        Assert.False(result.IsEnabled);
        Assert.Equal(600, result.FrequencyInSeconds);
    }

    #endregion

    #region ExecuteAsync Tests - Validation

    [Theory]
    [InlineData("")]                                                        // Missing all required
    [InlineData("--subscription sub1")]                                    // Missing resource-group and webtest-resource
    [InlineData("--subscription sub1 --resource-group rg1")]              // Missing webtest-resource
    [InlineData("--resource-group rg1 --webtest-resource test1")]         // Missing subscription
    public async Task ExecuteAsync_MissingRequiredParameters_ReturnsBadRequest(string args)
    {
        // Arrange
        var argArray = string.IsNullOrEmpty(args) ? Array.Empty<string>() : args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parseResult = _commandDefinition.Parse(argArray);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotEmpty(response.Message);
    }

    #endregion

    #region ExecuteAsync Tests - Error Handling

    #endregion

    #region Helper Methods

    private WebTestDetailedInfo? GetResult(ResponseResult? result)
    {
        if (result == null)
        {
            return null;
        }
        var json = JsonSerializer.Serialize(result);
        return JsonSerializer.Deserialize<WebTestsCreateOrUpdateCommandResult>(json)?.webTest;
    }

    private record WebTestsCreateOrUpdateCommandResult(WebTestDetailedInfo webTest);

    #endregion
}
