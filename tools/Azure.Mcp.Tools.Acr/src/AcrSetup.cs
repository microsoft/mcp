// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Azure.Mcp.Core.Areas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Acr;

public class AcrSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAcrService, AcrService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var acr = new CommandGroup("acr", "Azure Container Registry operations - Commands for managing Azure Container Registry resources. Includes operations for listing container registries and managing registry configurations.");
        rootGroup.AddSubGroup(acr);

        var registry = new CommandGroup("registry", "Container Registry resource operations - Commands for listing and managing Container Registry resources in your Azure subscription.");
        acr.AddSubGroup(registry);

        registry.AddCommand("list", new RegistryListCommand(loggerFactory.CreateLogger<RegistryListCommand>()));
    }
}
