// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Commands.Service;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search;

public class SearchSetup : IAreaSetup
{
    public string Name => "search";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISearchService, SearchService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var search = new CommandGroup(Name,
        """
        Search operations - Commands for Azure AI Search (formerly known as \"Azure Cognitive Search\") services and
        search indexes. Use this tool when you need to list search services and indexes, get index details, or execute
        queries against indexed content. This tool supports  enterprise search, document search, and knowledge mining
        workloads. Do not use this tool for database queries, Azure Monitor log searches, general web search, or
        simple string matching operations - this tool is specifically designed for Azure AI Search service management
        and complex search operations. This tool is a hierarchical MCP command router where sub-commands are routed to
        MCP servers that require specific fields inside the \"parameters\" object. To invoke a command, set
        \"command\" and wrap its arguments in \"parameters\". Set \"learn=true\" to discover available sub-commands
        for different search service and index operations. Note that this tool requires appropriate Azure AI Search
        permissions and will only access search services and indexes accessible to the authenticated user.
        """);
        rootGroup.AddSubGroup(search);

        var service = new CommandGroup("service", "Azure AI Search (formerly known as \"Azure Cognitive Search\") service operations - Commands for listing and managing search services in your Azure subscription.");
        search.AddSubGroup(service);

        service.AddCommand("list", new ServiceListCommand(loggerFactory.CreateLogger<ServiceListCommand>()));

        var index = new CommandGroup("index", "Azure AI Search (formerly known as \"Azure Cognitive Search\") index operations - Commands for listing, managing, and querying search indexes in a specific search service.");
        search.AddSubGroup(index);

        index.AddCommand("list", new IndexListCommand(loggerFactory.CreateLogger<IndexListCommand>()));
        index.AddCommand("describe", new IndexDescribeCommand(loggerFactory.CreateLogger<IndexDescribeCommand>()));
        index.AddCommand("query", new IndexQueryCommand(loggerFactory.CreateLogger<IndexQueryCommand>()));
    }
}
