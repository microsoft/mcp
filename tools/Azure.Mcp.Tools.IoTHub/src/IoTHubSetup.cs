// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
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

        services.AddSingleton<IoTHubDeviceListCommand>();
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

        return iothub;
    }
}
