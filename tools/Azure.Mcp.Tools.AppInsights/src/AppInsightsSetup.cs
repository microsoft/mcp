// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppInsights.Commands.Recommendation;
using Azure.Mcp.Tools.AppInsights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppInsights;

public class AppInsightsSetup : IAreaSetup
{
    public string Name => "appinsights";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppInsightsService, AppInsightsService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var group = new CommandGroup(Name, "Application Insights operations - Commands for listing and managing Application Insights components.");
        rootGroup.AddSubGroup(group);

        var recommendation = new CommandGroup("recommendation", "Application Insights recommendation operations - list recommendation targets (components).");
        group.AddSubGroup(recommendation);

        recommendation.AddCommand("list", new RecommendationListCommand(loggerFactory.CreateLogger<RecommendationListCommand>()));
    }
}
