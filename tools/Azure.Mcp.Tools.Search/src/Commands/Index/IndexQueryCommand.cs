// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Search.Commands.Index;

[CommandMetadata(
    Id = "f1938a77-8d6c-49c7-b592-71b4f26508e7",
    Name = "query",
    Title = "Query an Azure AI Search (formerly known as \"Azure Cognitive Search\") Index",
    Description = """
        Queries/searches documents in an Azure AI Search index with a given query, returning the results of the
        query/search.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IndexQueryCommand(ILogger<IndexQueryCommand> logger, ISearchService searchService)
    : AuthenticatedCommand<IndexQueryOptions, List<JsonElement>>
{
    private readonly ILogger<IndexQueryCommand> _logger = logger;
    private readonly ISearchService _searchService = searchService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, IndexQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _searchService.QueryIndex(
                options.Service,
                options.Index,
                options.Query,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(results, SearchJsonContext.Default.ListJsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing search query");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
