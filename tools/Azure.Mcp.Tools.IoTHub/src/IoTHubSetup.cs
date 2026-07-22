// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.IoTHub;

public class IoTHubSetup : IAreaSetup
{
    public string Name => "iothub";

    public string Title => "Manage Azure IoT Hub";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IIoTHubService, IoTHubService>();
        services.AddSingleton<IIoTHubDeviceService, IoTHubDeviceService>();

        services.AddSingleton<IoTHubDeviceListCommand>();
        services.AddSingleton<IoTHubGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var iothub = new CommandGroup(Name,
            "IoT Hub operations - Commands for managing Azure IoT Hubs.",
            Title);

        var device = new CommandGroup("device", "IoT Hub device registry operations.");
        iothub.AddSubGroup(device);

        var deviceListCommand = serviceProvider.GetRequiredService<IoTHubDeviceListCommand>();
        device.AddCommand(deviceListCommand.Name, deviceListCommand);
        var hub = new CommandGroup("hub", "IoT Hub resource operations.");
        iothub.AddSubGroup(hub);

        hub.AddCommand<IoTHubGetCommand>(serviceProvider);

        return iothub;
    }
}
