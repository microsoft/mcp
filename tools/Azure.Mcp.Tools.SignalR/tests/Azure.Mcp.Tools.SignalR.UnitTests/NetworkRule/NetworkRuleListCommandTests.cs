// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Commands.NetworkRule;
using Azure.Mcp.Tools.SignalR.Models;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.NetworkRule;

public class NetworkRuleListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<NetworkRuleListCommand> _logger;
    private readonly NetworkRuleListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public NetworkRuleListCommandTests()
    {
        _signalRService = Substitute.For<ISignalRService>();
        _logger = Substitute.For<ILogger<NetworkRuleListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_signalRService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("network access control rules", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --signalr signalr1", true)]
    [InlineData("--subscription sub1 --signalr signalr1", false)] // Missing resource-group
    [InlineData("--subscription sub1 --resource-group rg1", false)] // Missing signalr
    [InlineData("--resource-group rg1 --signalr signalr1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testNetworkRules = new Models.NetworkRule
            {
                DefaultAction = "Deny",
                PublicNetwork =
                    new NetworkAcl
                    {
                        Allow = new[] { "ServerConnection", "ClientConnection" },
                        Deny = new[] { "RESTAPI" }
                    },
                PrivateEndpoints = new[]
                {
                    new PrivateEndpointNetworkAcl { Name = "test-endpoint", Allow = new[] { "ServerConnection" } }
                }
            };

            _signalRService.GetNetworkRulesAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<AuthMethod?>(),
                    Arg.Any<RetryPolicyOptions>())
                .Returns(testNetworkRules);
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNetworkRulesWhenFound()
    {
        // Arrange
        var expectedNetworkRules = new Models.NetworkRule
        {
            DefaultAction = "Allow",
            PublicNetwork =
                new NetworkAcl
                {
                    Allow = new[] { "ServerConnection", "ClientConnection", "RESTAPI" },
                    Deny = new[] { "Trace" }
                },
            PrivateEndpoints = new[]
            {
                new PrivateEndpointNetworkAcl
                {
                    Name = "test-private-endpoint",
                    Allow = new[] { "ServerConnection", "ClientConnection" },
                    Deny = new[] { "RESTAPI" }
                }
            }
        };

        _signalRService.GetNetworkRulesAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedNetworkRules);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        var json = JsonSerializer.Serialize(response.Results);
        var result =
            JsonSerializer.Deserialize<NetworkRuleListCommand.NetworkRuleListCommandResult>(json,
                Commands.SignalRJsonContext.Default.NetworkRuleListCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Allow",
            result.NetworkRules.DefaultAction);
        Assert.NotNull(
            result.NetworkRules.PublicNetwork);
        Assert.Contains("ServerConnection",
            result.NetworkRules.PublicNetwork.Allow!);
        Assert.NotNull(
            result.NetworkRules.PrivateEndpoints);
        Assert.Single(
            result.NetworkRules.PrivateEndpoints);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenNetworkRulesNotFound()
    {
        // Arrange
        _signalRService.GetNetworkRulesAsync(
                "test-subscription",
                "test-rg",
                "nonexistent-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns((Models.NetworkRule?)null);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr",
            "nonexistent-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException()
    {
        // Arrange
        var exception = new RequestFailedException(403, "Insufficient permissions to access network rules");
        _signalRService.GetNetworkRulesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.NetworkRule?>(exception));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Insufficient permissions to access network rules", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _signalRService.GetNetworkRulesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.NetworkRule?>(new Exception("Network configuration error")));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Network configuration error", response.Message);
    }

    [Theory]
    [InlineData("Allow")]
    [InlineData("Deny")]
    public async Task ExecuteAsync_HandlesAllDefaultActions(string defaultAction)
    {
        // Arrange
        var networkRules = new Models.NetworkRule { DefaultAction = defaultAction };

        _signalRService.GetNetworkRulesAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(networkRules);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result =
            JsonSerializer.Deserialize<NetworkRuleListCommand.NetworkRuleListCommandResult>(json,
                Commands.SignalRJsonContext.Default.NetworkRuleListCommandResult);
        Assert.NotNull(result);
        var rules = Assert.IsType<Models.NetworkRule>(result.NetworkRules);
        Assert.Equal(defaultAction, rules.DefaultAction);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyNetworkRules()
    {
        // Arrange
        var emptyNetworkRules = new Models.NetworkRule
        {
            DefaultAction = "Allow",
            PublicNetwork = null,
            PrivateEndpoints = Array.Empty<PrivateEndpointNetworkAcl>()
        };

        _signalRService.GetNetworkRulesAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(emptyNetworkRules);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }
}
