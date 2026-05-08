// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Architecture;
using Azure.Mcp.Tools.SreAgent.Commands.Docs;
using Azure.Mcp.Tools.SreAgent.Commands.Incidents;
using Azure.Mcp.Tools.SreAgent.Commands.Workflows;
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

        // Incidents + Workflows + Docs + Architecture (sub-agent D)
        var incidents = new CommandGroup("incidents", "Incident response planning, connector setup, and active incident operations.");
        var workflows = new CommandGroup("workflows", "Generate, validate, and apply SRE Agent workflow YAML.");
        var docs = new CommandGroup("docs", "SRE Agent documentation and knowledge memory operations.");
        var architecture = new CommandGroup("architecture", "SRE Agent architecture planning commands.");
        sreAgent.AddSubGroup(incidents);
        sreAgent.AddSubGroup(workflows);
        sreAgent.AddSubGroup(docs);
        sreAgent.AddSubGroup(architecture);

        incidents.AddCommand<IncidentsPlansListCommand>(serviceProvider);
        incidents.AddCommand<IncidentsPlansCreateCommand>(serviceProvider);
        incidents.AddCommand<IncidentsActiveListCommand>(serviceProvider);
        incidents.AddCommand<IncidentsCreateCommand>(serviceProvider);
        incidents.AddCommand<IncidentsSetupPagerdutyCommand>(serviceProvider);
        incidents.AddCommand<IncidentsSetupServicenowCommand>(serviceProvider);

        workflows.AddCommand<WorkflowsGenerateCommand>(serviceProvider);
        workflows.AddCommand<WorkflowsApplyCommand>(serviceProvider);
        workflows.AddCommand<WorkflowsValidateCommand>(serviceProvider);

        docs.AddCommand<DocsGetCommand>(serviceProvider);
        docs.AddCommand<MemoriesListCommand>(serviceProvider);
        docs.AddCommand<MemoriesSearchCommand>(serviceProvider);
        docs.AddCommand<MemoriesAddCommand>(serviceProvider);
        docs.AddCommand<MemoriesDeleteCommand>(serviceProvider);
        docs.AddCommand<MemoriesReindexCommand>(serviceProvider);

        architecture.AddCommand<PlanCommand>(serviceProvider);

        return sreAgent;
    }
}
