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

public class IoTHubKeysGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubService _service;
    private readonly ILogger<IoTHubKeysGetCommand> _logger;
    private readonly IoTHubKeysGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubKeysGetCommandTests()
    {
        _service = Substitute.For<IIoTHubService>();
        _logger = Substitute.For<ILogger<IoTHubKeysGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubKeysGetCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_GetsIoTHubKeys()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        var expectedKeys = new List<IoTHubKey>
        {
            new("iothubowner", "primary-key-value", "secondary-key-value", "RegistryWrite, ServiceConnect, DeviceConnect"),
            new("service", "service-primary-key", "service-secondary-key", "ServiceConnect")
        };

        _service.GetIoTHubKeys(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedKeys);

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

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        var expectedKeys = new List<IoTHubKey>
        {
            new("iothubowner", "primary-key-value", "secondary-key-value", "RegistryWrite, ServiceConnect, DeviceConnect"),
            new("service", "service-primary-key", "service-secondary-key", "ServiceConnect")
        };

        _service.GetIoTHubKeys(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedKeys);

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
        var responseJson = JsonSerializer.Serialize(response.Results);
        Assert.NotNull(responseJson);
    }
}
