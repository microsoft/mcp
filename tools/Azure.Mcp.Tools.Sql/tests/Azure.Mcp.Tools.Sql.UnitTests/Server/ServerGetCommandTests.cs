// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerGetCommandTests : CommandUnitTestsBase<ServerGetCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithServerName_ReturnsSingleServer()
    {
        // Arrange
        var mockServer = CreateMockServer("server1");

        Service.GetServerAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockServer);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
        await Service.Received(1).GetServerAsync("server1", "rg", "sub", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceive().ListServersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutServerName_ReturnsAllServers()
    {
        // Arrange
        var mockServers = new List<SqlServer> { CreateMockServer("server1"), CreateMockServer("server2") };

        Service.ListServersAsync(
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockServers);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub", "--resource-group", "rg");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
        await Service.Received(1).ListServersAsync("rg", "sub", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceive().GetServerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub", "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Server not found");
        Service.GetServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var authException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden");
        Service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(authException);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub", "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub", false)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("--subscription sub --resource-group rg --server server1", true)]
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service
                .ListServersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
            Service
                .GetServerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(CreateMockServer("server1"));
        }

        // Act
        var response = await ExecuteCommandAsync(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        if (shouldSucceed)
            Assert.Equal(HttpStatusCode.OK, response.Status);
        else
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    private static SqlServer CreateMockServer(string name) => new(
        Name: name,
        FullyQualifiedDomainName: $"{name}.database.windows.net",
        Location: "East US",
        ResourceGroup: "rg",
        Subscription: "sub",
        AdministratorLogin: "adminuser",
        Version: "12.0",
        State: "Ready",
        PublicNetworkAccess: "Enabled",
        Tags: null
    );
}
