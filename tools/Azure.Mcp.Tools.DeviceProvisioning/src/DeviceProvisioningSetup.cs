// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Commands.Service;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DeviceProvisioning;

public class DeviceProvisioningSetup : IAreaSetup
{
    public string Name => "deviceprovisioning";

    public string Title => "Manage Azure Device Provisioning Service";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDeviceProvisioningService, DeviceProvisioningService>();
        services.AddSingleton<DeviceProvisioningServiceGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var deviceProvisioning = new CommandGroup(
            Name,
            "Azure Device Provisioning Service operations.",
            Title);

        var service = new CommandGroup(
            "service",
            "Azure Device Provisioning Service resource operations.");
        deviceProvisioning.AddSubGroup(service);
        service.AddCommand<DeviceProvisioningServiceGetCommand>(serviceProvider);

        return deviceProvisioning;
    }
}

