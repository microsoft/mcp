// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.FirewallRule;

public class FirewallRuleCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<FirewallRuleCreateCommand> _logger;
    private readonly FirewallRuleCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FirewallRuleCreateCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<FirewallRuleCreateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Creates a firewall rule", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server --firewall-rule-name rule1 --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255", true)]
    [InlineData("--subscription sub --resource-group rg --server server --firewall-rule-name rule1 --start-ip-address 192.168.1.1", false)] // Missing end IP
    [InlineData("--subscription sub --resource-group rg --server server --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255", false)] // Missing rule name
    [InlineData("--subscription sub --resource-group rg --firewall-rule-name rule1 --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255", false)] // Missing server
    [InlineData("--subscription sub --server server --firewall-rule-name rule1 --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255", false)] // Missing resource group
    [InlineData("--resource-group rg --server server --firewall-rule-name rule1 --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedFirewallRule = new SqlServerFirewallRule(
                "rule1",
                "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server/firewallRules/rule1",
                "Microsoft.Sql/servers/firewallRules",
                "192.168.1.1",
                "192.168.1.255");

            _service.CreateFirewallRuleAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedFirewallRule);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse(args);

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
    public async Task ExecuteAsync_CreatesFirewallRuleSuccessfully()
    {
        // Arrange
        var expectedFirewallRule = new SqlServerFirewallRule(
            "TestRule",
            "/subscriptions/testsub/resourceGroups/testrg/providers/Microsoft.Sql/servers/testserver/firewallRules/TestRule",
            "Microsoft.Sql/servers/firewallRules",
            "192.168.1.1",
            "192.168.1.255");

        _service.CreateFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "TestRule",
            "192.168.1.1",
            "192.168.1.255",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedFirewallRule);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255");

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
        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServerFirewallRule>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255");

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
        var requestException = new RequestFailedException(404, "Server not found");
        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServerFirewallRule>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255");

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
        var requestException = new RequestFailedException(403, "Access denied");
        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServerFirewallRule>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles409Error()
    {
        // Arrange
        var requestException = new RequestFailedException(409, "Conflict - rule already exists");
        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServerFirewallRule>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(409, response.Status);
        Assert.Contains("firewall rule with this name already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        var argumentException = new ArgumentException("Invalid IP address format");
        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServerFirewallRule>(argumentException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address invalid --end-ip-address invalid");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid IP address format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        const string serverName = "testserver";
        const string resourceGroup = "testrg";
        const string subscription = "testsub";
        const string ruleName = "TestRule";
        const string startIp = "192.168.1.1";
        const string endIp = "192.168.1.255";

        var expectedFirewallRule = new SqlServerFirewallRule(
            ruleName,
            $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/firewallRules/{ruleName}",
            "Microsoft.Sql/servers/firewallRules",
            startIp,
            endIp);

        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedFirewallRule);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse($"--subscription {subscription} --resource-group {resourceGroup} --server {serverName} --firewall-rule-name {ruleName} --start-ip-address {startIp} --end-ip-address {endIp}");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).CreateFirewallRuleAsync(
            serverName,
            resourceGroup,
            subscription,
            ruleName,
            startIp,
            endIp,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicyOptions()
    {
        // Arrange
        var expectedFirewallRule = new SqlServerFirewallRule(
            "TestRule",
            "/subscriptions/testsub/resourceGroups/testrg/providers/Microsoft.Sql/servers/testserver/firewallRules/TestRule",
            "Microsoft.Sql/servers/firewallRules",
            "192.168.1.1",
            "192.168.1.255");

        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedFirewallRule);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address 192.168.1.1 --end-ip-address 192.168.1.255 --retry-max-retries 3");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the service was called with retry policy
        await _service.Received(1).CreateFirewallRuleAsync(
            "testserver",
            "testrg",
            "testsub",
            "TestRule",
            "192.168.1.1",
            "192.168.1.255",
            Arg.Is<RetryPolicyOptions?>(r => r != null && r.MaxRetries == 3),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("10.0.0.1", "10.0.0.1")] // Single IP
    [InlineData("192.168.1.1", "192.168.1.255")] // IP range
    [InlineData("0.0.0.0", "255.255.255.255")] // Full range
    public async Task ExecuteAsync_HandlesVariousIPFormats(string startIp, string endIp)
    {
        // Arrange
        var expectedFirewallRule = new SqlServerFirewallRule(
            "TestRule",
            "/subscriptions/testsub/resourceGroups/testrg/providers/Microsoft.Sql/servers/testserver/firewallRules/TestRule",
            "Microsoft.Sql/servers/firewallRules",
            startIp,
            endIp);

        _service.CreateFirewallRuleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedFirewallRule);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse($"--subscription testsub --resource-group testrg --server testserver --firewall-rule-name TestRule --start-ip-address {startIp} --end-ip-address {endIp}");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }
}
