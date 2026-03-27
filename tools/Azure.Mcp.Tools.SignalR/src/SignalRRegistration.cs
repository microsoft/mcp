// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.SignalR;

public sealed class SignalRRegistration : IAreaRegistration
{
    public static string AreaName => "signalr";

    public static string AreaTitle => "Azure SignalR Service";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure SignalR operations - Commands for managing Azure SignalR Service resources. Includes operations for listing SignalR services.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "runtime",
                Description = "Runtime operations - Commands for managing Azure SignalR Service resources.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "bb9035f6-f642-4ee0-83c8-87d6da8266b1",
                        Name = "get",
                        Description = "Gets or lists details of an Azure SignalR Runtimes. If a specific SignalR name is used, the details of that SignalR runtime will be retrieved. Otherwise, all SignalR Runtimes in the specified subscription or resource group will be retrieved. Returns runtime information including identity, network ACLs, upstream templates.",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "signalr",
                                Description = "The name of the SignalR runtime",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(RuntimeGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<RuntimeGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(RuntimeGetCommand) => serviceProvider.GetRequiredService<RuntimeGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in signalr area.")
        };
}
