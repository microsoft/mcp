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

public class ServerParamSetCommandTests : CommandUnitTestsBase<ServerParamSetCommand, IMySqlService>
{
    [Fact]
    public async Task ExecuteAsync_SetsParameter_WhenSuccessful()
    {
        var newValue = "100";
        Service.SetServerParameterAsync("sub123", "rg1", "user1", "test-server", "max_connections", newValue, Arg.Any<CancellationToken>()).Returns(newValue);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "max_connections",
            "--value", newValue);

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.ServerParamSetCommandResult);
        Assert.Equal("max_connections", result.Parameter);
        Assert.Equal(newValue, result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        Service.SetServerParameterAsync("sub123", "rg1", "user1", "test-server", "invalid_param", "100", Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Parameter 'invalid_param' not found."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "invalid_param",
            "--value", "100");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Parameter 'invalid_param' not found", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
    }
}
