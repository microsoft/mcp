// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Server;

public class ServerParamGetCommandTests : CommandUnitTestsBase<ServerParamGetCommand, IPostgresService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsParamValue_WhenParamExists()
    {
        var expectedValue = "value123";
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", Arg.Any<CancellationToken>()).Returns(expectedValue);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server123",
            "--param", "param123");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.ServerParamGetCommandResult);
        Assert.Equal(expectedValue, result.ParameterValue);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenParamDoesNotExist()
    {
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", Arg.Any<CancellationToken>()).Returns("");
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server123",
            "--param", "param123");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    [InlineData("--server")]
    [InlineData("--param")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ("--user", "user1"),
            ("--server", "server123"),
            ("--param", "param123")
        ));

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }
}
