// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.DeviceRegistry;

public class DeviceRegistrySetup : IAreaSetup
{
    public string Name => "deviceregistry";

    public string Title => "Azure Device Registry";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDeviceRegistryService, DeviceRegistryService>();
        services.AddSingleton<NamespaceListCommand>();
        services.AddSingleton<NamespaceCreateCommand>();
        services.AddSingleton<NamespaceGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Device Registry top-level group
        var deviceRegistry = new CommandGroup(Name, "Azure Device Registry operations - Commands for managing Device Registry namespaces and resources.", Title);

        // Namespace subgroup
        var namespaces = new CommandGroup("namespace", "Device Registry namespace operations - Commands for managing Device Registry namespaces.");
        deviceRegistry.AddSubGroup(namespaces);

        // Register Namespace commands
        var namespaceList = serviceProvider.GetRequiredService<NamespaceListCommand>();
        namespaces.AddCommand(namespaceList.Name, namespaceList);

        var namespaceCreate = serviceProvider.GetRequiredService<NamespaceCreateCommand>();
        namespaces.AddCommand(namespaceCreate.Name, namespaceCreate);

        var namespaceGet = serviceProvider.GetRequiredService<NamespaceGetCommand>();
        namespaces.AddCommand(namespaceGet.Name, namespaceGet);

        return deviceRegistry;
    }
}
