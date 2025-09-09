// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Subscription.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Core.Areas.Subscription;

public class SubscriptionSetup : IAreaSetup
{
    public string Name => "subscription";

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Subscription command group
        var subscription = new CommandGroup(Name, "Azure subscription operations - Commands for listing and managing Azure subscriptions accessible to your account.");
        rootGroup.AddSubGroup(subscription);

        // Register Subscription commands
        subscription.AddCommand("list", new SubscriptionListCommand(loggerFactory.CreateLogger<SubscriptionListCommand>()));
    }
}
