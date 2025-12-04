// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.IoTHub;

public class IoTHubSetup : IAreaSetup
{
    public string Name => "iothub";

    public string Title => "Manage Azure IoT Hub";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IIoTHubService, IoTHubService>();
        services.AddSingleton<IIoTHubDeviceService, IoTHubDeviceService>();

        services.AddSingleton<IoTHubCreateCommand>();
        services.AddSingleton<IoTHubGetCommand>();
        services.AddSingleton<IoTHubUpdateCommand>();
        services.AddSingleton<IoTHubDeleteCommand>();
        services.AddSingleton<IoTHubKeysGetCommand>();

        services.AddSingleton<IoTHubDeviceListCommand>();
        services.AddSingleton<IoTHubDeviceTwinGetCommand>();
        services.AddSingleton<IoTHubDeviceTwinUpdateCommand>();
        services.AddSingleton<IoTHubDeviceTwinQueryCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var iothub = new CommandGroup(Name,
            "IoT Hub operations - Commands for managing Azure IoT Hubs.",
            Title);

        var hub = new CommandGroup("hub", "IoT Hub resource operations.");
        iothub.AddSubGroup(hub);

        var createCommand = serviceProvider.GetRequiredService<IoTHubCreateCommand>();
        hub.AddCommand(createCommand.Name, createCommand);

        var getCommand = serviceProvider.GetRequiredService<IoTHubGetCommand>();
        hub.AddCommand(getCommand.Name, getCommand);

        var updateCommand = serviceProvider.GetRequiredService<IoTHubUpdateCommand>();
        hub.AddCommand(updateCommand.Name, updateCommand);

        var deleteCommand = serviceProvider.GetRequiredService<IoTHubDeleteCommand>();
        hub.AddCommand(deleteCommand.Name, deleteCommand);

        var keysGetCommand = serviceProvider.GetRequiredService<IoTHubKeysGetCommand>();
        hub.AddCommand(keysGetCommand.Name, keysGetCommand);

        var device = new CommandGroup("device", "IoT Hub device registry operations.");
        iothub.AddSubGroup(device);

        var deviceListCommand = serviceProvider.GetRequiredService<IoTHubDeviceListCommand>();
        device.AddCommand(deviceListCommand.Name, deviceListCommand);

        var deviceTwinGetCommand = serviceProvider.GetRequiredService<IoTHubDeviceTwinGetCommand>();
        device.AddCommand(deviceTwinGetCommand.Name, deviceTwinGetCommand);

        var deviceTwinUpdateCommand = serviceProvider.GetRequiredService<IoTHubDeviceTwinUpdateCommand>();
        device.AddCommand(deviceTwinUpdateCommand.Name, deviceTwinUpdateCommand);

        var deviceTwinQueryCommand = serviceProvider.GetRequiredService<IoTHubDeviceTwinQueryCommand>();
        device.AddCommand(deviceTwinQueryCommand.Name, deviceTwinQueryCommand);

        return iothub;
    }
}
