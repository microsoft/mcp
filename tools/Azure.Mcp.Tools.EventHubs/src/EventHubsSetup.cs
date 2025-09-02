// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs;

public class EventHubsSetup : IAreaSetup
{
    public string Name => "eventhubs";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventHubsService, EventHubsService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var eventHubs = new CommandGroup(Name, "Azure EventHubs operations - Commands for managing Azure EventHubs namespaces and event hubs. Includes operations for listing namespaces in resource groups.");
        rootGroup.AddSubGroup(eventHubs);

        var namespaceGroup = new CommandGroup("namespace", "EventHubs namespace operations");
        eventHubs.AddSubGroup(namespaceGroup);

        namespaceGroup.AddCommand("list", new NamespaceListCommand(loggerFactory.CreateLogger<NamespaceListCommand>()));
    }
}
