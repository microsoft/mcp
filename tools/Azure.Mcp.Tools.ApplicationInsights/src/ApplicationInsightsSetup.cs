// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ApplicationInsights;

public class ApplicationInsightsSetup : IAreaSetup
{
    public string Name => "applicationinsights"; // Renamed from 'appinsights'

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();
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
