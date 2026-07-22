// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Architecture;
using Azure.Mcp.Tools.SreAgent.Commands.CommonPrompts;
using Azure.Mcp.Tools.SreAgent.Commands.Connectors;
using Azure.Mcp.Tools.SreAgent.Commands.Docs;
using Azure.Mcp.Tools.SreAgent.Commands.Hooks;
using Azure.Mcp.Tools.SreAgent.Commands.Incidents;
using Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Commands.Skills;
using Azure.Mcp.Tools.SreAgent.Commands.Threads;
using Azure.Mcp.Tools.SreAgent.Commands.Workflows;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
namespace Azure.Mcp.Tools.SreAgent;

public sealed class SreAgentSetup : IAreaSetup
{
    public string Name => "sreagent";

    public string Title => "Azure SRE Agent";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISreAgentService, SreAgentService>();

        services.AddSingleton<AgentsListCommand>();

        // Agents + Skills (sub-agent A)
        services.AddSingleton<AgentsGetCommand>();
        services.AddSingleton<AgentsCreateCommand>();
        services.AddSingleton<AgentsDeleteCommand>();
        services.AddSingleton<AgentsToolsGetCommand>();
        services.AddSingleton<AgentsToolsCreateCommand>();
        services.AddSingleton<AgentsToolsListCommand>();
        services.AddSingleton<SkillsDeleteCommand>();
        services.AddSingleton<SkillsListCommand>();
        services.AddSingleton<SkillsCreateCommand>();

        // Connectors + Hooks (sub-agent B)
        services.AddSingleton<ConnectorsListCommand>();
        services.AddSingleton<ConnectorsGetCommand>();
        services.AddSingleton<ConnectorsCreateKustoCommand>();
        services.AddSingleton<ConnectorsCreateMcpCommand>();
        services.AddSingleton<ConnectorsDeleteCommand>();
        services.AddSingleton<ConnectorsTestCommand>();
        services.AddSingleton<HooksListCommand>();
        services.AddSingleton<HooksGetCommand>();
        services.AddSingleton<HooksDeleteCommand>();
        services.AddSingleton<HooksThreadListCommand>();
        services.AddSingleton<HooksThreadActivateCommand>();
        services.AddSingleton<HooksThreadDeactivateCommand>();

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

        // Incidents + Workflows + Docs + Architecture (sub-agent D)
        services.AddSingleton<IncidentsPlansListCommand>();
        services.AddSingleton<IncidentsPlansCreateCommand>();
        services.AddSingleton<IncidentsActiveListCommand>();
        services.AddSingleton<IncidentsCreateCommand>();
        services.AddSingleton<IncidentsSetupPagerdutyCommand>();
        services.AddSingleton<IncidentsSetupServicenowCommand>();
        services.AddSingleton<WorkflowsGenerateCommand>();
        services.AddSingleton<WorkflowsApplyCommand>();
        services.AddSingleton<WorkflowsValidateCommand>();
        services.AddSingleton<DocsGetCommand>();
        services.AddSingleton<MemoriesListCommand>();
        services.AddSingleton<MemoriesSearchCommand>();
        services.AddSingleton<MemoriesAddCommand>();
        services.AddSingleton<MemoriesDeleteCommand>();
        services.AddSingleton<MemoriesReindexCommand>();
        services.AddSingleton<PlanCommand>();

        // Common Prompts
        services.AddSingleton<CommonPromptsListCommand>();
        services.AddSingleton<CommonPromptsGetCommand>();
        services.AddSingleton<CommonPromptsCreateCommand>();
        services.AddSingleton<CommonPromptsDeleteCommand>();
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

        // Agents + Skills (sub-agent A)
        agents.AddCommand<AgentsGetCommand>(serviceProvider);
        agents.AddCommand<AgentsCreateCommand>(serviceProvider);
        agents.AddCommand<AgentsDeleteCommand>(serviceProvider);

        var tools = new CommandGroup(
            "tools",
            "SRE Agent custom tool operations - Commands for listing and managing custom tools on an SRE Agent resource.");
        agents.AddSubGroup(tools);
        tools.AddCommand<AgentsToolsListCommand>(serviceProvider);
        tools.AddCommand<AgentsToolsGetCommand>(serviceProvider);
        tools.AddCommand<AgentsToolsCreateCommand>(serviceProvider);

        var skills = new CommandGroup(
            "skills",
            "SRE Agent skill operations - Commands for listing and managing custom skills on an SRE Agent resource.");
        sreAgent.AddSubGroup(skills);
        skills.AddCommand<SkillsListCommand>(serviceProvider);
        skills.AddCommand<SkillsCreateCommand>(serviceProvider);
        skills.AddCommand<SkillsDeleteCommand>(serviceProvider);

