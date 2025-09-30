// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.CloudArchitect.Commands.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.CloudArchitect;

public class CloudArchitectSetup : IAreaSetup
{
    public string Name => "cloudarchitect";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DesignCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create CloudArchitect command group
        var cloudArchitect = new CommandGroup(Name, "Azure Cloud Architecture Design Assistant - Interactive tool for designing optimal Azure architectures through guided requirements gathering. Use when users need: architecture recommendations, Azure solution design, cloud migration planning, component selection, or architectural guidance. Specializes in: multi-tier applications, scalability planning, security design, cost optimization, and Azure Well-Architected Framework compliance.");

        // Register CloudArchitect commands
        var design = serviceProvider.GetRequiredService<DesignCommand>();
        cloudArchitect.AddCommand(design.Name, design);

        return cloudArchitect;
    }
}
