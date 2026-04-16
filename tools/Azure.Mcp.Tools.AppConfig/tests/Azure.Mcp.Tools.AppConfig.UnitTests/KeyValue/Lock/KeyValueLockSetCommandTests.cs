// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue.Lock;

public class KeyValueLockSetCommandTests : CommandUnitTestsBase<KeyValueLockSetCommand, IAppConfigService>
{
    [Fact]
    public async Task ExecuteAsync_LocksKeyValue_WhenValidParametersProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--lock");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            true,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null,
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.True(result.Locked);
    }

    [Fact]
    public async Task ExecuteAsync_LocksKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod",
            "--lock");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            true,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            "prod",
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
        Assert.True(result.Locked);
    }

    [Fact]
    public async Task ExecuteAsync_UnlocksKeyValue_WhenValidParametersProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            false,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null,
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.False(result.Locked);
    }

    [Fact]
    public async Task ExecuteAsync_UnlocksKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            false,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            "prod",
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException(bool locked)
    {
        // Arrange
        Service.SetKeyValueLockState(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Failed to lock key-value"));

        string[] argsToParse = locked
            ? ["--subscription", "sub123", "--account", "account1", "--key", "my-key", "--lock"]
            : ["--subscription", "sub123", "--account", "account1", "--key", "my-key"];

        // Act
        var response = await ExecuteCommandAsync(argsToParse);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to lock key-value", response.Message);
    }

    [Theory]
    [InlineData("")] // No parameters
    [InlineData("--subscription sub123")] // Missing account and key
    [InlineData("--subscription sub123 --account account1")] // Missing key
    [InlineData("--account account1 --key my-key")] // Missing subscription
    [InlineData("--subscription sub123 --key my-key")] // Missing account
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(string args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
