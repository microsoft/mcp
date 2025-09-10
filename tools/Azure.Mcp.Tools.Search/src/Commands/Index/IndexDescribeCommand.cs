// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Index;

public sealed class IndexDescribeCommand(ILogger<IndexDescribeCommand> logger) : GlobalCommand<IndexDescribeOptions>()
{
    private const string CommandTitle = "Get Azure AI Search (formerly known as \"Azure Cognitive Search\") Index Details";
    private readonly ILogger<IndexDescribeCommand> _logger = logger;
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;
    private readonly Option<string> _indexOption = SearchOptionDefinitions.Index;

    public override string Name => "describe";

    public override string Description =>
        """
        Get the full definition of an Azure AI Search index. Returns the complete index configuration including
        fields, analyzers, suggesters, scoring profiles, and other settings.

        Required arguments:
        - service: The name of the Azure AI Search service
        - index: The name of the search index to retrieve
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serviceOption);
        command.Options.Add(_indexOption);
    }

    protected override IndexDescribeOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault(_serviceOption);
        options.Index = parseResult.GetValueOrDefault(_indexOption);
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

            var indexDefinition = await searchService.DescribeIndex(
                options.Service!,
                options.Index!,
                options.RetryPolicy);

            context.Response.Results = indexDefinition != null
                ? ResponseResult.Create(new(indexDefinition), SearchJsonContext.Default.IndexDescribeCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving search index definition");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record IndexDescribeCommandResult(SearchIndexProxy Index);

    /// <summary>
    /// This record represents the JSON-serialized form of <see cref="SearchIndex"/>
    /// </summary>
    public sealed record SearchIndexProxy()
    {
        public required string Name { get; init; }
        public required List<SearchFieldProxy> Fields { get; init; }

        public sealed record SearchFieldProxy()
        {
            public required string Name { get; init; }
            public required SearchFieldDataType Type { get; init; }
            public bool? Key { get; init; }
            public bool? Searchable { get; init; }
            public bool? Filterable { get; init; }
            public bool? Facetable { get; init; }
            public bool? Retrievable { get; init; }

            [SetsRequiredMembers]
            public SearchFieldProxy(SearchField field) : this()
            {
                Name = field.Name;
                Type = field.Type;
                Key = field.IsKey;
                Searchable = field.IsSearchable;
                Filterable = field.IsFilterable;
                Facetable = field.IsFacetable;
                Retrievable = !field.IsHidden;
            }
        }

        [SetsRequiredMembers]
        public SearchIndexProxy(SearchIndex index) : this()
        {
            Name = index.Name;
            Fields = [.. index.Fields.Select(field => new SearchFieldProxy(field))];
        }
    }
}
