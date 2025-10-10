// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Services.Azure.Resource;
using Azure.Mcp.Tools.ApplicationInsights.Commands.AppTrace;
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
        services.AddSingleton<IResourceResolverService, ResourceResolverService>();
        services.AddSingleton<IKQLQueryBuilder>(_ => KQLQueryBuilder.Instance);
        services.AddSingleton<IAppLogsQueryService, AppLogsQueryService>();
        services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();

        services.AddSingleton<RecommendationListCommand>();
        services.AddSingleton<AppTraceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(Name, "Application Insights operations - Commands for diagnosing problems with applications monitored with Application Insights.");

        var recommendationGroup = new CommandGroup("recommendation", "Application Insights recommendation operations - list recommendation targets (components).");
        group.AddSubGroup(recommendationGroup);

        var appInsightsGroup = new CommandGroup("apptrace", "Application Insights trace operations - list traces and spans for components.");
        group.AddSubGroup(appInsightsGroup);

        var recommendationList = serviceProvider.GetRequiredService<RecommendationListCommand>();
        recommendationGroup.AddCommand(recommendationList.Name, recommendationList);

        var appTraceList = serviceProvider.GetRequiredService<AppTraceListCommand>();
        appInsightsGroup.AddCommand(appTraceList.Name, appTraceList);

        return group;
    }
}
