// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Azure.Subscription;
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

        var deploymentList = serviceProvider.GetRequiredService<DeploymentsListCommand>();
        deployments.AddCommand(deploymentList.Name, deploymentList);

        var modelList = serviceProvider.GetRequiredService<ModelsListCommand>();
        models.AddCommand(modelList.Name, modelList);

        var deploymentCommand = serviceProvider.GetRequiredService<ModelDeploymentCommand>();
        models.AddCommand(deploymentCommand.Name, deploymentCommand);

        var knowledge = new CommandGroup("knowledge", "Foundry knowledge operations - Commands for managing knowledge bases and indexes in AI Foundry.");
        foundry.AddSubGroup(knowledge);

        var index = new CommandGroup("index", "Foundry knowledge index operations - Commands for managing knowledge indexes in AI Foundry.");
        knowledge.AddSubGroup(index);

        var indexList = serviceProvider.GetRequiredService<KnowledgeIndexListCommand>();
        index.AddCommand(indexList.Name, indexList);

        var indexSchema = serviceProvider.GetRequiredService<KnowledgeIndexSchemaCommand>();
        index.AddCommand(indexSchema.Name, indexSchema);

        var openai = new CommandGroup("openai", "Foundry OpenAI operations - Commands for working with Azure OpenAI models deployed in AI Foundry.");
        foundry.AddSubGroup(openai);

        openai.AddCommand("create-completion", new OpenAiCompletionsCreateCommand());
        var agents = new CommandGroup("agents", "Foundry agents operations - Commands for listing, querying, and evaluating agents in AI Foundry.");
        foundry.AddSubGroup(agents);

        agents.AddCommand("list", serviceProvider.GetRequiredService<AgentsListCommand>());
        agents.AddCommand("connect", serviceProvider.GetRequiredService<AgentsConnectCommand>());
        agents.AddCommand("query-and-evaluate", serviceProvider.GetRequiredService<AgentsQueryAndEvaluateCommand>());
        agents.AddCommand("evaluate", serviceProvider.GetRequiredService<AgentsEvaluateCommand>());

        return foundry;
    }
}
