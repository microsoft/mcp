// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Index;

public sealed class IndexQueryCommand(ILogger<IndexQueryCommand> logger) : GlobalCommand<IndexQueryOptions>()
{
    private const string CommandTitle = "Query Azure AI Search (formerly known as \"Azure Cognitive Search\") Index";
    private readonly ILogger<IndexQueryCommand> _logger = logger;

    public override string Name => "query";

    public override string Description =>
        """
        Queries an Azure AI Search index, returning the results of the query.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
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
