// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ApplicationInsights;

public sealed class ApplicationInsightsRegistration : IAreaRegistration
{
    public static string AreaName => "applicationinsights";

    public static string AreaTitle => "Azure Application Insights";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Application Insights operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "recommendation",
                Description = "Application Insights recommendation operations - list recommendation targets (components).",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "8d259f21-43b3-4962-bec8-de616b8b5f0d",
                        Name = "list",
                        Description = "List Application Insights Code Optimization Recommendations in a subscription. Optionally filter by resource group when --resource-group is provided. Returns the code optimization recommendations based on the profiler data.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(RecommendationListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        // Service for accessing Profiler dataplane.
        services.AddSingleton<IProfilerDataService, ProfilerDataService>();
        services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();
        services.AddSingleton<RecommendationListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(RecommendationListCommand) => serviceProvider.GetRequiredService<RecommendationListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in applicationinsights area.")
        };
}
