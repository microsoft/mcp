// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DeviceRegistry;

public class DeviceRegistrySetup : IAreaSetup
{
    public string Name => "deviceregistry";

    public string Title => "Manage Azure Device Registry";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDeviceRegistryService, DeviceRegistryService>();
        services.AddSingleton<NamespaceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var deviceRegistry = new CommandGroup(Name,
            """
            Device Registry operations - Commands to manage Azure Device Registry resources.
            Azure Device Registry (ADR) is a unified registry projecting assets and devices as Azure resources.
            Supports listing namespaces and managing device registry resources in your Azure subscription.
            """,
            Title);

        var namespaceGroup = new CommandGroup("namespace",
            "Device Registry namespace operations - Commands for listing and managing Device Registry namespaces.");
        deviceRegistry.AddSubGroup(namespaceGroup);

        var namespaceList = serviceProvider.GetRequiredService<NamespaceListCommand>();
        namespaceGroup.AddCommand(namespaceList.Name, namespaceList);

        return deviceRegistry;
    }
}
