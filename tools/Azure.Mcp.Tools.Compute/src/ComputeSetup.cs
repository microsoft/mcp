// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Commands;
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

        // Named HttpClient for the unauthenticated Azure Retail Prices API used by vm list-skus --include-pricing.
        services.AddHttpClient(ComputeService.RetailPricesClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // VM commands
        services.AddSingleton<VmGetCommand>();
        services.AddSingleton<VmCreateCommand>();
        services.AddSingleton<VmUpdateCommand>();
        services.AddSingleton<VmDeleteCommand>();
        services.AddSingleton<VmPowerStateCommand>();
        services.AddSingleton<VmSkuListCommand>();
        services.AddSingleton<VmImageListCommand>();
        services.AddSingleton<VmQuotaCheckCommand>();
        services.AddSingleton<VmRegionRecommendCommand>();

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

        // Unified top-level create (dispatches to VMSS Flex by default, single VM with --single-instance)
        services.AddSingleton<UnifiedCreateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var compute = new CommandGroup(Name,
            """
            Compute operations - Commands for managing and monitoring Azure Virtual Machines (VMs), Virtual Machine Scale Sets (VMSS), and Managed Disks.
            VMSS Flex is the recommended default for new compute (it works equally well for 1 instance or N and is the GA orchestration mode);
            a single standalone VM is a fallback for non-scalable workloads. Use `compute create` for the unified entry point that defaults to VMSS Flex,
            or `compute vmss create` directly. Reach for `compute vm create` only when the workload truly cannot scale out.
            This tool provides comprehensive access to VM/VMSS lifecycle management, instance monitoring, SKU/region/quota/image discovery,
            and scale set operations. Top-level discovery commands (list-skus, list-images, check-quota, recommend-region) are available
            both directly under `compute` and under `compute vm`.
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
        vm.AddCommand<VmPowerStateCommand>(serviceProvider);
        vm.AddCommand<VmSkuListCommand>(serviceProvider);
        vm.AddCommand<VmImageListCommand>(serviceProvider);
        vm.AddCommand<VmQuotaCheckCommand>(serviceProvider);
        vm.AddCommand<VmRegionRecommendCommand>(serviceProvider);

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

        // Top-level unified create — recommended entry point. Dispatches to VMSS Flex by default; pass
        // --single-instance to fall back to a single non-scalable VM.
        compute.AddCommand<UnifiedCreateCommand>(serviceProvider);

        // Top-level discovery aliases so the four guided-create commands are reachable without
        // descending into the `vm` subgroup. Same class instances, same metadata IDs — just an
        // additional registration under the root group.
        compute.AddCommand<VmSkuListCommand>(serviceProvider);
        compute.AddCommand<VmImageListCommand>(serviceProvider);
        compute.AddCommand<VmQuotaCheckCommand>(serviceProvider);
        compute.AddCommand<VmRegionRecommendCommand>(serviceProvider);

        return compute;
    }
}
