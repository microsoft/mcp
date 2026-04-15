// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue;

public class KeyValueGetCommandTests : CommandUnitTestsBase<KeyValueGetCommand, IAppConfigService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSettingsList_WhenSettingsExist()
    {
        // Arrange
        var expectedSettings = new List<KeyValueSetting>
        {
            new() { Key = "key1", Value = "value1", Label = "prod" },
            new() { Key = "key2", Value = "value2", Label = "dev" }
        };
        Service.GetKeyValues(
          "account1",
          "sub123",
          Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
          Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
          Arg.Any<string?>(),
          Arg.Any<string?>(),
          Arg.Any<string?>(),
          Arg.Any<RetryPolicyOptions>(),
          Arg.Any<CancellationToken>())
          .Returns(expectedSettings);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--account", "account1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Settings.Count);
        Assert.Equal("key1", result.Settings[0].Key);
        Assert.Equal("key2", result.Settings[1].Key);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredSettingsList_WhenKeyFilterProvided()
    {
        // Arrange
        var expectedSettings = new List<KeyValueSetting>
        {
            new() { Key = "key1", Value = "value1", Label = "prod" }
        };
        Service.GetKeyValues(
          "account1",
          "sub123",
          Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
          Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
          "key1",
          Arg.Any<string?>(),
          Arg.Any<string?>(),
          Arg.Any<RetryPolicyOptions>(),
          Arg.Any<CancellationToken>())
          .Returns(expectedSettings);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key-filter", "key1");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Settings);
        Assert.Equal("key1", result.Settings[0].Key);
        Assert.Equal("value1", result.Settings[0].Value);
        Assert.Equal("prod", result.Settings[0].Label);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredSettingsList_WhenLabelFilterProvided()
    {
        // Arrange
        var expectedSettings = new List<KeyValueSetting>
        {
            new() { Key = "key1", Value = "value1", Label = "prod" }
        };
        Service.GetKeyValues(
          "account1",
          "sub123",
          Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
          Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
          Arg.Any<string?>(),
          "prod",
          Arg.Any<string?>(),
          Arg.Any<RetryPolicyOptions>(),
          Arg.Any<CancellationToken>())
          .Returns(expectedSettings);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--label-filter", "prod");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Settings);
        Assert.Equal("key1", result.Settings[0].Key);
        Assert.Equal("value1", result.Settings[0].Value);
        Assert.Equal("prod", result.Settings[0].Label);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSettingGet_WhenSettingExists()
    {
        // Arrange
        var expectedSetting = new KeyValueSetting
        {
            Key = "my-key",
            Value = "my-value",
            Label = "prod",
            ContentType = "text/plain",
            Locked = false
        };
        Service.GetKeyValues(
            "account1",
            "sub123",
            "my-key",
            "prod",
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([expectedSetting]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueGetCommandResult);
        Assert.NotNull(result);
        Assert.Single(result.Settings);
        Assert.Equal("my-key", result.Settings[0].Key);
        Assert.Equal("my-value", result.Settings[0].Value);
        Assert.Equal("prod", result.Settings[0].Label);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSettingGet_WhenNoLabelProvided()
    {
        // Arrange
        var expectedSetting = new KeyValueSetting
        {
            Key = "my-key",
            Value = "my-value",
            Label = "",
            ContentType = "text/plain",
            Locked = false
        };
        Service.GetKeyValues(
            "account1",
            "sub123",
            "my-key",
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([expectedSetting]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Settings);
        Assert.Equal("my-key", result.Settings[0].Key);
        Assert.Equal("my-value", result.Settings[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        Service.GetKeyValues(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Setting not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Setting not found", response.Message);
    }

    [Theory]
    [InlineData("--account", "account1")] // Missing subscription
    [InlineData("--subscription", "sub123")] // Missing account
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenKeyAndKeyFilterAreSpecified()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "key1",
            "--key-filter", "keyfilter");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Cannot specify both --key and --key-filter options together", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
