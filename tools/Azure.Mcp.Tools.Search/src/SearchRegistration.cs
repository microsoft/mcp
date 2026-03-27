// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Commands.Service;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Search;

public sealed class SearchRegistration : IAreaRegistration
{
    public static string AreaName => "search";

    public static string AreaTitle => "Azure AI Search";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure AI Search operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "index",
                Description = "Azure AI Search (formerly known as \\\"Azure Cognitive Search\\\") index operations - Commands for listing, managing, and querying search indexes in a specific search service.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "471292d0-4f6d-49d8-bf29-cbcb7b27dedb",
                        Name = "get",
                        Description = "List/get/show Azure AI Search indexes in a Search service. Returns index properties such as fields, description, and more. If a specific index name is not provided, the command will return details for all indexes.",
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
                                Name = "service",
                                Description = "The name of the Azure AI Search service (e.g., my-search-service).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "index",
                                Description = "The name of the search index within the Azure AI Search service.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(IndexGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "f1938a77-8d6c-49c7-b592-71b4f26508e7",
                        Name = "query",
                        Description = "Queries/searches documents in an Azure AI Search index with a given query, returning the results of the query/search.",
                        Title = "Query",
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
                                Name = "service",
                                Description = "The name of the Azure AI Search service (e.g., my-search-service).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "index",
                                Description = "The name of the search index within the Azure AI Search service.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "query",
                                Description = "The search query to execute against the Azure AI Search index.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(IndexQueryCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "knowledge",
                Description = "Azure AI Search knowledge operations - Commands retrieving data from knowledge sources, listing knowledge sources and knowledge bases in a search service.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "base",
                        Description = "Knowledge base operations - get knowledge bases associated with a service.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "e0e7c288-8d16-4d11-811d-9236dc86d9a8",
                                Name = "get",
                                Description = "Gets the details of Azure AI Search knowledge bases. Knowledge bases encapsulate retrieval and reasoning capabilities over one or more knowledge sources or indexes. If a specific knowledge base name is not provided, the command will return details for all knowledge bases within the specified service. Required arguments: - service",
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
                                        Name = "service",
                                        Description = "The name of the Azure AI Search service (e.g., my-search-service).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "knowledge-base",
                                        Description = "The name of the knowledge base within the Azure AI Search service.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Global,
                                HandlerType = nameof(IndexGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "dcd2952d-02af-4ffc-a7a2-3c6d04251f66",
                                Name = "retrieve",
                                Description = "Execute a retrieval operation using a specific Azure AI Search knowledge base, effectively searching and querying the underlying data sources as needed to find relevant information. Provide either a --query for single-turn retrieval or one or more conversational --messages in role:content form (e.g. user:What policies apply?). Specifying both --query and --messages is not allowed. Required arguments: - service - knowledge-base",
                                Title = "Retrieve",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = true,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "service",
                                        Description = "The name of the Azure AI Search service (e.g., my-search-service).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "knowledge-base",
                                        Description = "The name of the knowledge base within the Azure AI Search service.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "query",
                                        Description = "Natural language query for retrieval when a conversational message history isn't provided.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "messages",
                                        Description = "Conversation history messages passed to the knowledge base. Able to specify multiple --messages entries. Each entry formatted as role:content, where role is `user` or `assistant` (e.g., user:How many docs?).",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Global,
                                HandlerType = nameof(KnowledgeBaseRetrieveCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "source",
                        Description = "Knowledge source operations - get knowledge sources associated with a service.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "efc985cd-5381-4547-8ffb-89ffe992ea41",
                                Name = "get",
                                Description = "Gets the details of Azure AI Search knowledge sources. A knowledge source may point directly at an existing Azure AI Search index, or may represent external data (e.g. a blob storage container) that has been indexed in Azure AI Search internally. These knowledge sources are used by knowledge bases during retrieval. If a specific knowledge source name is not provided, the command will return details for all knowledge sources within the specified service. Required arguments: - service",
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
                                        Name = "service",
                                        Description = "The name of the Azure AI Search service (e.g., my-search-service).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "knowledge-source",
                                        Description = "The name of the knowledge source within the Azure AI Search service.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Global,
                                HandlerType = nameof(IndexGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "service",
                Description = "Azure AI Search (formerly known as \\\"Azure Cognitive Search\\\") service operations - Commands for listing and managing search services in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b0684f8c-20de-4bc0-bbc3-982575c8441f",
                        Name = "list",
                        Description = "List/show Azure AI Search services in a subscription, returning details about each service.",
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
                        Options = [],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ServiceListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<ServiceListCommand>();
        services.AddSingleton<IndexGetCommand>();
        services.AddSingleton<IndexQueryCommand>();
        services.AddSingleton<KnowledgeSourceGetCommand>();
        services.AddSingleton<KnowledgeBaseGetCommand>();
        services.AddSingleton<KnowledgeBaseRetrieveCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(IndexGetCommand) => serviceProvider.GetRequiredService<IndexGetCommand>(),
            nameof(IndexQueryCommand) => serviceProvider.GetRequiredService<IndexQueryCommand>(),
            nameof(KnowledgeBaseRetrieveCommand) => serviceProvider.GetRequiredService<KnowledgeBaseRetrieveCommand>(),
            nameof(ServiceListCommand) => serviceProvider.GetRequiredService<ServiceListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in search area.")
        };
}
