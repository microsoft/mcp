// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.WellArchitectedFramework;

public sealed class WellArchitectedFrameworkRegistration : IAreaRegistration
{
    public static string AreaName => "wellarchitectedframework";

    public static string AreaTitle => "Azure Well-Architected Framework";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Well-Architected Framework operations - Commands for accessing guidance, best practices, and recommendations based on the Azure Well-Architected Framework pillars (reliability, security, cost optimization, operational excellence, and performance efficiency).",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "serviceguide",
                Description = "Service guide operations - Commands for retrieving Azure Well-Architected Framework service-specific guidance and recommendations.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a7d4e9f2-8c3b-4a1e-9f5d-6b2c8e4a7d3f",
                        Name = "get",
                        Description = "Get Azure Well-Architected Framework guidance for a specific Azure service, or list all supported services when no service is specified. When a service is provided, returns architectural best practices, design patterns, and recommendations based on the five pillars: reliability, security, cost optimization, operational excellence, and performance efficiency. Optional: --service: A single Azure service name. Service name format: case-insensitive; hyphens, underscores, spaces, and name variations allowed; use double quotes (not single quotes) for names with spaces. e.g., cosmos-db, Cosmos_DB, \"Cosmos DB\", cosmosdb, cosmos-database, cosmosdatabase",
                        Title = "Get",
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
                                Name = "service",
                                Description = "A single Azure service name. Service name format: case-insensitive; hyphens, underscores, spaces, and name variations allowed; use double quotes (not single quotes) for names with spaces. e.g., cosmos-db, Cosmos_DB, \"Cosmos DB\", cosmosdb, cosmos-database, cosmosdatabase",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(ServiceGuideGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceGuideService, ServiceGuideService>();
        services.AddSingleton<ServiceGuideGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ServiceGuideGetCommand) => serviceProvider.GetRequiredService<ServiceGuideGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in wellarchitectedframework area.")
        };
}
