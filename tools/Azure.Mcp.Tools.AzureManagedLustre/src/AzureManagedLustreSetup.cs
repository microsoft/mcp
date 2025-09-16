// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre;

public class AzureManagedLustreSetup : IAreaSetup
{
    public string Name => "azuremanagedlustre";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAzureManagedLustreService, AzureManagedLustreService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var azureManagedLustre = new CommandGroup(Name,
            "Azure Managed Lustre operations - Commands for listing and inspecting Azure Managed Lustre file systems (AMLFS) used for high-performance computing workloads.");
        rootGroup.AddSubGroup(azureManagedLustre);

        var fileSystem = new CommandGroup("filesystem", "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.");
        azureManagedLustre.AddSubGroup(fileSystem);

        fileSystem.AddCommand("list", new FileSystemListCommand(loggerFactory.CreateLogger<FileSystemListCommand>()));
        fileSystem.AddCommand("required-subnet-size", new FileSystemSubnetSizeCommand(loggerFactory.CreateLogger<FileSystemSubnetSizeCommand>()));

        var sku = new CommandGroup("sku", "This group provides commands to discover and retrieve information about available Azure Managed Lustre SKUs, including supported tiers, performance characteristics, and regional availability. Use these commands to validate SKU options prior to provisioning or updating a filesystem.");
        fileSystem.AddSubGroup(sku);

        sku.AddCommand("get", new SkuGetCommand(loggerFactory.CreateLogger<SkuGetCommand>()));
    }
}
