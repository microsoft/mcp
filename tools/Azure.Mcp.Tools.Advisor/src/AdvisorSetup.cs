// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Commands.Metadata;
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
        services.AddSingleton<IRecommendationSummaryService, RecommendationSummaryService>();
        services.AddSingleton<RecommendationListCommand>();
        services.AddSingleton<RecommendationUpdateCommand>();
        services.AddSingleton<RecommendationSummaryCommand>();
        services.AddSingleton<RecommendationApplyCommand>();
        services.AddSingleton<RecommendationMetadataListCommand>();
        services.AddSingleton<MetadataGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Advisor command group
        var advisor = new CommandGroup(Name, "Azure Advisor operations - Query Azure Advisor recommendations across subscriptions OR Apply Azure Advisor recommendations to your IaaC files (ARM, Terraform). Use when you need subscription-scoped visibility into Advisor recommendations OR want to apply Advisor recommendations to your IaaC files. Requires Azure subscription context for querying Advisor recommendations.", Title);

        // Create Advisor subgroups
        var recommendation = new CommandGroup(
            "recommendation",
            "Advisor recommendations - List individual recommendations; summarize counts, rankings, lifecycle states, metadata subcategories, and service-retirement dates; update customer-provided state; or apply recommendation guidance to infrastructure-as-code files.");
        advisor.AddSubGroup(recommendation);

        var metadata = new CommandGroup(
            "metadata",
            "Discover and retrieve the global Azure Advisor recommendation metadata catalog, also known as recommendation types, from Azure Resource Graph. List localized guidance, impact, categories, subcategories, supported resource types, actions, and service-retirement details, or get a specific catalog entry by recommendation type ID. Use the list command in greenfield environments with no generated recommendations, filter by resource type during brownfield onboarding, or find service retirements by tracking ID and retirement date. Service-retirement filters apply to the ServiceUpgradeAndRetirement subcategory; conflicting subcategory filters are rejected. List results are ordered High, Medium, then Low impact.");
        advisor.AddSubGroup(metadata);

        // Register Advisor commands
        recommendation.AddCommand<RecommendationListCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationUpdateCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationSummaryCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationApplyCommand>(serviceProvider);
        metadata.AddCommand<RecommendationMetadataListCommand>(serviceProvider);
        metadata.AddCommand<MetadataGetCommand>(serviceProvider);

        return advisor;
    }
}
