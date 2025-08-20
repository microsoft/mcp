// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.FirewallRule;

public class FirewallRuleListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<FirewallRuleListCommand> _logger;
    private readonly FirewallRuleListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public FirewallRuleListCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<FirewallRuleListCommand>>();

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
            _service.ListFirewallRulesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<SqlServerFirewallRule>());
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

        _service.ListFirewallRulesAsync(
            "testserver",
            "testrg",
            "testsub",
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(firewallRules);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoFirewallRules()
    {
        // Arrange
        _service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<SqlServerFirewallRule>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServerFirewallRule>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver");

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
        _service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServerFirewallRule>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("SQL server not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        // Arrange
        var requestException = new Azure.RequestFailedException(403, "Access denied");
        _service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServerFirewallRule>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver");

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

        _service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<SqlServerFirewallRule>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse($"--subscription {subscription} --resource-group {resourceGroup} --server {serverName}");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).ListFirewallRulesAsync(
            serverName,
            resourceGroup,
            subscription,
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
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

        _service.ListFirewallRulesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(firewallRules);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _parser.Parse("--subscription testsub --resource-group testrg --server testserver --retry-max-retries 3");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with retry policy
        await _service.Received(1).ListFirewallRulesAsync(
            "testserver",
            "testrg",
            "testsub",
            Arg.Is<Azure.Mcp.Core.Options.RetryPolicyOptions?>(r => r != null && r.MaxRetries == 3),
            Arg.Any<CancellationToken>());
    }
}
