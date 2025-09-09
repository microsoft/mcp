// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CloudArchitect.Commands.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.CloudArchitect;

public class CloudArchitectSetup : IAreaSetup
{
    public string Name => "cloudarchitect";

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create CloudArchitect command group
        var cloudArchitect = new CommandGroup(Name, "Cloud Architecture operations - Commands for generating Azure architecture designs and recommendations based on requirements.");
        rootGroup.AddSubGroup(cloudArchitect);

        // Register CloudArchitect commands
        cloudArchitect.AddCommand("design", new DesignCommand(
            loggerFactory.CreateLogger<DesignCommand>()));
    }
}
