// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Commands.Service;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Search;

public class SearchSetup : IAreaSetup
{
    public string Name => "search";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISearchService, SearchService>();

        services.AddSingleton<ServiceListCommand>();
        services.AddSingleton<IndexGetCommand>();
        services.AddSingleton<IndexQueryCommand>();
        services.AddSingleton<KnowledgeSourceListCommand>();
        services.AddSingleton<KnowledgeAgentListCommand>();
        services.AddSingleton<KnowledgeAgentRunRetrievalCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var search = new CommandGroup(Name,
        """
        Search operations - Commands for Azure AI Search (formerly known as \"Azure Cognitive Search\") services,
        search indexes, knowledge sources and knowledge agents. Use this tool when you need to retrieve knowledge,
        search indexes, or introspect search services and their components. You can use knowledge agents for 
        in-depth agentic retrieval, or just execute searches against search indexes for fast single-shot search. 
        There are also commands for listing indexes, knowledge sources and knowledge agents, and to describe 
        schemas. This tool supports  enterprise search, document search, and knowledge mining workloads. Do not 
        use this tool for database queries, Azure Monitor log searches, general web search, or simple string 
        matching operations - this tool is specifically designed for Azure AI Search service management and 
        complex search operations. This tool is a hierarchical MCP command router where sub-commands are routed to 
        MCP servers that require specific fields inside the \"parameters\" object. To invoke a command, set 
        \"command\" and wrap its arguments in \"parameters\". Set \"learn=true\" to discover available 
        sub-commands for different search service and index operations. Note that this tool requires appropriate 
        Azure AI Search permissions and will only access search services and indexes accessible to the 
        authenticated user.
        """);

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

        var knowledge = new CommandGroup("knowledge", "Azure AI Search knowledge operations - Commands for listing knowledge sources and agents in a search service.");
        search.AddSubGroup(knowledge);

        var knowledgeSource = new CommandGroup("source", "Knowledge source operations - list knowledge sources associated with a service.");
        knowledge.AddSubGroup(knowledgeSource);
        var knowledgeSourceList = serviceProvider.GetRequiredService<KnowledgeSourceListCommand>();
        knowledgeSource.AddCommand(knowledgeSourceList.Name, knowledgeSourceList);

        var knowledgeAgent = new CommandGroup("agent", "Knowledge agent operations - list knowledge agents associated with a service.");
        knowledge.AddSubGroup(knowledgeAgent);
        var knowledgeAgentList = serviceProvider.GetRequiredService<KnowledgeAgentListCommand>();
        knowledgeAgent.AddCommand(knowledgeAgentList.Name, knowledgeAgentList);
        var knowledgeAgentRunRetrieval = serviceProvider.GetRequiredService<KnowledgeAgentRunRetrievalCommand>();
        knowledgeAgent.AddCommand(knowledgeAgentRunRetrieval.Name, knowledgeAgentRunRetrieval);

        return search;
    }
}
