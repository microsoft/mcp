// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Commands.Disk;
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
    /// <inheritdoc/>
    public string Name => "compute";

    /// <inheritdoc/>
    public string Title => "Manage Azure compute resources";

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IComputeService, ComputeService>();
        services.AddSingleton<DiskGetCommand>();
    }

    /// <inheritdoc/>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var compute = new CommandGroup(
            Name,
            "Azure Compute operations - Commands for managing Azure compute resources including virtual machines and managed disks.",
            Title);

        var disk = new CommandGroup(
            "disk",
            "Managed Disk operations - Get details about Azure managed disks in your subscription.");
        compute.AddSubGroup(disk);

        disk.AddCommand("get", serviceProvider.GetRequiredService<DiskGetCommand>());

        return compute;
    }
}
