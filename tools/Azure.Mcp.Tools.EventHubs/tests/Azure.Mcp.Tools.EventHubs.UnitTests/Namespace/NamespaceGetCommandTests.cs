// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.Namespace;

public class NamespaceGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<NamespaceGetCommand> _logger;
    private readonly NamespaceGetCommand _command;
    private readonly CommandContext _context;

    public NamespaceGetCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<NamespaceGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription", true)]
    [InlineData("--subscription test-subscription --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --namespace test-namespace --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --namespace test-namespace", false)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args);
        if (shouldSucceed)
        {
            // Set up appropriate service method based on arguments
            if (args.Contains(EventHubsOptionDefinitions.NamespaceName.Name) && args.Contains(OptionDefinitions.Common.ResourceGroup.Name))
            {
                // Single namespace request
                var namespaceDetails = new Models.Namespace(
                    "eh-namespace-prod-001",
                    "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-namespace-prod-001",
                    "rg-eventhubs-prod",
                    "East US",
                    new EventHubsNamespaceSku("Standard", "Standard", 1),
                    "Active",
                    "Succeeded",
                    DateTimeOffset.UtcNow.AddDays(-30),
                    DateTimeOffset.UtcNow.AddDays(-1),
                    "https://eh-namespace-prod-001.servicebus.windows.net:443/",
                    "12345678-1234-1234-1234-123456789012:eh-namespace-prod-001",
                    false,
                    null,
                    true,
                    true,
                    new Dictionary<string, string> { { "env", "prod" } });

                _eventHubsService.GetNamespaceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
                    .Returns(namespaceDetails);
            }
            else
            {
                // List request
                var namespaces = new List<Models.Namespace>
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

                _eventHubsService.GetNamespacesAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<RetryPolicyOptions?>())
                    .Returns(namespaces);
            }
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, (int)response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(200, (int)response.Status);
            Assert.NotNull(response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription 12345678-1234-1234-1234-123456789012 --resource-group rg-eventhubs-test");

        _eventHubsService.GetNamespacesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Resource Group 'rg-eventhubs-test' could not be found"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription unauthorized-sub --resource-group rg-eventhubs-prod");
        _eventHubsService.GetNamespacesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("The current user does not have access to subscription 'unauthorized-sub'"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }
}
