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

        var subnetSize = new CommandGroup("subnetsize", "Subnet size planning and validation operations for Azure Managed Lustre.");
        fileSystem.AddSubGroup(subnetSize);
        subnetSize.AddCommand("ask", new SubnetSizeAskCommand(loggerFactory.CreateLogger<SubnetSizeAskCommand>()));
        subnetSize.AddCommand("validate", new SubnetSizeValidateCommand(loggerFactory.CreateLogger<SubnetSizeValidateCommand>()));
    }
}
