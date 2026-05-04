// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
public sealed class IndexQueryCommand(ILogger<IndexQueryCommand> logger, ISearchService searchService) : GlobalCommand<IndexQueryOptions>()
{
    private readonly ILogger<IndexQueryCommand> _logger = logger;
    private readonly ISearchService _searchService = searchService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SearchOptionDefinitions.Service);
        command.Options.Add(SearchOptionDefinitions.Index);
        command.Options.Add(SearchOptionDefinitions.Query);
    }

    protected override IndexQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Service.Name);
        options.Index = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Index.Name);
        options.Query = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Query.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var results = await _searchService.QueryIndex(
                options.Service!,
                options.Index!,
                options.Query!,
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
