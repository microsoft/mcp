// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventGrid;

public class EventGridSetup : IAreaSetup
{
    public string Name => "eventgrid";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventGridService, EventGridService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Event Grid top-level group
        var eventGrid = new CommandGroup(Name, "Event Grid operations - Commands for managing and accessing Event Grid topics, domains, and event subscriptions.");
        rootGroup.AddSubGroup(eventGrid);

        // Topics subgroup
        var topics = new CommandGroup("topic", "Event Grid topic operations - Commands for managing Event Grid topics and their configurations.");
        eventGrid.AddSubGroup(topics);

        // Register Topic commands
        topics.AddCommand("list", new TopicListCommand(loggerFactory.CreateLogger<TopicListCommand>()));
    }
}
