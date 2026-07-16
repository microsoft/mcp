// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Device;

public class IoTHubDeviceStatisticsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubDeviceService _service;
    private readonly ILogger<IoTHubDeviceStatisticsCommand> _logger;
    private readonly IoTHubDeviceStatisticsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubDeviceStatisticsCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        _logger = Substitute.For<ILogger<IoTHubDeviceStatisticsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubDeviceStatisticsCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_GetStatistics_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        var expected = new IoTHubRegistryStatistics(3, 7, 10);

        _service.GetDeviceStatistics(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

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

        await _service.Received(1).GetDeviceStatistics(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredOptions_ReturnsValidationError()
    {
        // Arrange - missing --name
        var args = _commandDefinition.Parse([
            "--resource-group", "test-rg",
            "--subscription", "sub-id"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        _service.GetDeviceStatistics(
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns<Task<IoTHubRegistryStatistics>>(_ => throw new Exception("service error"));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
