// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.Acr;

public class AcrSetup : IAreaSetup
{
    public string Name => "acr";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAcrService, AcrService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var acr = new CommandGroup(Name, "Azure Container Registry operations - Commands for managing Azure Container Registry resources. Includes operations for listing container registries and managing registry configurations.");
        rootGroup.AddSubGroup(acr);

        var registry = new CommandGroup("registry", "Container Registry resource operations - Commands for listing and managing Container Registry resources in your Azure subscription.");
        acr.AddSubGroup(registry);

        registry.AddCommand("list", new RegistryListCommand(loggerFactory.CreateLogger<RegistryListCommand>()));

        var repository = new CommandGroup("repository", "Container Registry repository operations - Commands for listing and managing repositories within a Container Registry.");
        registry.AddSubGroup(repository);
        repository.AddCommand("list", new RegistryRepositoryListCommand(loggerFactory.CreateLogger<RegistryRepositoryListCommand>()));
    }
}
