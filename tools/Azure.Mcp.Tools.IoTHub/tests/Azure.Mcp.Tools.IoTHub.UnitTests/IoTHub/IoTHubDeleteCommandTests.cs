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

public class IoTHubDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubService _service;
    private readonly ILogger<IoTHubDeleteCommand> _logger;
    private readonly IoTHubDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubDeleteCommandTests()
    {
        _service = Substitute.For<IIoTHubService>();
        _logger = Substitute.For<ILogger<IoTHubDeleteCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubDeleteCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_DeletesIoTHub()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        await _service.DeleteIoTHub(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

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

        _service.DeleteIoTHub(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

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