        // Connectors + Hooks (sub-agent B)
        var connectors = new CommandGroup(
            "connectors",
            "SRE Agent connector operations - Commands for listing, creating, deleting, and testing SRE Agent connectors.");
        sreAgent.AddSubGroup(connectors);
        connectors.AddCommand<ConnectorsListCommand>(serviceProvider);
        connectors.AddCommand<ConnectorsGetCommand>(serviceProvider);
        connectors.AddCommand<ConnectorsCreateKustoCommand>(serviceProvider);
        connectors.AddCommand<ConnectorsCreateMcpCommand>(serviceProvider);
        connectors.AddCommand<ConnectorsDeleteCommand>(serviceProvider);
        connectors.AddCommand<ConnectorsTestCommand>(serviceProvider);

        var hooks = new CommandGroup(
            "hooks",
            "SRE Agent hook operations - Commands for listing, retrieving, deleting, and managing thread-level activation of safety hooks.");
        sreAgent.AddSubGroup(hooks);
        hooks.AddCommand<HooksListCommand>(serviceProvider);
        hooks.AddCommand<HooksGetCommand>(serviceProvider);
        hooks.AddCommand<HooksDeleteCommand>(serviceProvider);

        var hooksThread = new CommandGroup(
            "thread",
            "SRE Agent hook thread activation - Commands for listing and toggling on-demand hooks for a specific thread.");
        hooks.AddSubGroup(hooksThread);
        hooksThread.AddCommand<HooksThreadListCommand>(serviceProvider);
        hooksThread.AddCommand<HooksThreadActivateCommand>(serviceProvider);
        hooksThread.AddCommand<HooksThreadDeactivateCommand>(serviceProvider);

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
            "SRE Agent scheduled task operations - Commands for listing, creating, deleting, pausing, and resuming agent runs on a cron schedule.");
        sreAgent.AddSubGroup(scheduledTasks);
        scheduledTasks.AddCommand<ScheduledTasksListCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksGetCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksCreateCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksDeleteCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksPauseCommand>(serviceProvider);
        scheduledTasks.AddCommand<ScheduledTasksResumeCommand>(serviceProvider);

        // Incidents + Workflows + Docs + Architecture (sub-agent D)
        var incidents = new CommandGroup("incidents", "Incident response planning, connector setup, and active incident operations.");
        var workflows = new CommandGroup("workflows", "Generate, validate, and apply SRE Agent workflow YAML.");
        var docs = new CommandGroup("docs", "SRE Agent documentation and knowledge memory operations.");
        var architecture = new CommandGroup("architecture", "SRE Agent architecture planning commands.");
        sreAgent.AddSubGroup(incidents);
        sreAgent.AddSubGroup(workflows);
        sreAgent.AddSubGroup(docs);
        sreAgent.AddSubGroup(architecture);

        var incidentsActive = new CommandGroup("active", "Active incident listing and management.");
        incidents.AddSubGroup(incidentsActive);
        incidentsActive.AddCommand<IncidentsActiveListCommand>(serviceProvider);

        incidents.AddCommand<IncidentsCreateCommand>(serviceProvider);
        incidents.AddCommand<IncidentsPlansListCommand>(serviceProvider);
        incidents.AddCommand<IncidentsPlansCreateCommand>(serviceProvider);
        incidents.AddCommand<IncidentsSetupPagerdutyCommand>(serviceProvider);
        incidents.AddCommand<IncidentsSetupServicenowCommand>(serviceProvider);

        workflows.AddCommand<WorkflowsGenerateCommand>(serviceProvider);
        workflows.AddCommand<WorkflowsValidateCommand>(serviceProvider);
        workflows.AddCommand<WorkflowsApplyCommand>(serviceProvider);

        docs.AddCommand<DocsGetCommand>(serviceProvider);
        docs.AddCommand<MemoriesListCommand>(serviceProvider);
        docs.AddCommand<MemoriesSearchCommand>(serviceProvider);
        docs.AddCommand<MemoriesAddCommand>(serviceProvider);
        docs.AddCommand<MemoriesDeleteCommand>(serviceProvider);
        docs.AddCommand<MemoriesReindexCommand>(serviceProvider);

        architecture.AddCommand<PlanCommand>(serviceProvider);

        var commonPrompts = new CommandGroup("commonprompts", "SRE Agent common prompts: list, get, create or update, and delete reusable prompts.");
        sreAgent.AddSubGroup(commonPrompts);
        commonPrompts.AddCommand<CommonPromptsListCommand>(serviceProvider);
        commonPrompts.AddCommand<CommonPromptsGetCommand>(serviceProvider);
        commonPrompts.AddCommand<CommonPromptsCreateCommand>(serviceProvider);
        commonPrompts.AddCommand<CommonPromptsDeleteCommand>(serviceProvider);

        return sreAgent;
    }
}
