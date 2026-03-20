// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;
using Azure.Mcp.Tools.ContainerApps.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ContainerApps;

public class ContainerAppsSetup : IAreaSetup
{
    public string Name => "containerapps";

    public string Title => "Azure Container Apps Management";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IContainerAppsService, ContainerAppsService>();

        services.AddSingleton<ContainerAppListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var containerapps = new CommandGroup(Name, "Azure Container Apps operations - Commands for managing Azure Container Apps resources. Includes operations for listing container apps and managing container app configurations.", Title);

        var containerAppList = serviceProvider.GetRequiredService<ContainerAppListCommand>();
        containerapps.AddCommand(containerAppList.Name, containerAppList);

        return containerapps;
    }
}
