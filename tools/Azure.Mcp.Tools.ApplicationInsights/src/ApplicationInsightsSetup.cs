// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.ApplicationInsights;

public class ApplicationInsightsSetup : IAreaSetup
{
    public string Name => "applicationinsights";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClientServices(options =>
        {
            options.DefaultUserAgent = "AzureMCPClient/1.0";
        });

        // Service for accessing Profiler dataplane.
        services.AddSingleton<IProfilerDataService, ProfilerDataService>();

        services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();

        services.AddSingleton<RecommendationListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(Name, "Application Insights operations - Commands for listing and managing Application Insights components.");

        var recommendation = new CommandGroup("recommendation", "Application Insights recommendation operations - list recommendation targets (components).");
        group.AddSubGroup(recommendation);

        var recommendationList = serviceProvider.GetRequiredService<RecommendationListCommand>();
        recommendation.AddCommand(recommendationList.Name, recommendationList);

        return group;
    }
}
