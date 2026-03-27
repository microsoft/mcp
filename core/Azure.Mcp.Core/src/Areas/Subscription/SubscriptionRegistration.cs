// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Core.Areas.Subscription.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Core.Areas.Subscription;

public sealed class SubscriptionRegistration : IAreaRegistration
{
    public static string AreaName => "subscription";

    public static string AreaTitle => "Azure Subscriptions Management";

    public static CommandCategory Category => CommandCategory.SubscriptionManagement;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure subscription operations - Commands for listing and managing Azure subscriptions accessible to your account.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "72bbe80e-ca42-4a43-8f02-45495bca1179",
                Name = "list",
                Description = "List all Azure subscriptions for the current account. Returns subscriptionId, displayName, state, tenantId, and isDefault for each subscription. The isDefault field indicates the user's default subscription as resolved from the Azure CLI profile (configured via 'az account set') or, if not set there, from the AZURE_SUBSCRIPTION_ID environment variable. When the user has not specified a subscription, prefer the subscription where isDefault is true. If no default can be determined from either source and multiple subscriptions exist, ask the user which subscription to use.",
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
                Options = [],
                Kind = CommandKind.Global,
                HandlerType = nameof(SubscriptionListCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<SubscriptionListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(SubscriptionListCommand) => serviceProvider.GetRequiredService<SubscriptionListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in subscription area.")
        };
}
