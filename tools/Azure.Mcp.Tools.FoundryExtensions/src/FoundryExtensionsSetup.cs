// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.FoundryExtensions;

public class FoundryExtensionsSetup : IAreaSetup
{
    public string Name => "foundryextensions";

    public string Title => "Microsoft Foundry Extensions";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFoundryExtensionsService, FoundryExtensionsService>();

        services.AddSingleton<KnowledgeIndexListCommand>();
        services.AddSingleton<KnowledgeIndexSchemaCommand>();

        services.AddSingleton<OpenAiCompletionsCreateCommand>();
        services.AddSingleton<OpenAiEmbeddingsCreateCommand>();
        services.AddSingleton<OpenAiModelsListCommand>();
        services.AddSingleton<OpenAiChatCompletionsCreateCommand>();

        services.AddSingleton<ResourceGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var description = 
        """
            Use this tool for Microsoft Foundry knowledge indexes and OpenAI-compatible APIs: list knowledge indexes, get 
            index schemas, create completions, generate embeddings, list models, and inspect Foundry resources. Use this
            tool for any knowledge-index request. Use foundry instead for deployments, agents, evaluations, sessions, 
            connections, monitoring, and data-generation jobs.
        """;
        var foundryExtensions = new CommandGroup(Name, description, Title);

        var knowledge = new CommandGroup("knowledge", "Foundry knowledge operations - Commands for managing knowledge bases and indexes in Microsoft Foundry.");
        foundryExtensions.AddSubGroup(knowledge);

        var index = new CommandGroup("index", "Foundry knowledge index operations - Commands for managing knowledge indexes in Microsoft Foundry.");
        knowledge.AddSubGroup(index);

        index.AddCommand<KnowledgeIndexListCommand>(serviceProvider);
        index.AddCommand<KnowledgeIndexSchemaCommand>(serviceProvider);

        var openai = new CommandGroup("openai", "Foundry OpenAI operations - Commands for working with Azure OpenAI models deployed in Microsoft Foundry.");
        foundryExtensions.AddSubGroup(openai);

        openai.AddCommand<OpenAiCompletionsCreateCommand>(serviceProvider);
        openai.AddCommand<OpenAiEmbeddingsCreateCommand>(serviceProvider);
        openai.AddCommand<OpenAiModelsListCommand>(serviceProvider);
        openai.AddCommand<OpenAiChatCompletionsCreateCommand>(serviceProvider);

        var resources = new CommandGroup("resource", "Foundry resource operations - Commands for listing and managing Microsoft Foundry resources.");
        foundryExtensions.AddSubGroup(resources);

        resources.AddCommand<ResourceGetCommand>(serviceProvider);

        return foundryExtensions;
    }
}
