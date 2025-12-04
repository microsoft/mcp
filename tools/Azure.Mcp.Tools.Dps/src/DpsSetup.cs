// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Dps.Commands.Instance;
using Azure.Mcp.Tools.Dps.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Dps;

/// <summary>
/// Setup for Device Provisioning Service toolset.
/// </summary>
public class DpsSetup : IAreaSetup
{
    public string Name => "dps";

    public string Title => "Manage Azure Device Provisioning Service";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDpsService, DpsService>();
        services.AddSingleton<InstanceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var dps = new CommandGroup(Name,
            """
            Device Provisioning Service operations - Commands for listing and managing Azure IoT Hub Device Provisioning Service instances for zero-touch device provisioning. Use this tool when you need to list DPS instances and view their configuration. This tool focuses on read-only DPS management scenarios. This tool is a hierarchical MCP command router where sub-commands are routed to MCP servers that require specific fields inside the "parameters" object. To invoke a command, set "command" and wrap its arguments in "parameters". Set "learn=true" to discover available sub-commands for different DPS operations. Note that this tool requires appropriate DPS permissions and will only access resources accessible to the authenticated user.
            """,
            Title);

        // Create instance subgroup
        var instance = new CommandGroup("instance", "DPS instance operations - Commands for listing and viewing Device Provisioning Service instances in your Azure subscription.");
        dps.AddSubGroup(instance);

        // Register instance commands
        var instanceList = serviceProvider.GetRequiredService<InstanceListCommand>();
        instance.AddCommand(instanceList.Name, instanceList);

        return dps;
    }
}
