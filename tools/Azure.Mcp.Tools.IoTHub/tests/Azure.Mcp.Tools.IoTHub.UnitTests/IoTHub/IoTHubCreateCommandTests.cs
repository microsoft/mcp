// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.IoTHub;

public class IoTHubCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubService _service;
    private readonly ILogger<IoTHubCreateCommand> _logger;
    private readonly IoTHubCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubCreateCommandTests()
    {
        _service = Substitute.For<IIoTHubService>();
        _logger = Substitute.For<ILogger<IoTHubCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubCreateCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_CreatesIoTHub()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var location = "eastus";
        var sku = "S1";
        var capacity = 1;
        var subscription = "sub-id";

        var expectedHub = new IoTHubDescription(
            "id", name, location, resourceGroup, subscription, sku, capacity, "Active", "test-hub.azure-devices.net");

        _service.CreateIoTHub(
            name,
            resourceGroup,
            location,
            sku,
            capacity,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedHub);

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--location", location,
            "--sku", sku,
            "--capacity", capacity.ToString(),
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }
}
