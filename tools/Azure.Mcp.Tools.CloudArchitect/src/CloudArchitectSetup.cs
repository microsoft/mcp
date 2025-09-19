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
        var cloudArchitect = new CommandGroup(Name, "Cloud Architecture operations - Commands for generating Azure architecture designs and recommendations based on requirements.");

        // Register CloudArchitect commands
        var design = serviceProvider.GetRequiredService<DesignCommand>();
        cloudArchitect.AddCommand(design.Name, design);

        return cloudArchitect;
    }
}
