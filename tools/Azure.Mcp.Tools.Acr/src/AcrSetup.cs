// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Acr;

public class AcrSetup : IAreaSetup
{
    public string Name => "acr";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAcrService, AcrService>();

        services.AddSingleton<RegistryListCommand>();
        services.AddSingleton<RegistryRepositoryListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var acr = new CommandGroup(Name, "Azure Container Registry operations - Commands for managing Azure Container Registry resources. Includes operations for listing container registries and managing registry configurations.");

        var registry = new CommandGroup("registry", "Container Registry resource operations - Commands for listing and managing Container Registry resources in your Azure subscription.");
        acr.AddSubGroup(registry);

        var registryList = serviceProvider.GetRequiredService<RegistryListCommand>();
        registry.AddCommand(registryList.Name, registryList);

        var repository = new CommandGroup("repository", "Container Registry repository operations - Commands for listing and managing repositories within a Container Registry.");

        registry.AddSubGroup(repository);

        var repositoryList = serviceProvider.GetRequiredService<RegistryRepositoryListCommand>();
        repository.AddCommand(repositoryList.Name, repositoryList);

        return acr;
    }
}
