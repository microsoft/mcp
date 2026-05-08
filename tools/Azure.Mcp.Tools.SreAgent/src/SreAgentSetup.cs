// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Commands.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.SreAgent;

public class SreAgentSetup : IAreaSetup
{
    public string Name => "sreagent";

    public string Title => "Azure SRE Agent";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISreAgentService, SreAgentService>();

        services.AddSingleton<AgentsListCommand>();

        // Threads + ScheduledTasks (sub-agent C)
        services.AddSingleton<ThreadsListCommand>();
        services.AddSingleton<ThreadsGetCommand>();
        services.AddSingleton<ThreadsCreateCommand>();
        services.AddSingleton<ThreadsSendMessageCommand>();
        services.AddSingleton<ThreadsDeleteCommand>();
        services.AddSingleton<ThreadsInvestigateCommand>();
        services.AddSingleton<ThreadsInvestigateYoloCommand>();
        services.AddSingleton<ScheduledTasksListCommand>();
        services.AddSingleton<ScheduledTasksGetCommand>();
        services.AddSingleton<ScheduledTasksCreateCommand>();
        services.AddSingleton<ScheduledTasksDeleteCommand>();
        services.AddSingleton<ScheduledTasksPauseCommand>();
        services.AddSingleton<ScheduledTasksResumeCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var sreAgent = new CommandGroup(
            Name,
            "Azure SRE Agent operations - Commands for managing and interacting with Azure SRE Agent resources, including agents, skills, connectors, threads, hooks, scheduled tasks, incidents, knowledge memory, documentation, workflows, and architecture planning.",
            Title);

        var agents = new CommandGroup(
            "agents",
            "SRE Agent resource operations - Commands for listing and managing SRE Agent resources in your Azure subscription.");
        sreAgent.AddSubGroup(agents);

        agents.AddCommand<AgentsListCommand>(serviceProvider);

        // Threads + ScheduledTasks (sub-agent C)
        var threads = new CommandGroup(
            "threads",
            "SRE Agent thread operations - Commands for listing, reading, creating, messaging, deleting, and running investigations.");
        sreAgent.AddSubGroup(threads);
        threads.AddCommand<ThreadsListCommand>(serviceProvider);
        threads.AddCommand<ThreadsGetCommand>(serviceProvider);
        threads.AddCommand<ThreadsCreateCommand>(serviceProvider);
        threads.AddCommand<ThreadsSendMessageCommand>(serviceProvider);
        threads.AddCommand<ThreadsDeleteCommand>(serviceProvider);
        threads.AddCommand<ThreadsInvestigateCommand>(serviceProvider);
        threads.AddCommand<ThreadsInvestigateYoloCommand>(serviceProvider);

        var scheduledTasks = new CommandGroup(
            "scheduledtasks",
            "SRE Agent scheduled task operations - Commands for listing, reading, creating, deleting, pausing, and resuming scheduled tasks.");
        sreAgent.AddSubGroup(scheduledTasks);
        scheduledTasks.AddCommand<ScheduledTasksListCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksGetCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksCreateCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksDeleteCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksPauseCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksResumeCommand>(serviceProvider);

        return sreAgent;
    }
}
