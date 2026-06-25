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

public class ServerParamGetCommandTests : SubscriptionCommandUnitTestsBase<ServerParamGetCommand, IPostgresService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsParamValue_WhenParamExists()
    {
        var expectedValue = "value123";
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expectedValue);

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
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns("");
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
        Assert.Contains($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsTenantAndRetryPolicy()
    {
        Service.GetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns("value123");

        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server123",
            "--param", "param123",
            "--tenant", "tenant123",
            "--retry-max-retries", "3");

        await Service.Received(1).GetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", "tenant123", Arg.Is<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(p => p != null && p.MaxRetries == 3), Arg.Any<CancellationToken>());
    }
}
