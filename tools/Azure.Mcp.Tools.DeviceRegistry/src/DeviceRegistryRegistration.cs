// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.DeviceRegistry;

public sealed class DeviceRegistryRegistration : IAreaRegistration
{
    public static string AreaName => "deviceregistry";

    public static string AreaTitle => "Manage Azure Device Registry";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Manage Azure Device Registry operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "namespace",
                Description = "Device Registry namespace operations - Commands for listing and managing Device Registry namespaces.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "9c42f93b-2d4e-4fb3-b98b-2ef119b46c94",
                        Name = "list",
                        Description = "Lists Azure Device Registry namespaces in a subscription or resource group. Returns namespace details including name, location, provisioning state, and UUID. If a resource group is specified, only namespaces within that resource group are returned. Otherwise, all namespaces in the subscription are listed.",
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
                        HandlerType = nameof(NamespaceListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IDeviceRegistryService, DeviceRegistryService>();
        services.AddSingleton<NamespaceListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(NamespaceListCommand) => serviceProvider.GetRequiredService<NamespaceListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in deviceregistry area.")
        };
}
