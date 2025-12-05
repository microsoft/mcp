// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Device;

public class IoTHubDeviceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubDeviceService _service;
    private readonly ILogger<IoTHubDeviceListCommand> _logger;
    private readonly IoTHubDeviceListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubDeviceListCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        _logger = Substitute.For<ILogger<IoTHubDeviceListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubDeviceListCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ListDevices_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        var expectedDevices = new List<DeviceIdentity>
        {
            new DeviceIdentity("device1", "gen1", "aaaa==", "Connected", "Enabled", null, "2024-01-01T00:00:00Z", "2024-01-02T00:00:00Z", "2024-01-03T00:00:00Z", 0, "SAS", null),
            new DeviceIdentity("device2", "gen2", "bbbb==", "Connected", "Enabled", null, "2024-01-01T00:00:00Z", "2024-01-02T00:00:00Z", "2024-01-03T00:00:00Z", 0, "SAS", null)
        };

        _service.ListDevices(
            name,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDevices);

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
    public async Task ExecuteAsync_ListDevices_WithMaxCount_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var maxCount = 10;

        var expectedDevices = new List<DeviceIdentity>
        {
            new DeviceIdentity("device1", "gen1", "aaaa==", "Connected", "Enabled", null, "2024-01-01T00:00:00Z", "2024-01-02T00:00:00Z", "2024-01-03T00:00:00Z", 0, "SAS", null)
        };

        _service.ListDevices(
            name,
            resourceGroup,
            subscription,
            maxCount,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDevices);

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--max-count", maxCount.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Assert
        Assert.Equal("iothub-device-list", _command.Id);
        Assert.Equal("list", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Secret);
    }
}
