// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Compute;

/// <summary>
/// Setup class for Compute toolset registration.
/// </summary>
public class ComputeSetup : IAreaSetup
{
    public string Name => "compute";

    public string Title => "Manage Azure Compute Resources";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IComputeService, ComputeService>();

        // VM commands
        services.AddSingleton<VmGetCommand>();
        services.AddSingleton<VmCreateCommand>();
        services.AddSingleton<VmUpdateCommand>();
        services.AddSingleton<VmDeleteCommand>();

        // VMSS commands
        services.AddSingleton<VmssGetCommand>();
        services.AddSingleton<VmssCreateCommand>();
        services.AddSingleton<VmssUpdateCommand>();
        services.AddSingleton<VmssDeleteCommand>();

        // Disk commands
        services.AddSingleton<DiskCreateCommand>();
        services.AddSingleton<DiskDeleteCommand>();
        services.AddSingleton<DiskGetCommand>();
        services.AddSingleton<DiskUpdateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var compute = new CommandGroup(Name,
            """
            Compute operations - Commands for managing and monitoring Azure Virtual Machines (VMs), Virtual Machine Scale Sets (VMSS), and Managed Disks.
            This tool provides comprehensive access to VM lifecycle management, instance monitoring, size discovery, and scale set operations.
            Use this tool when you need to list, query, create, or monitor VMs and VMSS instances across subscriptions and resource groups.
            Defaults to Standard_D2s_v5 VM size for VM and VMSS creation when not specified; the --image option is required.
            This tool is a hierarchical MCP command router where sub-commands are routed to MCP servers that require specific fields
            inside the "parameters" object. To invoke a command, set "command" and wrap its arguments in "parameters".
            Set "learn=true" to discover available sub-commands for different Azure Compute operations.
            Note that this tool requires appropriate Azure RBAC permissions and will only access compute resources accessible to the authenticated user.
            """,
            Title);

        // Create VM subgroup
        var vm = new CommandGroup("vm", "Virtual Machine operations - Commands for managing and monitoring Azure Virtual Machines including lifecycle, status, creation, and size information.");
        compute.AddSubGroup(vm);

        // Register VM commands
        vm.AddCommand<VmGetCommand>(serviceProvider);
        vm.AddCommand<VmCreateCommand>(serviceProvider);
        vm.AddCommand<VmUpdateCommand>(serviceProvider);
        vm.AddCommand<VmDeleteCommand>(serviceProvider);

        // Create VMSS subgroup
        var vmss = new CommandGroup("vmss", "Virtual Machine Scale Set operations - Commands for managing and monitoring Azure Virtual Machine Scale Sets including scale set details, instances, and rolling upgrades.");
        compute.AddSubGroup(vmss);

        // Register VMSS commands
        vmss.AddCommand<VmssGetCommand>(serviceProvider);
        vmss.AddCommand<VmssCreateCommand>(serviceProvider);
        vmss.AddCommand<VmssUpdateCommand>(serviceProvider);
        vmss.AddCommand<VmssDeleteCommand>(serviceProvider);

        // Create Disk subgroup
        var disk = new CommandGroup(
            "disk",
            "Managed Disk operations - Get details about Azure managed disks in your subscription.");
        compute.AddSubGroup(disk);

        // Register Disk commands
        disk.AddCommand<DiskCreateCommand>(serviceProvider);
        disk.AddCommand<DiskDeleteCommand>(serviceProvider);
        disk.AddCommand<DiskGetCommand>(serviceProvider);
        disk.AddCommand<DiskUpdateCommand>(serviceProvider);

        return compute;
    }
}
