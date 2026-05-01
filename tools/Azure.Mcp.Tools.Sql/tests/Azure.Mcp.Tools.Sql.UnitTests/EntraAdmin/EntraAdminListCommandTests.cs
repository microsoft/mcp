// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.EntraAdmin;

public class EntraAdminListCommandTests : CommandUnitTestsBase<EntraAdminListCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server", true)]
    [InlineData("--subscription sub --resource-group rg", false)] // Missing server
    [InlineData("--subscription sub --server server", false)] // Missing resource group
    [InlineData("--resource-group rg --server server", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GetEntraAdministratorsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
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
    public async Task ExecuteAsync_ReturnsAdministratorsSuccessfully()
    {
        // Arrange
        var administrators = new List<SqlServerEntraAdministrator>
        {
            new("ActiveDirectory", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server/administrators/ActiveDirectory",
                "Microsoft.Sql/servers/administrators", "ActiveDirectory", "admin@domain.com", "12345678-1234-1234-1234-123456789012",
                "87654321-4321-4321-4321-210987654321", false)
        };

        Service.GetEntraAdministratorsAsync(
            "testserver",
            "testrg",
            "testsub",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(administrators);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoAdministrators()
    {
        // Arrange
        Service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404Error()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.NotFound, "Server not found");
        Service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL server not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Access denied");
        Service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
