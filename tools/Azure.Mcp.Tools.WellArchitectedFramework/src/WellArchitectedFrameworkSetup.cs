// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands.UsageGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.UsageGuide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.WellArchitectedFramework;

public class WellArchitectedFrameworkSetup : IAreaSetup
{
    public string Name => "wellarchitectedframework";

    public string Title => "Azure Well-Architected Framework";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUsageGuideService, UsageGuideService>();
        services.AddSingleton<UsageGuideGetCommand>();

        services.AddSingleton<IServiceGuideService, ServiceGuideService>();
        services.AddSingleton<ServiceGuideGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var wellArchitectedCommandGroup = new CommandGroup(Name, "Azure Well-Architected Framework operations - Commands for accessing guidance, best practices, and recommendations based on the Azure Well-Architected Framework pillars (reliability, security, cost optimization, operational excellence, and performance efficiency).", Title);

        // Register usageguide command group
        var usageGuide = new CommandGroup("usageguide", "Usage guide operations - Commands for retrieving the Azure Well-Architected Framework usage guide for AI agents.");
        wellArchitectedCommandGroup.AddSubGroup(usageGuide);

        var usageGuideGet = serviceProvider.GetRequiredService<UsageGuideGetCommand>();
        usageGuide.AddCommand(usageGuideGet.Name, usageGuideGet);

        // Register serviceguide command group
        var serviceGuide = new CommandGroup("serviceguide", "Service guide operations - Commands for retrieving Azure Well-Architected Framework service-specific guidance and recommendations.");
        wellArchitectedCommandGroup.AddSubGroup(serviceGuide);

        var serviceGuideGet = serviceProvider.GetRequiredService<ServiceGuideGetCommand>();
        serviceGuide.AddCommand(serviceGuideGet.Name, serviceGuideGet);

        return wellArchitectedCommandGroup;
    }
}
