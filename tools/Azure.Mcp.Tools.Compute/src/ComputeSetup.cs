// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Commands.PlacementScore;
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

        services.AddSingleton<IComputePlacementService, ComputePlacementService>();

        services.AddSingleton<SpotPlacementMetadataCommand>();
        services.AddSingleton<SpotPlacementScoreCommand>();

        // VM commands
        services.AddSingleton<VmGetCommand>();

        // VMSS commands
        services.AddSingleton<VmssGetCommand>();

        // Disk commands
        services.AddSingleton<DiskGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var compute = new CommandGroup(Name,
            """
            Compute operations - Commands for managing and monitoring Azure Virtual Machines (VMs), Virtual Machine Scale Sets (VMSS), Managed Disks,
            and spot vm placement recommendations. This tool provides comprehensive access to VM lifecycle management, instance monitoring, size
            discovery, scale set operations, and capacity-aware Spot VM placement scores across regions and availability zones.
            Use this tool when you need to list, query, or monitor VMs and VMSS instances across subscriptions and resource groups, or when you need
            to evaluate Spot VM placement likelihood and quota availability to choose optimal regions and SKUs.
            This tool is a hierarchical MCP command router where sub-commands are routed to MCP servers that require specific fields
            inside the "parameters" object. To invoke a command, set "command" and wrap its arguments in "parameters".
            Set "learn=true" to discover available sub-commands for different Azure Compute operations.
            Note that this tool requires appropriate Azure RBAC permissions and will only access compute resources accessible to the authenticated user.
            """,
            Title);

        // Create VM subgroup
        var vm = new CommandGroup("vm", "Virtual Machine operations - Commands for managing and monitoring Azure Virtual Machines including lifecycle, status, and size information.");
        compute.AddSubGroup(vm);

        // Register VM commands
        var vmGet = serviceProvider.GetRequiredService<VmGetCommand>();
        vm.AddCommand(vmGet.Name, vmGet);

        // Create VMSS subgroup
        var vmss = new CommandGroup("vmss", "Virtual Machine Scale Set operations - Commands for managing and monitoring Azure Virtual Machine Scale Sets including scale set details, instances, and rolling upgrades.");
        compute.AddSubGroup(vmss);

        // Register VMSS commands
        var vmssGet = serviceProvider.GetRequiredService<VmssGetCommand>();
        vmss.AddCommand(vmssGet.Name, vmssGet);

        // Create Disk subgroup
        var disk = new CommandGroup(
            "disk",
            "Managed Disk operations - Get details about Azure managed disks in your subscription.");
        compute.AddSubGroup(disk);

        // Register Disk commands
        var diskGet = serviceProvider.GetRequiredService<DiskGetCommand>();
        disk.AddCommand(diskGet.Name, diskGet);

        // Create placement score subgroup
        var placementScore = new CommandGroup(
            "placementscore",
            "Placement score operations - Commands for evaluating VM placement scores across Azure regions and availability zones.");
        compute.AddSubGroup(placementScore);

        // Create spot subgroup under placement scores
        var spot = new CommandGroup(
            "spot",
            "Spot VM placement score operations - Commands for evaluating Spot VM allocation likelihood and discovering supported resource types.");
        placementScore.AddSubGroup(spot);

        // Register Spot placement commands
        var metadataCommand = serviceProvider.GetRequiredService<SpotPlacementMetadataCommand>();
        spot.AddCommand(metadataCommand.Name, metadataCommand);

        var scoreCommand = serviceProvider.GetRequiredService<SpotPlacementScoreCommand>();
        spot.AddCommand(scoreCommand.Name, scoreCommand);

        return compute;
    }
}
