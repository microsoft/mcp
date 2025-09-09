// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.VirtualDesktop;

public class VirtualDesktopSetup : IAreaSetup
{
    public string Name => "virtualdesktop";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IVirtualDesktopService, VirtualDesktopService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var desktop = new CommandGroup(Name, "Azure Virtual Desktop operations - Commands for managing and accessing Azure Virtual Desktop resources. Includes operations for hostpools, session hosts, and user sessions.");
        rootGroup.AddSubGroup(desktop);

        // Create AVD subgroups
        var hostpool = new CommandGroup("hostpool", "Hostpool operations - Commands for listing and managing Hostpools, including listing and changing settings on hostpools.");
        desktop.AddSubGroup(hostpool);

        var sessionhost = new CommandGroup("sessionhost", "Sessionhost operations - Commands for listing and managing session hosts inside a host pool.");
        hostpool.AddSubGroup(sessionhost);

        // Register AVD commands
        hostpool.AddCommand("list", new HostpoolListCommand(
            loggerFactory.CreateLogger<HostpoolListCommand>()));
        sessionhost.AddCommand("list", new SessionHostListCommand(
            loggerFactory.CreateLogger<SessionHostListCommand>()));
        sessionhost.AddCommand("usersession-list", new SessionHostUserSessionListCommand(
            loggerFactory.CreateLogger<SessionHostUserSessionListCommand>()));
    }
}
