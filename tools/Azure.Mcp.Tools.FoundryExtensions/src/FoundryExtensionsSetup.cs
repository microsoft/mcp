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
        var foundryExtensions = new CommandGroup(Name, "Foundry Extensions service operations - Commands for interacting with Microsoft Foundry OpenAI, knowledge indexes, threads, agents, and resources.", Title);

        var knowledge = new CommandGroup("knowledge", "Foundry knowledge operations - Commands for managing knowledge bases and indexes in Microsoft Foundry.");
        foundryExtensions.AddSubGroup(knowledge);

        var index = new CommandGroup("index", "Foundry knowledge index operations - Commands for managing knowledge indexes in Microsoft Foundry.");
        knowledge.AddSubGroup(index);

        var indexList = serviceProvider.GetRequiredService<KnowledgeIndexListCommand>();
        index.AddCommand(indexList.Name, indexList);

        var indexSchema = serviceProvider.GetRequiredService<KnowledgeIndexSchemaCommand>();
        index.AddCommand(indexSchema.Name, indexSchema);

        var openai = new CommandGroup("openai", "Foundry OpenAI operations - Commands for working with Azure OpenAI models deployed in Microsoft Foundry.");
        foundryExtensions.AddSubGroup(openai);

        openai.AddCommand("create-completion", serviceProvider.GetRequiredService<OpenAiCompletionsCreateCommand>());
        openai.AddCommand("embeddings-create", serviceProvider.GetRequiredService<OpenAiEmbeddingsCreateCommand>());
        openai.AddCommand("models-list", serviceProvider.GetRequiredService<OpenAiModelsListCommand>());
        openai.AddCommand("chat-completions-create", serviceProvider.GetRequiredService<OpenAiChatCompletionsCreateCommand>());

        var resources = new CommandGroup("resource", "Foundry resource operations - Commands for listing and managing Microsoft Foundry resources.");
        foundryExtensions.AddSubGroup(resources);

        resources.AddCommand("get", serviceProvider.GetRequiredService<ResourceGetCommand>());

        return foundryExtensions;
    }
}
