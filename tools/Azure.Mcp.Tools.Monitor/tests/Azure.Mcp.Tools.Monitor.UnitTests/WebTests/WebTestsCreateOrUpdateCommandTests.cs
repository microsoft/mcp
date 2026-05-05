// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.WebTests;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.WebTests;

public class WebTestsCreateOrUpdateCommandTests : CommandUnitTestsBase<WebTestsCreateOrUpdateCommand, IMonitorWebTestService>
{
    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("createorupdate", CommandDefinition.Name);
        Assert.False(string.IsNullOrEmpty(CommandDefinition.Description));
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("createorupdate", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create or update a web test in Azure Monitor", Command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = Command.Description;
        Assert.False(string.IsNullOrEmpty(description));
        Assert.True(description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var metadata = Command.Metadata;
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
        var options = CommandDefinition.Options.Select(o => o.Name).ToList();

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
        var requiredOptions = CommandDefinition.Options.Where(o => o.Required).Select(o => o.Name).ToList();
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
        Service.GetWebTest("sub1", "rg1", "newwebtest", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Resource not found"));

        Service.CreateWebTest(
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

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WebTestsCreateOrUpdateCommandResult);

        Assert.NotNull(result.WebTest);
        Assert.Equal("newwebtest", result.WebTest.ResourceName);
        Assert.Equal("eastus", result.WebTest.Location);
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
        Service.GetWebTest("sub1", "rg1", "newwebtest", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Resource not found"));

        // Act
        var response = await ExecuteCommandAsync(args);

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
        Service.GetWebTest("sub1", "rg1", "existingwebtest", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(existingWebTest);

        Service.UpdateWebTest(
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

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WebTestsCreateOrUpdateCommandResult);

        Assert.NotNull(result.WebTest);
        Assert.Equal("existingwebtest", result.WebTest.ResourceName);
        Assert.False(result.WebTest.IsEnabled);
        Assert.Equal(600, result.WebTest.FrequencyInSeconds);
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
        // Arrange & Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotEmpty(response.Message);
    }

    #endregion

    #region ExecuteAsync Tests - Error Handling

    #endregion
}
