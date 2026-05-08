// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Skills;
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

        // Agents + Skills (sub-agent A)
        services.AddSingleton<AgentsGetCommand>();
        services.AddSingleton<AgentsCreateCommand>();
        services.AddSingleton<AgentsDeleteCommand>();
        services.AddSingleton<AgentsToolsGetCommand>();
        services.AddSingleton<AgentsToolsCreateCommand>();
        services.AddSingleton<AgentToolsListCommand>();
        services.AddSingleton<SkillsDeleteCommand>();
        services.AddSingleton<SkillsListCommand>();
        services.AddSingleton<SkillsCreateCommand>();
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
        tools.AddCommand<AgentToolsListCommand>(serviceProvider);
        tools.AddCommand<AgentsToolsGetCommand>(serviceProvider);
        tools.AddCommand<AgentsToolsCreateCommand>(serviceProvider);
        tools.AddCommand<SkillsDeleteCommand>(serviceProvider);

        var skills = new CommandGroup(
            "skills",
            "SRE Agent skill operations - Commands for listing and managing custom skills on an SRE Agent resource.");
        sreAgent.AddSubGroup(skills);
        skills.AddCommand<SkillsListCommand>(serviceProvider);
        skills.AddCommand<SkillsCreateCommand>(serviceProvider);

        return sreAgent;
    }
}
