// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.ManagedLustre;

public class ManagedLustreSetup : IAreaSetup
{
    public string Name => "managedlustre";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IManagedLustreService, ManagedLustreService>();

        services.AddSingleton<FileSystemListCommand>();
        services.AddSingleton<FileSystemCreateCommand>();
        services.AddSingleton<FileSystemUpdateCommand>();
        services.AddSingleton<SubnetSizeAskCommand>();
        services.AddSingleton<SubnetSizeValidateCommand>();
        services.AddSingleton<SkuGetCommand>();
        services.AddSingleton<FileSystemImportJobCreateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var managedLustre = new CommandGroup(Name,
            """
            Azure Managed Lustre operations - Azure Managed Lustre file systems (AMLFS) interaction for high-performance computing workloads.
            Use this tool to list and manage Azure Managed Lustre file systems, including creating import jobs to hydrate the file system namespace.
            """);

        var fileSystem = new CommandGroup("filesystem", "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.");
        managedLustre.AddSubGroup(fileSystem);

        var list = serviceProvider.GetRequiredService<FileSystemListCommand>();
        fileSystem.AddCommand(list.Name, list);

        var create = serviceProvider.GetRequiredService<FileSystemCreateCommand>();
        fileSystem.AddCommand(create.Name, create);

        var update = serviceProvider.GetRequiredService<FileSystemUpdateCommand>();
        fileSystem.AddCommand(update.Name, update);

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

        var importJob = new CommandGroup("importjob", "Azure Managed Lustre file system import job operations - Create manual import jobs to hydrate the file system namespace.");
        fileSystem.AddSubGroup(importJob);

        var importJobCreate = serviceProvider.GetRequiredService<FileSystemImportJobCreateCommand>();
        importJob.AddCommand(importJobCreate.Name, importJobCreate);

        return managedLustre;
    }
}
