// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Index;

public sealed class IndexQueryCommand(ILogger<IndexQueryCommand> logger) : GlobalCommand<IndexQueryOptions>()
{
    private const string CommandTitle = "Query Azure AI Search (formerly known as \"Azure Cognitive Search\") Index";
    private readonly ILogger<IndexQueryCommand> _logger = logger;
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;
    private readonly Option<string> _indexOption = SearchOptionDefinitions.Index;
    private readonly Option<string> _queryOption = SearchOptionDefinitions.Query;

    public override string Name => "query";

    public override string Description =>
        """
        Query an Azure AI Search index. Returns search results matching the specified query.

        Required arguments:
        - service: The name of the Azure AI Search service
        - index: The name of the search index to query
        - query: The search text to query with
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_serviceOption);
        command.AddOption(_indexOption);
        command.AddOption(_queryOption);
    }

    protected override IndexQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueForOption(_serviceOption);
        options.Index = parseResult.GetValueForOption(_indexOption);
        options.Query = parseResult.GetValueForOption(_queryOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var searchService = context.GetService<ISearchService>();

            var results = await searchService.QueryIndex(
                options.Service!,
                options.Index!,
                options.Query!,
                options.RetryPolicy);

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
