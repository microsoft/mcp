// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor;

public class AdvisorSetup : IAreaSetup
{
    public string Name => "advisor";
    public string Title => "Azure Advisor Recommendations";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAdvisorService, AdvisorService>();

        services.AddSingleton<RecommendationListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Advisor command group
        var advisor = new CommandGroup(Name,
            """
            Azure Advisor operations - Manage and query Azure Advisor
            recommendations across subscriptions. Use when you need
            subscription-scoped visibility into Advisor recommendations. Requires
            Azure subscription context.
            """, Title);

        // Create Advisor subgroups
        var recommendation = new CommandGroup("recommendation",
            """
            Advisor recommendations - Commands for listing Advisor recommendations
            in your Azure subscription.
            """);
        advisor.AddSubGroup(recommendation);

        // Register Advisor commands
        var recommendationList = serviceProvider.GetRequiredService<RecommendationListCommand>();
        recommendation.AddCommand(recommendationList.Name, recommendationList);

        return advisor;
    }
}
