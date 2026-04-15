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

public class KeyValueSetCommandTests : CommandUnitTestsBase<KeyValueSetCommand, IAppConfigService>
{
    [Fact]
    public async Task ExecuteAsync_SetsKeyValue_WhenValidParametersProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--value", "my-value");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValue(
            "account1",
            "my-key",
            "my-value",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null,
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("my-value", result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_SetsKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--value", "my-value",
            "--label", "prod");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValue(
            "account1",
            "my-key",
            "my-value",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            "prod",
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("my-value", result.Value);
        Assert.Equal("prod", result.Label);
    }

    [Fact]
    public async Task ExecuteAsync_SetsKeyValueWithContentTypeAndTagsProvided()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--value", "my-value",
            "--content-type", "application/json",
            "--tags", "environment=prod", "team=backend");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetKeyValue(
            "account1",
            "my-key",
            "my-value",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null,
            "application/json",
            Arg.Is<string[]>(tags => tags.Contains("environment=prod") && tags.Contains("team=backend")),
            Arg.Any<CancellationToken>());

        var result = ConvertResponse(response, AppConfigJsonContext.Default.KeyValueSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("my-value", result.Value);
        Assert.Equal("application/json", result.ContentType);
        Assert.NotNull(result.Tags);
        Assert.Contains("environment=prod", result.Tags);
        Assert.Contains("team=backend", result.Tags);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        Service.SetKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Failed to set key-value"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--value", "my-value");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to set key-value", response.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--subscription sub123")]
    [InlineData("--subscription sub123 --account account1")]
    [InlineData("--subscription sub123 --account account1 --key my-key")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(string args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
