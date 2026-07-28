// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.Tests.KeyValue;

public class KeyValueDeleteCommandTests : SubscriptionCommandUnitTestsBase<KeyValueDeleteCommand, IAppConfigService>
{
    [Fact]
    public async Task ExecuteAsync_DeletesKeyValue_WhenValidParametersProvided()
    {
        // Arrange
        Service.DeleteKeyValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key");

        // Assert
        await Service.Received(1).DeleteKeyValue(
            "account1",
            "my-key",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null,
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.Equal("my-key", result.Key);
        Assert.True(result.Existed);
        Assert.Equal("Key 'my-key' deleted successfully.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange
        Service.DeleteKeyValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod");

        // Assert
        await Service.Received(1).DeleteKeyValue(
            "account1",
            "my-key",
            "sub123", null,
            Arg.Any<RetryPolicyOptions>(),
            "prod",
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
        Assert.True(result.Existed);
        Assert.Equal("Key 'my-key' with label 'prod' deleted successfully.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExistedFalse_WhenKeyWasAbsent()
    {
        // Arrange — service returns false indicating the key did not exist
        Service.DeleteKeyValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "nonexistent-key");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.Equal("nonexistent-key", result.Key);
        Assert.False(result.Existed);
        Assert.Equal("Key 'nonexistent-key' did not exist in store 'account1'.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        Service.DeleteKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Failed to delete key-value"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to delete key-value", response.Message);
    }

    [Theory]
    [InlineData("--account", "account1", "--key", "my-key")] // Missing subscription
    [InlineData("--subscription", "sub123", "--key", "my-key")] // Missing account
    [InlineData("--subscription", "sub123", "--account", "account1")] // Missing key
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
