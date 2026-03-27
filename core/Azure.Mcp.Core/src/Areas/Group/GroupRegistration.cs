// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Core.Areas.Group;

/// <summary>
/// Descriptor-based registration for the Group area. Provides command metadata
/// statically without requiring DI or handler instantiation.
/// </summary>
public sealed class GroupRegistration : IAreaRegistration
{
    public static string AreaName => "group";

    public static string AreaTitle => "Azure Resource Groups";

    public static CommandCategory Category => CommandCategory.SubscriptionManagement;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Resource group operations - Commands for listing and managing Azure resource groups and their resources in your subscriptions.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "a0049f31-9a32-4b5e-91ec-e7b074fc7246",
                Name = "list",
                Description = $"""
                    List all resource groups in a subscription. This command retrieves all resource groups available
                    in the specified {OptionDefinitions.Common.SubscriptionName}. Results include resource group names and IDs,
                    returned as a JSON array.
                    """,
                Title = "List Resource Groups",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false
                },
                Options = [],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(GroupListCommand)
            }
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "resource",
                Description = "Resource operations - Commands for listing resources within a resource group.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b1c2d3e4-f5a6-7890-abcd-ef1234567890",
                        Name = "list",
                        Description = $"""
                            List all resources in a resource group. This command retrieves all resources available
                            in the specified {OptionDefinitions.Common.ResourceGroupName} within the given
                            {OptionDefinitions.Common.SubscriptionName}. Results include resource names, IDs, types,
                            and locations. The command returns a JSON object with a `resources` array containing these entries.
                            """,
                        Title = "List Resources in Resource Group",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = OptionDefinitions.Common.ResourceGroupName,
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true
                            }
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ResourceListCommand)
                    }
                ]
            }
        ]
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<GroupListCommand>();
        services.AddSingleton<ResourceListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(GroupListCommand) => serviceProvider.GetRequiredService<GroupListCommand>(),
            nameof(ResourceListCommand) => serviceProvider.GetRequiredService<ResourceListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{handlerTypeName}' in Group area.")
        };
}
