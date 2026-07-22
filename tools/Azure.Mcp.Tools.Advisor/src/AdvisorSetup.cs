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
        services.AddSingleton<RecommendationSummaryCommand>();
        services.AddSingleton<RecommendationApplyCommand>();
        services.AddSingleton<RecommendationTypeListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Advisor command group
        var advisor = new CommandGroup(Name, "Azure Advisor operations - Query Azure Advisor recommendations across subscriptions OR Apply Azure Advisor recommendations to your IaaC files (ARM, Terraform). Use when you need subscription-scoped visibility into Advisor recommendations OR want to apply Advisor recommendations to your IaaC files. Requires Azure subscription context for querying Advisor recommendations.", Title);

        // Create Advisor subgroups
        var recommendation = new CommandGroup("recommendation", "Advisor recommendations - Commands for listing, summarizing, and applying Advisor recommendations in your Azure subscription.");
        advisor.AddSubGroup(recommendation);

        var recommendationType = new CommandGroup("recommendation-type", "Advisor recommendation type metadata - Commands for listing the catalog of Advisor recommendation types, categories, and impact levels available in the tenant. Useful for new or empty environments without generated recommendations.");
        advisor.AddSubGroup(recommendationType);

        // Register Advisor commands
        recommendation.AddCommand<RecommendationListCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationSummaryCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationApplyCommand>(serviceProvider);
        recommendationType.AddCommand<RecommendationTypeListCommand>(serviceProvider);

        return advisor;
    }
}
