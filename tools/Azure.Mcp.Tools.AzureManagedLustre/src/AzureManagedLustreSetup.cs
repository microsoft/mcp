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
        services.AddSingleton<SubnetSizeAskCommand>();
        services.AddSingleton<SubnetSizeValidateCommand>();
        services.AddSingleton<SkuGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var azureManagedLustre = new CommandGroup(Name,
            "Azure Managed Lustre operations - Commands for listing and inspecting Azure Managed Lustre file systems (AMLFS) used for high-performance computing workloads.");

        var fileSystem = new CommandGroup("filesystem", "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.");
        azureManagedLustre.AddSubGroup(fileSystem);

        var list = serviceProvider.GetRequiredService<FileSystemListCommand>();
        fileSystem.AddCommand(list.Name, list);

        var subnetSize = new CommandGroup("subnetsize", "Subnet size planning and validation operations for Azure Managed Lustre.");
        fileSystem.AddSubGroup(subnetSize);

        var subnetSizeAsk = serviceProvider.GetRequiredService<SubnetSizeAskCommand>();
        subnetSize.AddCommand(subnetSizeAsk.Name, subnetSizeAsk);

        var subnetSizeValidate = serviceProvider.GetRequiredService<SubnetSizeValidateCommand>();
        subnetSize.AddCommand(subnetSizeValidate.Name, subnetSizeValidate);

        var sku = new CommandGroup("sku", "This group provides commands to discover and retrieve information about available Azure Managed Lustre SKUs, including supported tiers, performance characteristics, and regional availability. Use these commands to validate SKU options prior to provisioning or updating a filesystem.");
        fileSystem.AddSubGroup(sku);

        var skuGet = serviceProvider.GetRequiredService<SkuGetCommand>();
        sku.AddCommand(skuGet.Name, skuGet);

        return azureManagedLustre;
    }
}
