// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.Namespace;

public class NamespaceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<NamespaceListCommand> _logger;
    private readonly NamespaceListCommand _command;
    private readonly CommandContext _context;

    public NamespaceListCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<NamespaceListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-sub", false)]
    [InlineData("--subscription 00000000-0000-0000-0000-000000000000 --resource-group test-rg", true)]
    [InlineData("--subscription production-subscription --resource-group rg-eventhubs-prod", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args);
        if (shouldSucceed)
        {
            var namespaces = new List<EventHubsNamespaceInfo>
            {
                new("eh-namespace-prod-001",
                    "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-namespace-prod-001",
                    "rg-eventhubs-prod"),
                new("eh-namespace-prod-002",
                    "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-namespace-prod-002",
                    "rg-eventhubs-prod"),
                new("eh-shared-services",
                    "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-shared-services",
                    "rg-eventhubs-prod")
            };

            _eventHubsService.ListNamespacesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(namespaces);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(200, response.Status);
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription 12345678-1234-1234-1234-123456789012 --resource-group rg-eventhubs-test");

        _eventHubsService.ListNamespacesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Resource Group 'rg-eventhubs-test' could not be found"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("Resource Group 'rg-eventhubs-test' could not be found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription unauthorized-sub --resource-group rg-eventhubs-prod");
        _eventHubsService.ListNamespacesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("The current user does not have access to subscription 'unauthorized-sub'"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("does not have access", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoNamespacesExist()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription 12345678-1234-1234-1234-123456789012 --resource-group rg-empty");
        _eventHubsService.ListNamespacesAsync("rg-empty", "12345678-1234-1234-1234-123456789012", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(new List<EventHubsNamespaceInfo>());

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results); // Should be null when no namespaces found

        await _eventHubsService.Received(1).ListNamespacesAsync("rg-empty", "12345678-1234-1234-1234-123456789012", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedNamespaces()
    {
        // Arrange
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var resourceGroup = "rg-eventhubs-production";
        var parseResult = _command.GetCommand().Parse($"--subscription {subscriptionId} --resource-group {resourceGroup}");

        var expectedNamespaces = new List<EventHubsNamespaceInfo>
        {
            new("eh-prod-eastus-001",
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.EventHub/namespaces/eh-prod-eastus-001",
                resourceGroup),
            new("eh-prod-westus-001",
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.EventHub/namespaces/eh-prod-westus-001",
                resourceGroup)
        };

        _eventHubsService.ListNamespacesAsync(resourceGroup, subscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedNamespaces);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify response structure
        Assert.NotNull(response.Results);

        // Verify service was called correctly
        await _eventHubsService.Received(1).ListNamespacesAsync(resourceGroup, subscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }
}
