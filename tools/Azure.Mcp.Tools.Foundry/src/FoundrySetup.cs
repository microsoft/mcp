// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Foundry;

public class FoundrySetup : IAreaSetup
{
    public string Name => "foundry";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFoundryService, FoundryService>();

        services.AddSingleton<DeploymentsListCommand>();
        services.AddSingleton<ModelsListCommand>();
        services.AddSingleton<ModelDeploymentCommand>();
        services.AddSingleton<KnowledgeIndexListCommand>();
        services.AddSingleton<KnowledgeIndexSchemaCommand>();

        services.AddSingleton<AgentsListCommand>();
        services.AddSingleton<AgentsConnectCommand>();
        services.AddSingleton<AgentsQueryAndEvaluateCommand>();
        services.AddSingleton<AgentsEvaluateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var foundry = new CommandGroup(Name, "Foundry service operations - Commands for listing and managing services and resources in AI Foundry.");

        var models = new CommandGroup("models", "Foundry models operations - Commands for listing and managing models in AI Foundry.");
        foundry.AddSubGroup(models);

        var deployments = new CommandGroup("deployments", "Foundry models deployments operations - Commands for listing and managing models deployments in AI Foundry.");
        models.AddSubGroup(deployments);

        deployments.AddCommand("list", serviceProvider.GetRequiredService<DeploymentsListCommand>());

        models.AddCommand("list", serviceProvider.GetRequiredService<ModelsListCommand>());
        models.AddCommand("deploy", serviceProvider.GetRequiredService<ModelDeploymentCommand>());

        var knowledge = new CommandGroup("knowledge", "Foundry knowledge operations - Commands for managing knowledge bases and indexes in AI Foundry.");
        foundry.AddSubGroup(knowledge);

        var index = new CommandGroup("index", "Foundry knowledge index operations - Commands for managing knowledge indexes in AI Foundry.");
        knowledge.AddSubGroup(index);

        index.AddCommand("list", serviceProvider.GetRequiredService<KnowledgeIndexListCommand>());
        index.AddCommand("schema", serviceProvider.GetRequiredService<KnowledgeIndexSchemaCommand>());

        var agents = new CommandGroup("agents", "Foundry agents operations - Commands for listing, querying, and evaluating agents in AI Foundry.");
        foundry.AddSubGroup(agents);

        agents.AddCommand("list", serviceProvider.GetRequiredService<AgentsListCommand>());
        agents.AddCommand("connect", serviceProvider.GetRequiredService<AgentsConnectCommand>());
        agents.AddCommand("query-and-evaluate", serviceProvider.GetRequiredService<AgentsQueryAndEvaluateCommand>());
        agents.AddCommand("evaluate", serviceProvider.GetRequiredService<AgentsEvaluateCommand>());

        return foundry;
    }
}
