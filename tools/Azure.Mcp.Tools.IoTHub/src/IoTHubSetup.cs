// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
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

        services.AddSingleton<IoTHubCreateCommand>();
        services.AddSingleton<IoTHubGetCommand>();
        services.AddSingleton<IoTHubUpdateCommand>();
        services.AddSingleton<IoTHubDeleteCommand>();
        services.AddSingleton<IoTHubKeysGetCommand>();
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

        return iothub;
    }
}
