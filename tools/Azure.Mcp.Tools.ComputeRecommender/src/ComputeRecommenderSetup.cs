// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ComputeRecommender.Commands.PlacementScore;
using Azure.Mcp.Tools.ComputeRecommender.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ComputeRecommender;

public class ComputeRecommenderSetup : IAreaSetup
{
    public string Name => "computerecommender";

    public string Title => "Manage Azure Compute Placement Recommendations";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IComputeRecommenderService, ComputeRecommenderService>();

        services.AddSingleton<SpotPlacementMetadataCommand>();
        services.AddSingleton<SpotPlacementScoreCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var computeRecommender = new CommandGroup(Name,
            """
            Compute Recommendation operations - Commands for evaluating Azure compute placement scores and capacity
            recommendations. Use this tool to check Spot VM placement likelihood across regions and availability zones,
            discover supported resource types for placement scoring, and get capacity-aware deployment recommendations.
            Helps optimize VM and VMSS deployments by providing High/Medium/Low placement scores indicating
            allocation success probability. Covers Spot VM placement scores with quota availability checks.
            Do not use for general VM management (use compute tools), pricing information, or non-compute resources.
            """,
            Title);

        // Create placement score subgroup
        var placementScore = new CommandGroup(
            "placementscore",
            "Placement score operations - Commands for evaluating VM placement scores across Azure regions and availability zones.");
        computeRecommender.AddSubGroup(placementScore);

        // Create spot subgroup under placement scores
        var spot = new CommandGroup(
            "spot",
            "Spot VM placement score operations - Commands for evaluating Spot VM allocation likelihood and discovering supported resource types.");
        placementScore.AddSubGroup(spot);

        // Register commands
        var metadataCommand = serviceProvider.GetRequiredService<SpotPlacementMetadataCommand>();
        spot.AddCommand(metadataCommand.Name, metadataCommand);

        var scoreCommand = serviceProvider.GetRequiredService<SpotPlacementScoreCommand>();
        spot.AddCommand(scoreCommand.Name, scoreCommand);

        return computeRecommender;
    }
}
