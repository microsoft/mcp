// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Device;

public class IoTHubQueryDiscoverCommandTests
{
    private readonly IIoTHubDeviceService _service;
    private readonly IoTHubQueryDiscoverCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubQueryDiscoverCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        var logger = Substitute.For<ILogger<IoTHubQueryDiscoverCommand>>();
        var collection = new ServiceCollection().AddSingleton(_service);
        _command = new IoTHubQueryDiscoverCommand(_service, logger);
        _context = new CommandContext(collection.BuildServiceProvider());
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_DiscoversFieldsFromSampledDevices()
    {
        // Arrange
        var items = new List<JsonElement>
        {
            JsonSerializer.SerializeToElement(new
            {
                deviceId = "device-1",
                status = "enabled",
                tags = new
                {
                    machineType = "welding",
                    location = new { factory = "A", floor = 1 }
                },
                properties = new
                {
                    reported = new { batteryLevel = 92, runtimeStatus = "running", temperature = 44 },
                    desired = new { targetTemperature = 70 }
                }
            }),
            JsonSerializer.SerializeToElement(new
            {
                deviceId = "device-2",
                status = "enabled",
                tags = new
                {
                    machineType = "welding",
                    location = new { factory = "A", floor = 2 }
                },
                properties = new
                {
                    reported = new { batteryLevel = 66, runtimeStatus = "error", temperature = 90 },
                    desired = new { targetTemperature = 75 }
                }
            })
        };

        _service.RunQuery(
            "SELECT * FROM devices",
            "test-hub",
            "test-rg",
            "sub-id",
            10,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage(items, null));

        var args = _commandDefinition.Parse([
            "--name", "test-hub",
            "--resource-group", "test-rg",
            "--subscription", "sub-id"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal(2, result.SampleCount);
        Assert.Equal(10, result.MaxCount);
        Assert.Contains(result.Fields.Device, field => field.Field == "deviceId" && field.Type == "string");
        Assert.Contains(result.Fields.Device, field => field.Field == "status" && field.Type == "string");
        Assert.Contains(result.Fields.Tags, field => field.Field == "machineType" && field.Type == "string");
        Assert.Contains(result.Fields.Tags, field => field.Field == "location.factory" && field.Type == "string");
        Assert.Contains(result.Fields.Tags, field => field.Field == "location.floor" && field.Type == "number");
        Assert.Contains(result.Fields.Desired, field => field.Field == "targetTemperature" && field.Type == "number");
        Assert.Contains(result.Fields.Reported, field => field.Field == "batteryLevel" && field.Type == "number");
        Assert.Contains(result.Fields.Reported, field => field.Field == "runtimeStatus" && field.Type == "string");
        Assert.Contains("Discovered", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_CapsMaxCountAt100()
    {
        // Arrange
        _service.RunQuery(
            "SELECT * FROM devices",
            "test-hub",
            "test-rg",
            "sub-id",
            100,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage([], null));

        var args = _commandDefinition.Parse([
            "--name", "test-hub",
            "--resource-group", "test-rg",
            "--subscription", "sub-id",
            "--max-count", "500"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal(100, result.MaxCount);

        await _service.Received(1).RunQuery(
            "SELECT * FROM devices",
            "test-hub",
            "test-rg",
            "sub-id",
            100,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MaxCountLessThanOne_ReturnsBadRequest()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--name", "test-hub",
            "--resource-group", "test-rg",
            "--subscription", "sub-id",
            "--max-count", "0"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("less than 1", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Assert
        Assert.Equal("iothub-query-discover", _command.Id);
        Assert.Equal("discover", _command.Name);
        Assert.Contains("Discover queryable IoT Hub device twin field paths", _command.Description, StringComparison.Ordinal);
        Assert.Contains("iothub query compile --discovered-fields", _command.Description, StringComparison.Ordinal);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.Secret);
    }

    private static IoTHubQueryDiscoverResult DeserializeResult(CommandResponse response)
    {
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<IoTHubQueryDiscoverResult>(json);
        Assert.NotNull(result);
        return result;
    }
}
