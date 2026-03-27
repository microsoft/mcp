// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.FoundryExtensions;

public sealed class FoundryExtensionsRegistration : IAreaRegistration
{
    public static string AreaName => "foundryextensions";

    public static string AreaTitle => "Microsoft Foundry Extensions";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Foundry Extensions service operations - Commands for interacting with Microsoft Foundry OpenAI, knowledge indexes, threads, agents, and resources.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "knowledge",
                Description = "Foundry knowledge operations - Commands for managing knowledge bases and indexes in Microsoft Foundry.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "index",
                        Description = "Foundry knowledge index operations - Commands for managing knowledge indexes in Microsoft Foundry.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "b2c3d4e5-2345-6789-bcde-f01234567890",
                                Name = "list",
                                Description = "Retrieves a list of knowledge indexes from Microsoft Foundry. This function is used when a user requests information about the available knowledge indexes in Microsoft Foundry. It provides an overview of the knowledge bases and search indexes that are currently deployed and available for use with AI agents and applications. Requires the project endpoint URL (format: https://<resource>.services.ai.azure.com/api/projects/<project-name>). Usage: Use this function when a user wants to explore the available knowledge indexes in Microsoft Foundry. This can help users understand what knowledge bases are currently operational and how they can be utilized for retrieval-augmented generation (RAG) scenarios. Notes: - The indexes listed are knowledge indexes specifically created within Microsoft Foundry projects. - These indexes can be used with AI agents for knowledge retrieval and RAG applications. - The list may change as new indexes are created or existing ones are updated.",
                                Title = "List",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "endpoint",
                                        Description = "The endpoint URL for the Microsoft Foundry project/service. The endpoint follows this pattern https://<foundry-resource-name>.services.ai.azure.com/api/projects/<project-name>.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Global,
                                HandlerType = nameof(KnowledgeIndexListCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "c3d4e5f6-3456-789a-cdef-012345678901",
                                Name = "schema",
                                Description = "Retrieves the detailed schema configuration of a specific knowledge index from Microsoft Foundry. This function provides comprehensive information about the structure and configuration of a knowledge index, including field definitions, data types, searchable attributes, and other schema properties. The schema information is essential for understanding how the index is structured and how data is indexed and searchable. Usage: Use this function when you need to examine the detailed configuration of a specific knowledge index. This is helpful for troubleshooting search issues, understanding index capabilities, planning data mapping, or when integrating with the index programmatically. Notes: - Returns the index schema.",
                                Title = "Schema",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "endpoint",
                                        Description = "The endpoint URL for the Microsoft Foundry project/service. The endpoint follows this pattern https://<foundry-resource-name>.services.ai.azure.com/api/projects/<project-name>.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "index",
                                        Description = "The name of the knowledge index.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Global,
                                HandlerType = nameof(KnowledgeIndexSchemaCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "openai",
                Description = "Foundry OpenAI operations - Commands for working with Azure OpenAI models deployed in Microsoft Foundry.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "d4e5f6a7-4567-89ab-def0-123456789012",
                        Name = "chat-completions-create",
                        Description = "Create chat completions using Azure OpenAI in Microsoft Foundry. Send messages to Azure OpenAI chat models deployed in your Microsoft Foundry resource and receive AI-generated conversational responses. Supports multi-turn conversations with message history, system instructions, and response customization. Use this when you need to create chat completions, have AI conversations, get conversational responses, or build interactive dialogues with Azure OpenAI. Requires resource-name, deployment-name, and message-array.",
                        Title = "Chat Completions Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-name",
                                Description = "The name of the Azure OpenAI resource.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "deployment",
                                Description = "The name of the deployment.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "message-array",
                                Description = "Array of messages in the conversation (JSON format). Each message should have 'role' and 'content' properties.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "max-tokens",
                                Description = "The maximum number of tokens to generate in the completion.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "temperature",
                                Description = "Controls randomness in the output. Lower values make it more deterministic.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "top-p",
                                Description = "Controls diversity via nucleus sampling (0.0 to 1.0). Default is 1.0.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "frequency-penalty",
                                Description = "Penalizes new tokens based on their frequency (-2.0 to 2.0). Default is 0.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "presence-penalty",
                                Description = "Penalizes new tokens based on presence (-2.0 to 2.0). Default is 0.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "stop",
                                Description = "Up to 4 sequences where the API will stop generating further tokens.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "stream",
                                Description = "Whether to stream back partial progress. Default is false.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "seed",
                                Description = "If specified, the system will make a best effort to sample deterministically.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "user",
                                Description = "Optional user identifier for tracking and abuse monitoring.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(OpenAiChatCompletionsCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "e5f6a7b8-5678-9abc-ef01-234567890123",
                        Name = "create-completion",
                        Description = "Create text completions using Azure OpenAI in Microsoft Foundry. Send a prompt or question to Azure OpenAI models deployed in your Microsoft Foundry resource and receive generated text answers. Use this when you need to create completions, get AI-generated content, generate answers to questions, or produce text completions from Azure OpenAI based on any input prompt. Supports customization with temperature and max tokens. Requires resource-name, deployment-name, and prompt-text.",
                        Title = "Create Completion",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-name",
                                Description = "The name of the Azure OpenAI resource.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "deployment",
                                Description = "The name of the deployment.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "prompt-text",
                                Description = "The prompt text to send to the completion model.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "max-tokens",
                                Description = "The maximum number of tokens to generate in the completion.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "temperature",
                                Description = "Controls randomness in the output. Lower values make it more deterministic.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(OpenAiCompletionsCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "f6a7b8c9-6789-abcd-f012-345678901234",
                        Name = "embeddings-create",
                        Description = "Create embeddings using Azure OpenAI in Microsoft Foundry. Generate vector embeddings from text using Azure OpenAI deployments in your Microsoft Foundry resource for semantic search, similarity comparisons, clustering, or machine learning. Use this when you need to create foundry embeddings, generate vectors from text, or convert text to numerical representations using Azure OpenAI. Requires resource-name, deployment-name, and input-text.",
                        Title = "Embeddings Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-name",
                                Description = "The name of the Azure OpenAI resource.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "deployment",
                                Description = "The name of the deployment.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "input-text",
                                Description = "The input text to generate embeddings for.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "user",
                                Description = "Optional user identifier for tracking and abuse monitoring.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "encoding-format",
                                Description = "The format to return embeddings in (float or base64).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "dimensions",
                                Description = "The number of dimensions for the embedding output. Only supported in some models.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(OpenAiEmbeddingsCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "a7b8c9d0-7890-bcde-0123-456789012345",
                        Name = "models-list",
                        Description = "List all available Azure OpenAI models and deployments in a Microsoft Foundry resource. This tool retrieves information about Azure OpenAI models deployed in your Microsoft Foundry resource including model names, versions, capabilities, and deployment status. Use this when you need to see what OpenAI models are available, check model deployments, or list Azure OpenAI models in your foundry resource. Returns model information as JSON array. Requires resource-name.",
                        Title = "Models List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-name",
                                Description = "The name of the Azure OpenAI resource.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(OpenAiModelsListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "resource",
                Description = "Foundry resource operations - Commands for listing and managing Microsoft Foundry resources.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b8c9d0e1-8901-cdef-1234-567890123456",
                        Name = "get",
                        Description = "Gets detailed information about Microsoft Foundry (Cognitive Services) resources, including endpoint URL, location, SKU, and all deployed models with their configuration. If a specific resource name is provided, returns details for that resource only. If no resource name is provided, lists all Microsoft Foundry resources in the subscription or resource group. Use this tool when users need endpoint information, want to discover available AI resources, or need to see all models deployed on AI resources.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-name",
                                Description = "The name of the Azure OpenAI resource.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ResourceGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(KnowledgeIndexListCommand) => serviceProvider.GetRequiredService<KnowledgeIndexListCommand>(),
            nameof(KnowledgeIndexSchemaCommand) => serviceProvider.GetRequiredService<KnowledgeIndexSchemaCommand>(),
            nameof(OpenAiChatCompletionsCreateCommand) => serviceProvider.GetRequiredService<OpenAiChatCompletionsCreateCommand>(),
            nameof(OpenAiCompletionsCreateCommand) => serviceProvider.GetRequiredService<OpenAiCompletionsCreateCommand>(),
            nameof(OpenAiEmbeddingsCreateCommand) => serviceProvider.GetRequiredService<OpenAiEmbeddingsCreateCommand>(),
            nameof(OpenAiModelsListCommand) => serviceProvider.GetRequiredService<OpenAiModelsListCommand>(),
            nameof(ResourceGetCommand) => serviceProvider.GetRequiredService<ResourceGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in foundryextensions area.")
        };
}
