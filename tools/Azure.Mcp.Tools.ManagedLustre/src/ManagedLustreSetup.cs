// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;
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
        services.AddSingleton<AutoexportJobDeleteCommand>();
        services.AddSingleton<AutoimportJobCreateCommand>();
        services.AddSingleton<AutoimportJobCancelCommand>();
        services.AddSingleton<AutoimportJobGetCommand>();
        services.AddSingleton<AutoimportJobDeleteCommand>();
        services.AddSingleton<ImportJobCreateCommand>();
        services.AddSingleton<ImportJobCancelCommand>();
        services.AddSingleton<ImportJobGetCommand>();
        services.AddSingleton<ImportJobDeleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var managedLustre = new CommandGroup(Name,
            "Azure Managed Lustre operations - Commands for creating, updating, listing and inspecting Azure Managed Lustre file systems (AMLFS) used for high-performance computing workloads. The tool focuses on managing all the aspects related to Azure Managed Lustre file system instances.", Title);

        var fileSystem = new CommandGroup("fs", "Azure Managed Lustre file system operations - Commands for listing managed Lustre file systems.");
        managedLustre.AddSubGroup(fileSystem);

        fileSystem.AddCommand<FileSystemListCommand>(serviceProvider);
        fileSystem.AddCommand<FileSystemCreateCommand>(serviceProvider);
        fileSystem.AddCommand<FileSystemUpdateCommand>(serviceProvider);

        var subnetSize = new CommandGroup("subnetsize", "Subnet size planning and validation operations for Azure Managed Lustre.");
        fileSystem.AddSubGroup(subnetSize);

        subnetSize.AddCommand<SubnetSizeAskCommand>(serviceProvider);
        subnetSize.AddCommand<SubnetSizeValidateCommand>(serviceProvider);

        var sku = new CommandGroup("sku", "This group provides commands to discover and retrieve information about available Azure Managed Lustre SKUs, including supported tiers, performance characteristics, and regional availability. Use these commands to validate SKU options prior to provisioning or updating a filesystem.");
        fileSystem.AddSubGroup(sku);

        sku.AddCommand<SkuGetCommand>(serviceProvider);

        var autoexportJob = new CommandGroup("blob-autoexport", "Autoexport job operations for Azure Managed Lustre - Commands for creating jobs to export data from the filesystem to blob storage.");
        fileSystem.AddSubGroup(autoexportJob);

        autoexportJob.AddCommand<AutoexportJobCreateCommand>(serviceProvider);
        autoexportJob.AddCommand<AutoexportJobCancelCommand>(serviceProvider);
        autoexportJob.AddCommand<AutoexportJobGetCommand>(serviceProvider);
        autoexportJob.AddCommand<AutoexportJobDeleteCommand>(serviceProvider);

        var autoimportJob = new CommandGroup("blob-autoimport", "Autoimport job operations for Azure Managed Lustre - Commands for creating jobs to import data from blob storage to the filesystem.");
        fileSystem.AddSubGroup(autoimportJob);

        autoimportJob.AddCommand<AutoimportJobCreateCommand>(serviceProvider);
        autoimportJob.AddCommand<AutoimportJobCancelCommand>(serviceProvider);
        autoimportJob.AddCommand<AutoimportJobGetCommand>(serviceProvider);
        autoimportJob.AddCommand<AutoimportJobDeleteCommand>(serviceProvider);

        var blobImport = new CommandGroup("blob-import", "One-time blob import operations for Azure Managed Lustre - Commands for creating jobs to perform one-time import of data from blob storage to the filesystem.");
        fileSystem.AddSubGroup(blobImport);

        blobImport.AddCommand<ImportJobCreateCommand>(serviceProvider);
        blobImport.AddCommand<ImportJobCancelCommand>(serviceProvider);
        blobImport.AddCommand<ImportJobGetCommand>(serviceProvider);
        blobImport.AddCommand<ImportJobDeleteCommand>(serviceProvider);

        return managedLustre;
    }
}
