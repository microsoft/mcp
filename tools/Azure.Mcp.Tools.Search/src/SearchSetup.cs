// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Commands.Service;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Search;

public class SearchSetup : IAreaSetup
{
    public string Name => "search";

    public string Title => "Azure AI Search";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISearchService, SearchService>();

        services.AddSingleton<ServiceListCommand>();
        services.AddSingleton<IndexGetCommand>();
        services.AddSingleton<IndexQueryCommand>();
        services.AddSingleton<KnowledgeSourceGetCommand>();
        services.AddSingleton<KnowledgeBaseGetCommand>();
        services.AddSingleton<KnowledgeBaseRetrieveCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var search = new CommandGroup(Name,
        """
        Search operations – Commands to manage and query Azure AI Search services, indexes, and
        knowledge sources. Supports enterprise search, document search, and knowledge mining. Not
        intended for database queries or Azure Monitor logs. This is a hierarchical MCP command
        router using the command field and parameters; set learn=true to discover available
        sub-commands.
        """, Title);

        var service = new CommandGroup("service", "Azure AI Search (formerly known as \"Azure Cognitive Search\") service operations - Commands for listing and managing search services in your Azure subscription.");
        search.AddSubGroup(service);

        var serviceList = serviceProvider.GetRequiredService<ServiceListCommand>();
        service.AddCommand(serviceList.Name, serviceList);

        var index = new CommandGroup("index", "Azure AI Search (formerly known as \"Azure Cognitive Search\") index operations - Commands for listing, managing, and querying search indexes in a specific search service.");
        search.AddSubGroup(index);

        var indexGet = serviceProvider.GetRequiredService<IndexGetCommand>();
        index.AddCommand(indexGet.Name, indexGet);
        var indexQuery = serviceProvider.GetRequiredService<IndexQueryCommand>();
        index.AddCommand(indexQuery.Name, indexQuery);

        var knowledge = new CommandGroup("knowledge", "Azure AI Search knowledge operations - Commands retrieving data from knowledge sources, listing knowledge sources and knowledge bases in a search service.");
        search.AddSubGroup(knowledge);

        var knowledgeSource = new CommandGroup("source", "Knowledge source operations - get knowledge sources associated with a service.");
        knowledge.AddSubGroup(knowledgeSource);

        var knowledgeSourceGet = serviceProvider.GetRequiredService<KnowledgeSourceGetCommand>();
        knowledgeSource.AddCommand(knowledgeSourceGet.Name, knowledgeSourceGet);

        var knowledgeBase = new CommandGroup("base", "Knowledge base operations - get knowledge bases associated with a service.");
        knowledge.AddSubGroup(knowledgeBase);

        var knowledgeBaseGet = serviceProvider.GetRequiredService<KnowledgeBaseGetCommand>();
        knowledgeBase.AddCommand(knowledgeBaseGet.Name, knowledgeBaseGet);
        var knowledgeBaseRetrieve = serviceProvider.GetRequiredService<KnowledgeBaseRetrieveCommand>();
        knowledgeBase.AddCommand(knowledgeBaseRetrieve.Name, knowledgeBaseRetrieve);

        return search;
    }
}
