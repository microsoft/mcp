// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.VirtualDesktop;

public sealed class VirtualDesktopRegistration : IAreaRegistration
{
    public static string AreaName => "virtualdesktop";

    public static string AreaTitle => "Azure Virtual Desktop";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Virtual Desktop operations - Commands for managing and accessing Azure Virtual Desktop resources. Includes operations for hostpools, session hosts, and user sessions.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "hostpool",
                Description = "Hostpool operations - Commands for listing and managing Hostpools, including listing and changing settings on hostpools.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "bf0ae005-7dfd-4f96-8f45-3d0ba07f81ed",
                        Name = "list",
                        Description = "List all hostpools in a subscription or resource group. This command retrieves all Azure Virtual Desktop hostpool objects available in the specified Option`1: --subscription. If a resource group is specified, only hostpools in that resource group are returned. Results include hostpool names and are returned as a JSON array.",
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
                        HandlerType = nameof(HostpoolListCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "host",
                        Description = "Sessionhost operations - Commands for listing and managing session hosts inside a host pool.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "6f543101-3c70-41bd-a6ed-5cc4af716081",
                                Name = "list",
                                Description = "List all SessionHosts in a hostpool. This command retrieves all Azure Virtual Desktop SessionHost objects available in the specified --subscription and hostpool. Results include SessionHost details and are returned as a JSON array.",
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
                                        Name = "hostpool",
                                        Description = "The name of the Azure Virtual Desktop host pool. This is the unique name you chose for your hostpool.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "hostpool-resource-id",
                                        Description = "The Azure resource ID of the host pool. When provided, this will be used instead of searching by name.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(HostpoolListCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "1653a208-ac9f-4e51-996f-fe2d29a79b2b",
                                Name = "user-list",
                                Description = "List all user sessions on a specific session host in a host pool. This command retrieves all Azure Virtual Desktop user session objects available on the specified session host. Results include user session details such as user principal name, session state, application type, and creation time.",
                                Title = "User List",
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
                                        Name = "hostpool",
                                        Description = "The name of the Azure Virtual Desktop host pool. This is the unique name you chose for your hostpool.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "hostpool-resource-id",
                                        Description = "The Azure resource ID of the host pool. When provided, this will be used instead of searching by name.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "sessionhost",
                                        Description = "The name of the session host. This is the computer name of the virtual machine in the host pool.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(SessionHostUserSessionListCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IVirtualDesktopService, VirtualDesktopService>();
        services.AddSingleton<HostpoolListCommand>();
        services.AddSingleton<SessionHostListCommand>();
        services.AddSingleton<SessionHostUserSessionListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(HostpoolListCommand) => serviceProvider.GetRequiredService<HostpoolListCommand>(),
            nameof(SessionHostUserSessionListCommand) => serviceProvider.GetRequiredService<SessionHostUserSessionListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in virtualdesktop area.")
        };
}
