// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.FirewallRule;

public class FirewallRuleDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<FirewallRuleDeleteCommand> _logger;
    private readonly FirewallRuleDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public FirewallRuleDeleteCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<FirewallRuleDeleteCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Deletes a firewall rule", command.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.True(_command.Metadata.Destructive);
        Assert.False(_command.Metadata.ReadOnly);
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
            _service.DeleteFirewallRuleAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
        _service.DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "TestRule",
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesIdempotentDelete_WhenRuleDoesNotExist()
    {
        // Arrange - Rule doesn't exist, but delete operation should still succeed (idempotent)
        _service.DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "NonExistentRule",
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name NonExistentRule");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404Error()
    {
        // Arrange
        var requestException = new Azure.RequestFailedException(404, "Server not found");
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("SQL server or firewall rule not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        // Arrange
        var requestException = new Azure.RequestFailedException(403, "Access denied");
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
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

        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse($"--subscription {subscription} --resource-group {resourceGroup} --server {serverName} --firewall-rule-name {ruleName}");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).DeleteFirewallRuleAsync(
            serverName,
            resourceGroup,
            subscription,
            ruleName,
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicyOptions()
    {
        // Arrange
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --retry-max-retries 3");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with retry policy
        await _service.Received(1).DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "TestRule",
            Arg.Is<Azure.Mcp.Core.Options.RetryPolicyOptions?>(r => r != null && r.MaxRetries == 3),
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
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse($"--subscription testsub --resource-group testrg --server testserver --firewall-rule-name {ruleName}");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with the correct rule name
        await _service.Received(1).DeleteFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            ruleName,
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        var argumentException = new ArgumentException("Invalid firewall rule name");
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(argumentException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name InvalidRule");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid firewall rule name", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesResultContainsExpectedData()
    {
        // Arrange
        const string ruleName = "TestRule";
        _service.DeleteFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse($"--subscription testsub --resource-group testrg --server testserver --firewall-rule-name {ruleName}");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }
}
