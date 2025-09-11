// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AzureManagedLustre;

public class AzureManagedLustreSetup : IAreaSetup
{
    public string Name => "azuremanagedlustre";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAzureManagedLustreService, AzureManagedLustreService>();

        services.AddSingleton<FileSystemListCommand>();
        services.AddSingleton<FileSystemSubnetSizeCommand>();
        services.AddSingleton<SkuGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var azureManagedLustre = new CommandGroup(Name,
            """
            Azure Managed Lustre operations - Azure Managed Lustre file systems (AMLFS) interaction for high-performance computing workloads.
            Use this tool to list and manage Azure Managed Lustre file systems, including creating import jobs to hydrate the file system namespace.
            """);
        rootGroup.AddSubGroup(azureManagedLustre);

        var fileSystem = new CommandGroup("filesystem", "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.");
        azureManagedLustre.AddSubGroup(fileSystem);

        var list = serviceProvider.GetRequiredService<FileSystemListCommand>();
        fileSystem.AddCommand(list.Name, list);

        var subnetSize = serviceProvider.GetRequiredService<FileSystemSubnetSizeCommand>();
        fileSystem.AddCommand(subnetSize.Name, subnetSize);

        var sku = new CommandGroup("sku", "This group provides commands to discover and retrieve information about available Azure Managed Lustre SKUs, including supported tiers, performance characteristics, and regional availability. Use these commands to validate SKU options prior to provisioning or updating a filesystem.");
        fileSystem.AddSubGroup(sku);

        var skuGet = serviceProvider.GetRequiredService<SkuGetCommand>();
        sku.AddCommand(skuGet.Name, skuGet);

        sku.AddCommand("get", new SkuGetCommand(loggerFactory.CreateLogger<SkuGetCommand>()));

        var importJob = new CommandGroup("import-job", "AMLFS import job operations - Create manual import jobs to hydrate the file system namespace.");
        fileSystem.AddSubGroup(importJob);

        importJob.AddCommand("create", new FileSystemImportJobCreateCommand(loggerFactory.CreateLogger<FileSystemImportJobCreateCommand>()));
        return azureManagedLustre;
    }
}
