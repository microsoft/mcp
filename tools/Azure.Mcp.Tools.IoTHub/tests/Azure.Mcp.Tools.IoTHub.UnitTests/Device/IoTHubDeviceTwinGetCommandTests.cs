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

public class IoTHubDeviceTwinGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubDeviceService _service;
    private readonly ILogger<IoTHubDeviceTwinGetCommand> _logger;
    private readonly IoTHubDeviceTwinGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubDeviceTwinGetCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        _logger = Substitute.For<ILogger<IoTHubDeviceTwinGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubDeviceTwinGetCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_GetDeviceTwin_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var deviceId = "test-device";

        var expectedTwin = new DeviceTwin(
            deviceId,
            "etag123",
            "deviceEtag123",
            "Enabled",
            "2024-01-01T00:00:00Z",
            "Connected",
            "2024-01-03T00:00:00Z",
            0,
            "SAS",
            1,
            new TwinProperties(new Dictionary<string, object>(), new Dictionary<string, object>()),
            new DeviceCapabilities(false),
            new Dictionary<string, object>());

        _service.GetDeviceTwin(
            deviceId,
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTwin);

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--device-id", deviceId
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
        Assert.Equal("iothub-device-twin-get", _command.Id);
        Assert.Equal("get-twin", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Secret);
    }
}
