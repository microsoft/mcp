// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Commands.AppTrace;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.ApplicationInsights;

public class ApplicationInsightsSetup : IAreaSetup
{
    public string Name => "applicationinsights";

    public string Title => "Azure Application Insights";

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
        services.AddSingleton<AppTraceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(Name,
        """
        Application Insights operations - Commands for listing and managing Application Insights components. 
        These commands do not support querying metrics or logs. Use Azure Monitor querying tools for that purpose.
        """,
         Title);

        var recommendation = new CommandGroup("recommendation", "Application Insights recommendation operations - list recommendation targets (components).");
        group.AddSubGroup(recommendation);

        var appTrace = new CommandGroup("apptrace", "Application Insights trace operations - list trace metadata for components.");
        group.AddSubGroup(appTrace);

        var recommendationList = serviceProvider.GetRequiredService<RecommendationListCommand>();
        recommendation.AddCommand(recommendationList.Name, recommendationList);

        var appTraceList = serviceProvider.GetRequiredService<AppTraceListCommand>();
        appTrace.AddCommand(appTraceList.Name, appTraceList);

        return group;
    }
}
