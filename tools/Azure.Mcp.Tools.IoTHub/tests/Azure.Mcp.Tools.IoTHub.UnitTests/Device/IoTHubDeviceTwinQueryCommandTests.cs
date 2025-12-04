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

public class IoTHubDeviceTwinQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubDeviceService _service;
    private readonly ILogger<IoTHubDeviceTwinQueryCommand> _logger;
    private readonly IoTHubDeviceTwinQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubDeviceTwinQueryCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        _logger = Substitute.For<ILogger<IoTHubDeviceTwinQueryCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubDeviceTwinQueryCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_QueryTwins_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices WHERE properties.reported.temperature > 70";

        var expectedTwins = new List<DeviceTwin>
        {
            new DeviceTwin(
                "device1",
                "etag123",
                "deviceEtag123",
                "Enabled",
                "2024-01-01T00:00:00Z",
                "Connected",
                "2024-01-03T00:00:00Z",
                0,
                "SAS",
                1,
                new TwinProperties(new Dictionary<string, object>(), new Dictionary<string, object> { ["temperature"] = 75 }),
                new DeviceCapabilities(false),
                new Dictionary<string, object>())
        };

        _service.QueryTwins(
            query,
            name,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTwins);

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query
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
        Assert.Equal("iothub-device-twin-query", _command.Id);
        Assert.Equal("query-twins", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Secret);
    }
}
