// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Foundry;

public class FoundrySetup : IAreaSetup
{
    public string Name => "foundry";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFoundryService, FoundryService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var foundry = new CommandGroup(Name, "Foundry service operations - Commands for listing and managing services and resources in AI Foundry.");
        rootGroup.AddSubGroup(foundry);

        var models = new CommandGroup("models", "Foundry models operations - Commands for listing and managing models in AI Foundry.");
        foundry.AddSubGroup(models);

        var deployments = new CommandGroup("deployments", "Foundry models deployments operations - Commands for listing and managing models deployments in AI Foundry.");
        models.AddSubGroup(deployments);

        deployments.AddCommand("list", new DeploymentsListCommand());

        models.AddCommand("list", new ModelsListCommand());
        models.AddCommand("deploy", new ModelDeploymentCommand());

        var knowledge = new CommandGroup("knowledge", "Foundry knowledge operations - Commands for managing knowledge bases and indexes in AI Foundry.");
        foundry.AddSubGroup(knowledge);

        var index = new CommandGroup("index", "Foundry knowledge index operations - Commands for managing knowledge indexes in AI Foundry.");
        knowledge.AddSubGroup(index);

        index.AddCommand("list", new KnowledgeIndexListCommand());
        index.AddCommand("schema", new KnowledgeIndexSchemaCommand());

        var agents = new CommandGroup("agents", "Foundry agents operations - Commands for listing, querying, and evaluating agents in AI Foundry.");
        foundry.AddSubGroup(agents);

        agents.AddCommand("list", new AgentsListCommand());
        agents.AddCommand("connect", new AgentsConnectCommand());
        agents.AddCommand("query-and-evaluate", new AgentsQueryAndEvaluateCommand());
        agents.AddCommand("evaluate", new AgentsEvaluateCommand());
    }
}
