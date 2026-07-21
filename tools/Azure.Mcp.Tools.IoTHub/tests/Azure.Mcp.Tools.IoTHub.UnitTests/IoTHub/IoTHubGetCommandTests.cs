// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.IoTHub;

public class IoTHubGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubService _service;
    private readonly ILogger<IoTHubGetCommand> _logger;
    private readonly IoTHubGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubGetCommandTests()
    {
        _service = Substitute.For<IIoTHubService>();
        _logger = Substitute.For<ILogger<IoTHubGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubGetCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_GetIoTHub()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        var expectedHubs = new List<IoTHubDescription> {
            new IoTHubDescription("id", name, "eastus", resourceGroup, subscription, "S1", 1, "Active", "test-hub.azure-devices.net")
        };

        _service.GetIoTHub(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedHubs);

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }
}
