// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Compute;

public class ComputeSetup : IAreaSetup
{
    public string Name => "compute";

    public string Title => "Manage Azure Compute Resources";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IComputeService, ComputeService>();

        // VM commands
        services.AddSingleton<VmGetCommand>();

        // VMSS commands
        services.AddSingleton<VmssGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var compute = new CommandGroup(Name,
            """
            Compute operations - Commands for managing and monitoring Azure Virtual Machines (VMs) and Virtual Machine Scale Sets (VMSS).
            This tool provides comprehensive access to VM lifecycle management, instance monitoring, size discovery, and scale set operations.
            Use this tool when you need to list, query, or monitor VMs and VMSS instances across subscriptions and resource groups.
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

        return compute;
    }
}
