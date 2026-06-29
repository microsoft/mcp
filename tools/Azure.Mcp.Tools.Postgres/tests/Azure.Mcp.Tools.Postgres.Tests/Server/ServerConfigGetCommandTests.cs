// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Mcp.Core.TestUtilities;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Server;

public class ServerConfigGetCommandTests : SubscriptionCommandUnitTestsBase<ServerConfigGetCommand, IPostgresService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsConfig_WhenConfigExists()
    {
        var expectedConfig = "config123";
        Service.GetServerConfigAsync("sub123", "rg1", "user1", "server123", Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expectedConfig);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server123");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.ServerConfigGetCommandResult);
        Assert.Equal(expectedConfig, result.Configuration);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenConfigDoesNotExist()
    {
        Service.GetServerConfigAsync("sub123", "rg1", "user1", "server123", Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns("");

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server123");

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
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ("--user", "user1"),
            ("--server", "server123")
        ));

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsTenantAndRetryPolicy()
    {
        Service.GetServerConfigAsync("sub123", "rg1", "user1", "server123", Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns("config123");

        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server123",
            "--tenant", "tenant123",
            "--retry-max-retries", "3");

        await Service.Received(1).GetServerConfigAsync("sub123", "rg1", "user1", "server123", "tenant123", Arg.Is<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(p => p != null && p.MaxRetries == 3), Arg.Any<CancellationToken>());
    }
}
