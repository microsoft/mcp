// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Connectors;
using Azure.Mcp.Tools.SreAgent.Commands.Hooks;
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
            "SRE Agent hook operations - Commands for listing, deleting, and activating hooks for threads.");
        sreAgent.AddSubGroup(hooks);
        hooks.AddCommand<HooksListCommand>(serviceProvider);
        hooks.AddCommand<HooksGetCommand>(serviceProvider);
        hooks.AddCommand<HooksDeleteCommand>(serviceProvider);
        hooks.AddCommand<HooksThreadListCommand>(serviceProvider);
        hooks.AddCommand<HooksThreadActivateCommand>(serviceProvider);
        hooks.AddCommand<HooksThreadDeactivateCommand>(serviceProvider);

        return sreAgent;
    }
}

