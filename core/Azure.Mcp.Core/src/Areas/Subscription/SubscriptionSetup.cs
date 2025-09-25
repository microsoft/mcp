// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Subscription.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas.Subscription;

public class SubscriptionSetup : IAreaSetup
{
    public string Name => "subscription";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SubscriptionListCommand>();
    }


    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Subscription command group
        var subscription = new CommandGroup(Name, "Azure subscription operations - Commands for listing and managing Azure subscriptions accessible to your account.");

        // Register Subscription commands
        var subscriptionListCommand = serviceProvider.GetRequiredService<SubscriptionListCommand>();
        subscription.AddCommand(subscriptionListCommand.Name, subscriptionListCommand);

        return subscription;
    }
}
