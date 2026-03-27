// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Policy;

public sealed class PolicyRegistration : IAreaRegistration
{
    public static string AreaName => "policy";

    public static string AreaTitle => "Azure Policy Management";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Manage Azure Policy assignments and definitions using Azure CLI. Retrieve policy assignments, view enforcement modes, and analyze policy compliance across subscriptions.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "assignment",
                Description = "Policy assignment operations - Commands for getting and managing Azure Policy assignments.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b7c4d3e2-0f1a-4b8c-9d6e-5a7b8c9d0e1f",
                        Name = "list",
                        Description = "List policy assignments in a subscription or scope. This command retrieves all Azure Policy assignments along with their complete policy definition details (rules, effects, parameters schema), enforcement modes, assignment parameters, and metadata. This enables agents to understand policy requirements and design compliant cloud services. You can optionally filter by scope to list assignments at a specific resource group, resource, or management group level.",
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
                                Name = "scope",
                                Description = "The scope of the policy assignment (e.g., /subscriptions/{subscriptionId}, /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}).",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(PolicyAssignmentListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IPolicyService, PolicyService>();
        services.AddSingleton<PolicyAssignmentListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(PolicyAssignmentListCommand) => serviceProvider.GetRequiredService<PolicyAssignmentListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in policy area.")
        };
}
