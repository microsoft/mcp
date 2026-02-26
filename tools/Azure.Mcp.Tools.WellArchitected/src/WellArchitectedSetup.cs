// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.WellArchitected.Commands.Analyze;
using Azure.Mcp.Tools.WellArchitected.Commands.Checklist;
using Azure.Mcp.Tools.WellArchitected.Commands.Recommendation;
using Azure.Mcp.Tools.WellArchitected.Commands.ServiceGuide;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.WellArchitected;

public class WellArchitectedSetup : IAreaSetup
{
    public string Name => "wellarchitected";
    public string Title => "Azure Well-Architected Framework";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWellArchitectedService, WellArchitectedService>();
        services.AddSingleton<AnalyzeCommand>();
        services.AddSingleton<RecommendationListCommand>();
        services.AddSingleton<RecommendationGetCommand>();
        services.AddSingleton<ChecklistGetCommand>();
        services.AddSingleton<ServiceGuideGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var wellArchitectedGroup = new CommandGroup(Name, "Azure Well-Architected Framework tools", Title);

        wellArchitectedGroup.AddCommand("analyze", serviceProvider.GetRequiredService<AnalyzeCommand>());

        var recommendationGroup = new CommandGroup("recommendation", "Well-Architected recommendation operations");
        recommendationGroup.AddCommand("list", serviceProvider.GetRequiredService<RecommendationListCommand>());
        recommendationGroup.AddCommand("get", serviceProvider.GetRequiredService<RecommendationGetCommand>());
        wellArchitectedGroup.AddSubGroup(recommendationGroup);

        var checklistGroup = new CommandGroup("checklist", "Well-Architected checklist operations");
        checklistGroup.AddCommand("get", serviceProvider.GetRequiredService<ChecklistGetCommand>());
        wellArchitectedGroup.AddSubGroup(checklistGroup);

        var serviceGuideGroup = new CommandGroup("serviceguide", "Well-Architected service guide operations");
        serviceGuideGroup.AddCommand("get", serviceProvider.GetRequiredService<ServiceGuideGetCommand>());
        wellArchitectedGroup.AddSubGroup(serviceGuideGroup);

        return wellArchitectedGroup;
    }
}
