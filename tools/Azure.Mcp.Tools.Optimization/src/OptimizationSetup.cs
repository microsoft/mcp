// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Optimization.Commands.Recommendation;
using Azure.Mcp.Tools.Optimization.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Optimization;

public class OptimizationSetup : IAreaSetup
{
    public string Name => "optimization";

    public string Title => "Azure Optimization Recommendations";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOptimizationService, OptimizationService>();
        services.AddSingleton<RecommendationListCommand>();
        services.AddSingleton<RecommendationAlternativesCommand>();
        services.AddSingleton<RecommendationExplainCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var optimization = new CommandGroup(
            Name,
            "Azure optimization operations - Discover Azure Advisor cost-saving recommendations for a subscription, " +
            "compare alternative compute resize/SKU options, and explain a recommendation with current-versus-target " +
            "utilization projections. Requires Azure subscription context.",
            Title);

        var recommendation = new CommandGroup(
            "recommendation",
            "Cost optimization recommendations - List top cost-saving recommendations, get alternative compute options, " +
            "and explain a recommendation with utilization projections.");
        optimization.AddSubGroup(recommendation);

        recommendation.AddCommand<RecommendationListCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationAlternativesCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationExplainCommand>(serviceProvider);

        return optimization;
    }
}
