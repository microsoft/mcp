// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue;

public class KeyValueDeleteCommandTests : CommandUnitTestsBase<KeyValueDeleteCommand, IAppConfigService>
{
    [Fact]
    public async Task ExecuteAsync_DeletesKeyValue_WhenValidParametersProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteKeyValue(
            "account1",
            "my-key",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null,
            Arg.Any<CancellationToken>());

        var result = DeserializeResponse(response, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteKeyValue(
            "account1",
            "my-key",
            "sub123", null,
            Arg.Any<RetryPolicyOptions>(),
            "prod",
            Arg.Any<CancellationToken>());

        var result = DeserializeResponse(response, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
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
