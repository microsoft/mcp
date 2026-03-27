// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;
using Azure.Mcp.Tools.ContainerApps.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ContainerApps;

public sealed class ContainerAppsRegistration : IAreaRegistration
{
    public static string AreaName => "containerapps";

    public static string AreaTitle => "Azure Container Apps Management";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Container Apps operations - Commands for managing Azure Container Apps resources. Includes operations for listing container apps and managing container app configurations.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f90",
                Name = "list",
                Description = "List Azure Container Apps in a subscription. Optionally filter by resource group. Each container app result includes: name, location, resourceGroup, managedEnvironmentId, provisioningState. If no container apps are found the tool returns an empty list of results (consistent with other list commands).",
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
                HandlerType = nameof(ContainerAppListCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IContainerAppsService, ContainerAppsService>();
        services.AddSingleton<ContainerAppListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ContainerAppListCommand) => serviceProvider.GetRequiredService<ContainerAppListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in containerapps area.")
        };
}
