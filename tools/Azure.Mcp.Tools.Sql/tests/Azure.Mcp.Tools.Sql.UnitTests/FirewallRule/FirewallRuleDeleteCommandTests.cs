// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.FirewallRule;

public class FirewallRuleDeleteCommandTests : CommandUnitTestsBase<FirewallRuleDeleteCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server --firewall-rule-name rule1", true)]
    [InlineData("--subscription sub --resource-group rg --server server", false)] // Missing rule name
    [InlineData("--subscription sub --resource-group rg --firewall-rule-name rule1", false)] // Missing server
    [InlineData("--subscription sub --server server --firewall-rule-name rule1", false)] // Missing resource group
    [InlineData("--resource-group rg --server server --firewall-rule-name rule1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.DeleteFirewallRuleAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
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
    public async Task ExecuteAsync_DeletesFirewallRuleSuccessfully()
    {
        // Arrange
        Service.DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "TestRule",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--firewall-rule-name", "TestRule");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesIdempotentDelete_WhenRuleDoesNotExist()
    {
        // Arrange - Rule doesn't exist, but delete operation should still succeed (idempotent)
        Service.DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "NonExistentRule",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--firewall-rule-name", "NonExistentRule");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
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
            "--server", "testserver",
            "--firewall-rule-name", "TestRule");

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
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
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
            "--server", "testserver",
            "--firewall-rule-name", "TestRule");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL server or firewall rule not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Access denied");
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
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
            "--server", "testserver",
            "--firewall-rule-name", "TestRule");

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
        const string ruleName = "TestRule";

        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--server", serverName,
            "--firewall-rule-name", ruleName);

        // Assert
        await Service.Received(1).DeleteFirewallRuleAsync(
            serverName,
            resourceGroup,
            subscription,
            ruleName,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicyOptions()
    {
        // Arrange
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--firewall-rule-name", "TestRule",
            "--retry-max-retries", "3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with retry policy
        await Service.Received(1).DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "TestRule",
            Arg.Is<RetryPolicyOptions?>(r => r != null && r.MaxRetries == 3),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("TestRule")]
    [InlineData("MyFirewallRule")]
    [InlineData("Rule-With-Hyphens")]
    [InlineData("Rule_With_Underscores")]
    [InlineData("Rule123")]
    public async Task ExecuteAsync_HandlesVariousRuleNames(string ruleName)
    {
        // Arrange
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--firewall-rule-name", ruleName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with the correct rule name
        await Service.Received(1).DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            ruleName,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        var argumentException = new ArgumentException("Invalid firewall rule name");
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(argumentException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--firewall-rule-name", "InvalidRule");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid firewall rule name", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesResultContainsExpectedData()
    {
        // Arrange
        const string ruleName = "TestRule";
        Service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--firewall-rule-name", ruleName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
