// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.VirtualDesktop;

public class VirtualDesktopSetup : IAreaSetup
{
    public string Name => "virtualdesktop";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IVirtualDesktopService, VirtualDesktopService>();

        services.AddSingleton<HostpoolListCommand>();
        services.AddSingleton<SessionHostListCommand>();
        services.AddSingleton<SessionHostUserSessionListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var desktop = new CommandGroup(Name, "Azure Virtual Desktop operations - Commands for managing and accessing Azure Virtual Desktop resources. Includes operations for hostpools, session hosts, and user sessions.");

        // Create AVD subgroups
        var hostpool = new CommandGroup("hostpool", "Hostpool operations - Commands for listing and managing Hostpools, including listing and changing settings on hostpools.");
        desktop.AddSubGroup(hostpool);

        var sessionhost = new CommandGroup("sessionhost", "Sessionhost operations - Commands for listing and managing session hosts inside a host pool.");
        hostpool.AddSubGroup(sessionhost);

        // Register AVD commands
        var hostpoolList = serviceProvider.GetRequiredService<HostpoolListCommand>();
        hostpool.AddCommand(hostpoolList.Name, hostpoolList);
        var sessionHostList = serviceProvider.GetRequiredService<SessionHostListCommand>();
        sessionhost.AddCommand(sessionHostList.Name, sessionHostList);
        var sessionHostUserSessionList = serviceProvider.GetRequiredService<SessionHostUserSessionListCommand>();
        sessionhost.AddCommand(sessionHostUserSessionList.Name, sessionHostUserSessionList);

        return desktop;
    }
}
