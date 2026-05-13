// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Server;

public class ServerParamGetCommandTests : CommandUnitTestsBase<ServerParamGetCommand, IMySqlService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsParameter_WhenSuccessful()
    {
        var expectedValue = "ON";
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "test-server", "max_connections", Arg.Any<CancellationToken>()).Returns(expectedValue);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "max_connections");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.ServerParamGetCommandResult);
        Assert.Equal("max_connections", result.Parameter);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "test-server", "invalid_param", Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Parameter 'invalid_param' not found."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "invalid_param");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Parameter 'invalid_param' not found", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }
}
