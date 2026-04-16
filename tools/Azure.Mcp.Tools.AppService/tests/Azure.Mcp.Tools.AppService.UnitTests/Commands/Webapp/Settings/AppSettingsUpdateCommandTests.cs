// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Settings;

[Trait("Command", "AppSettingsUpdate")]
public class AppSettingsUpdateCommandTests : CommandUnitTestsBase<AppSettingsUpdateCommand, IAppServiceService>
{
    [Theory]
    [InlineData("add", "Setting1", "Value1", "Application setting 'Setting1' added successfully.")]
    [InlineData("add", "Setting1", "Value1", "Failed to add application setting 'Setting1' because it already exists.")]
    [InlineData("set", "Setting1", "Value1", "Application setting 'Setting1' set successfully.")]
    [InlineData("delete", "Setting1", null, "Application setting 'Setting1' deleted successfully.")]
    [InlineData("delete", "NonExistingSetting", null, "Application setting 'NonExistingSetting' doesn't exist, deletion is skipped.")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(
        string settingUpdateType,
        string settingName,
        string? settingValue,
        string expectedValue)
    {
        // Arrange
        // Set up the mock to return success for any arguments
        Service.UpdateAppSettingsAsync("sub123", "rg1", "test-app", settingName, settingUpdateType,
            settingValue, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        List<string> unparsedArgs = [
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--setting-name", settingName,
            "--setting-update-type", settingUpdateType
        ];
        if (settingValue != null)
        {
            unparsedArgs.AddRange(["--setting-value", settingValue]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).UpdateAppSettingsAsync("sub123", "rg1", "test-app", settingName,
            settingUpdateType, settingValue, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppServiceJsonContext.Default.AppSettingsUpdateResult);

        Assert.NotNull(result);
        Assert.Equal(expectedValue, result.UpdateStatus);
    }

    [Theory]
    [InlineData()] // Missing all parameters
    [InlineData("--subscription", "sub123")] // Missing resource group and app name
    [InlineData("--resource-group", "rg1")] // Missing subscription and app name
    [InlineData("--app", "app")] // Missing subscription and resource group
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app name
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app")] // Missing setting name and update type
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--setting-name", "Setting1")] // Missing update type
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--setting-update-type", "add")] // Missing setting name and setting value
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--setting-update-type", "set")] // Missing setting name and setting value
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--setting-update-type", "delete")] // Missing setting name
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--setting-update-type", "add", "--setting-name", "setting-name")] // Missing setting value
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--setting-update-type", "set", "--setting-name", "setting-name")] // Missing setting value
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(commandArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().UpdateAppSettingsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("add", "Setting1", "Value1")]
    [InlineData("set", "Setting1", "Value1")]
    [InlineData("delete", "Setting1", null)]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse(string settingUpdateType, string settingName, string? settingValue)
    {
        // Arrange
        Service.UpdateAppSettingsAsync("sub123", "rg1", "test-app", settingName, settingUpdateType,
            settingValue, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        List<string> unparsedArgs = [
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--setting-name", settingName,
            "--setting-update-type", settingUpdateType
        ];
        if (settingValue != null)
        {
            unparsedArgs.AddRange(["--setting-value", settingValue]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await Service.Received(1).UpdateAppSettingsAsync("sub123", "rg1", "test-app", settingName,
            settingUpdateType, settingValue, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
