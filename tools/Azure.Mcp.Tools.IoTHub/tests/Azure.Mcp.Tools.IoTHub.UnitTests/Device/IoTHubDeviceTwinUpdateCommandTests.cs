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

public class IoTHubDeviceTwinUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubDeviceService _service;
    private readonly ILogger<IoTHubDeviceTwinUpdateCommand> _logger;
    private readonly IoTHubDeviceTwinUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubDeviceTwinUpdateCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        _logger = Substitute.For<ILogger<IoTHubDeviceTwinUpdateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubDeviceTwinUpdateCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDeviceTwin_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var deviceId = "test-device";
        var patch = "{\"properties\":{\"desired\":{\"temperature\":72}}}";

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
            new TwinProperties(new Dictionary<string, object> { ["temperature"] = 72 }, new Dictionary<string, object>()),
            new DeviceCapabilities(false),
            new Dictionary<string, object>());

        _service.UpdateDeviceTwin(
            deviceId,
            Arg.Any<TwinPatch>(),
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
            "--device-id", deviceId,
            "--patch", patch
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
        Assert.Equal("iothub-device-twin-update", _command.Id);
        Assert.Equal("update-twin", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.False(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Secret);
        Assert.False(_command.Metadata.Idempotent);
    }
}
