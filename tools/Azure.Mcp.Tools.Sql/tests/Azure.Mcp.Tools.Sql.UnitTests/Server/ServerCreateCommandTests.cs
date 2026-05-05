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

public class ServerCreateCommandTests : CommandUnitTestsBase<ServerCreateCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
        Assert.True(command.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!", true)]
    [InlineData("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin", false)] // Missing password
    [InlineData("--subscription sub --resource-group rg --server testserver --location eastus --administrator-password Password123!", false)] // Missing login
    [InlineData("--subscription sub --resource-group rg --server testserver --administrator-login admin --administrator-password Password123!", false)] // Missing location
    [InlineData("--subscription sub --resource-group rg --location eastus --administrator-login admin --administrator-password Password123!", false)] // Missing server
    [InlineData("--subscription sub --server testserver --location eastus --administrator-login admin --administrator-password Password123!", false)] // Missing resource group
    [InlineData("--resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedServer = new SqlServer(
                Name: "testserver",
                FullyQualifiedDomainName: "testserver.database.windows.net",
                Location: "East US",
                ResourceGroup: "rg",
                Subscription: "sub",
                AdministratorLogin: "admin",
                Version: "12.0",
                State: "Ready",
                PublicNetworkAccess: "Enabled",
                Tags: []
            );

            Service.CreateServerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedServer);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesServerSuccessfully()
    {
        // Arrange
        var expectedServer = new SqlServer(
            Name: "testserver",
            FullyQualifiedDomainName: "testserver.database.windows.net",
            Location: "East US",
            ResourceGroup: "rg",
            Subscription: "sub",
            AdministratorLogin: "admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Enabled",
            Tags: []
        );

        Service.CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "testserver",
            "--location", "eastus",
            "--administrator-login", "admin",
            "--administrator-password", "Password123!");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_PassesAllParameters()
    {
        // Arrange
        var expectedServer = new SqlServer(
            Name: "testserver",
            FullyQualifiedDomainName: "testserver.database.windows.net",
            Location: "East US",
            ResourceGroup: "rg",
            Subscription: "sub",
            AdministratorLogin: "admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Disabled",
            Tags: []
        );

        Service.CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            "12.0",
            "Disabled",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "testserver",
            "--location", "eastus",
            "--administrator-login", "admin",
            "--administrator-password", "Password123!",
            "--version", "12.0",
            "--public-network-access", "Disabled");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);

        await Service.Received(1).CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            "12.0",
            "Disabled",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutPublicNetworkAccess_PassesNullToService()
    {
        // Arrange
        // The service layer defaults publicNetworkAccess to Disabled when null is passed
        var expectedServer = new SqlServer(
            Name: "testserver",
            FullyQualifiedDomainName: "testserver.database.windows.net",
            Location: "East US",
            ResourceGroup: "rg",
            Subscription: "sub",
            AdministratorLogin: "admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Disabled",
            Tags: []
        );

        Service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "testserver",
            "--location", "eastus",
            "--administrator-login", "admin",
            "--administrator-password", "Password123!");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            Arg.Is<string?>(v => v == null), // version not specified
            Arg.Is<string?>(p => p == null), // publicNetworkAccess not specified; service defaults to Disabled
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPublicNetworkAccess_ReturnsBadRequest()
    {
        // Arrange
        Service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid value 'Enabeld' for public-network-access. Allowed values are 'Enabled' or 'Disabled'.", "publicNetworkAccess"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "testserver",
            "--location", "eastus",
            "--administrator-login", "admin",
            "--administrator-password", "Password123!",
            "--public-network-access", "Enabeld");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid parameter", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        Service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "testserver",
            "--location", "eastus",
            "--administrator-login", "admin",
            "--administrator-password", "Password123!");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("error", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerAlreadyExists_Returns409StatusCode()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Conflict, "Conflict: Server already exists");

        Service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "testserver",
            "--location", "eastus",
            "--administrator-login", "admin",
            "--administrator-password", "Password123!");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("server with this name already exists", response.Message.ToLower());
    }
}
