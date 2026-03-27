// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Advisor;

public sealed class AdvisorRegistration : IAreaRegistration
{
    public static string AreaName => "advisor";

    public static string AreaTitle => "Azure Advisor Recommendations";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Advisor operations - Manage and query Azure Advisor recommendations across subscriptions. Use when you need subscription-scoped visibility into Advisor recommendations. Requires Azure subscription context.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "recommendation",
                Description = "Advisor recommendations - Commands for listing Advisor recommendations in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "e3f09221-523a-4107-a715-823cebd97902",
                        Name = "list",
                        Description = "List Azure advisor recommendations in a subscription.",
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
        services.AddSingleton<IAdvisorService, AdvisorService>();
        services.AddSingleton<RecommendationListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(RecommendationListCommand) => serviceProvider.GetRequiredService<RecommendationListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in advisor area.")
        };
}
