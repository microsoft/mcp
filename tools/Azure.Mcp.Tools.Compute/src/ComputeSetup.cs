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

        // VMSS commands
        services.AddSingleton<VmssGetCommand>();
        services.AddSingleton<VmssCreateCommand>();
        services.AddSingleton<VmssUpdateCommand>();

        // Disk commands
        services.AddSingleton<DiskGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var compute = new CommandGroup(Name,
            """
            Compute operations - Commands for managing and monitoring Azure Virtual Machines (VMs), Virtual Machine Scale Sets (VMSS), and Managed Disks.
            This tool provides comprehensive access to VM lifecycle management, instance monitoring, size discovery, and scale set operations.
            Use this tool when you need to list, query, create, or monitor VMs and VMSS instances across subscriptions and resource groups.
            Supports smart defaults for VM creation based on workload requirements (development, web, database, compute, memory, gpu, general).
            This tool is a hierarchical MCP command router where sub-commands are routed to MCP servers that require specific fields
            inside the "parameters" object. To invoke a command, set "command" and wrap its arguments in "parameters".
            Set "learn=true" to discover available sub-commands for different Azure Compute operations.
            Note that this tool requires appropriate Azure RBAC permissions and will only access compute resources accessible to the authenticated user.
            """,
            Title);

        // Create VM subgroup
        var vm = new CommandGroup("vm", "Virtual Machine operations - Commands for managing and monitoring Azure Virtual Machines including lifecycle, status, creation with smart workload-based defaults, and size information.");
        compute.AddSubGroup(vm);

        // Register VM commands
        var vmGet = serviceProvider.GetRequiredService<VmGetCommand>();
        vm.AddCommand(vmGet.Name, vmGet);

        var vmCreate = serviceProvider.GetRequiredService<VmCreateCommand>();
        vm.AddCommand(vmCreate.Name, vmCreate);

        var vmUpdate = serviceProvider.GetRequiredService<VmUpdateCommand>();
        vm.AddCommand(vmUpdate.Name, vmUpdate);

        // Create VMSS subgroup
        var vmss = new CommandGroup("vmss", "Virtual Machine Scale Set operations - Commands for managing and monitoring Azure Virtual Machine Scale Sets including scale set details, instances, and rolling upgrades.");
        compute.AddSubGroup(vmss);

        // Register VMSS commands
        var vmssGet = serviceProvider.GetRequiredService<VmssGetCommand>();
        vmss.AddCommand(vmssGet.Name, vmssGet);

        var vmssCreate = serviceProvider.GetRequiredService<VmssCreateCommand>();
        vmss.AddCommand(vmssCreate.Name, vmssCreate);

        var vmssUpdate = serviceProvider.GetRequiredService<VmssUpdateCommand>();
        vmss.AddCommand(vmssUpdate.Name, vmssUpdate);

        // Create Disk subgroup
        var disk = new CommandGroup(
            "disk",
            "Managed Disk operations - Get details about Azure managed disks in your subscription.");
        compute.AddSubGroup(disk);

        // Register Disk commands
        var diskGet = serviceProvider.GetRequiredService<DiskGetCommand>();
        disk.AddCommand(diskGet.Name, diskGet);

        return compute;
    }
}
