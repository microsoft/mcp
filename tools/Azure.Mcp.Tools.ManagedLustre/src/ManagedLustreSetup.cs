// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ManagedLustre;

public class ManagedLustreSetup : IAreaSetup
{
    public string Name => "managedlustre";

    public string Title => "Azure Managed Lustre";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IManagedLustreService, ManagedLustreService>();

        services.AddSingleton<FileSystemListCommand>();
        services.AddSingleton<FileSystemCreateCommand>();
        services.AddSingleton<FileSystemUpdateCommand>();
        services.AddSingleton<SubnetSizeAskCommand>();
        services.AddSingleton<SubnetSizeValidateCommand>();
        services.AddSingleton<SkuGetCommand>();
        services.AddSingleton<AutoexportJobCreateCommand>();
        services.AddSingleton<AutoexportJobCancelCommand>();
        services.AddSingleton<AutoexportJobGetCommand>();
        services.AddSingleton<AutoexportJobListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var managedLustre = new CommandGroup(Name,
            "Azure Managed Lustre operations - Commands for creating, updating, listing and inspecting Azure Managed Lustre file systems (AMLFS) used for high-performance computing workloads. The tool focuses on managing all the aspects related to Azure Managed Lustre file system instances.", Title);

        var fileSystem = new CommandGroup("fs", "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.");
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

        var autoexportJob = new CommandGroup("autoexport-job", "Autoexport archive job operations for Azure Managed Lustre - Commands for creating archive jobs to export data from the filesystem to blob storage.");
        fileSystem.AddSubGroup(autoexportJob);

        var autoexportJobCreate = serviceProvider.GetRequiredService<AutoexportJobCreateCommand>();
        autoexportJob.AddCommand(autoexportJobCreate.Name, autoexportJobCreate);

        var autoexportJobCancel = serviceProvider.GetRequiredService<AutoexportJobCancelCommand>();
        autoexportJob.AddCommand(autoexportJobCancel.Name, autoexportJobCancel);

        var autoexportJobGet = serviceProvider.GetRequiredService<AutoexportJobGetCommand>();
        autoexportJob.AddCommand(autoexportJobGet.Name, autoexportJobGet);

        var autoexportJobList = serviceProvider.GetRequiredService<AutoexportJobListCommand>();
        autoexportJob.AddCommand(autoexportJobList.Name, autoexportJobList);

        return managedLustre;
    }
}
