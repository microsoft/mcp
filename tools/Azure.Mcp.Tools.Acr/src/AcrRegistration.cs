// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Acr;

public sealed class AcrRegistration : IAreaRegistration
{
    public static string AreaName => "acr";

    public static string AreaTitle => "Azure Container Registry Services";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Container Registry operations - Commands for managing Azure Container Registry resources. Includes operations for listing container registries and managing registry configurations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "registry",
                Description = "Container Registry resource operations - Commands for listing and managing Container Registry resources in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "796f8778-2fa7-4343-87ad-06bdcf6b296c",
                        Name = "list",
                        Description = "List Azure Container Registries in a subscription. Optionally filter by resource group. Each registry result includes: name, location, loginServer, skuName, skuTier. If no registries are found the tool returns null results (consistent with other list commands).",
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
                        HandlerType = nameof(RegistryListCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "repository",
                        Description = "Container Registry repository operations - Commands for listing and managing repositories within a Container Registry.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "adc6eb20-ad98-4624-954d-61581f6fbca9",
                                Name = "list",
                                Description = "List repositories in Azure Container Registries. By default, lists repositories for all registries in the subscription. You can narrow the scope using --resource-group and/or --registry to list repositories for a specific registry only.",
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
                                    new OptionDescriptor
                                    {
                                        Name = "registry",
                                        Description = "The name of the Azure Container Registry. This is the unique name you chose for your container registry.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(RegistryListCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAcrService, AcrService>();
        services.AddSingleton<RegistryListCommand>();
        services.AddSingleton<RegistryRepositoryListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(RegistryListCommand) => serviceProvider.GetRequiredService<RegistryListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in acr area.")
        };
}
