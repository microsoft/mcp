// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.FirewallRule;

public class FirewallRuleListCommandTests : CommandUnitTestsBase<FirewallRuleListCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
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
            Service.ListFirewallRulesAsync(
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
    public async Task ExecuteAsync_ReturnsFirewallRulesSuccessfully()
    {
        // Arrange
        var firewallRules = new List<SqlServerFirewallRule>
        {
            new("AllowAllWindowsAzureIps", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server/firewallRules/AllowAllWindowsAzureIps",
                "Microsoft.Sql/servers/firewallRules", "0.0.0.0", "0.0.0.0"),
            new("ClientIPRule", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server/firewallRules/ClientIPRule",
                "Microsoft.Sql/servers/firewallRules", "192.168.1.1", "192.168.1.255")
        };

        Service.ListFirewallRulesAsync(
            "testserver",
            "testrg",
            "testsub",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(firewallRules);

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
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoFirewallRules()
    {
        // Arrange
        Service.ListFirewallRulesAsync(
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
        Service.ListFirewallRulesAsync(
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
        Service.ListFirewallRulesAsync(
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
        Service.ListFirewallRulesAsync(
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

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        const string serverName = "testserver";
        const string resourceGroup = "testrg";
        const string subscription = "testsub";

        Service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--server", serverName);

        // Assert
        await Service.Received(1).ListFirewallRulesAsync(
            serverName,
            resourceGroup,
            subscription,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicyOptions()
    {
        // Arrange
        var firewallRules = new List<SqlServerFirewallRule>
        {
            new("TestRule", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server/firewallRules/TestRule",
                "Microsoft.Sql/servers/firewallRules", "10.0.0.1", "10.0.0.10")
        };

        Service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(firewallRules);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--retry-max-retries", "3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with retry policy
        await Service.Received(1).ListFirewallRulesAsync(
            "testserver",
            "testrg",
            "testsub",
            Arg.Is<RetryPolicyOptions?>(r => r != null && r.MaxRetries == 3),
            Arg.Any<CancellationToken>());
    }
}
