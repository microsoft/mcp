// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Index;

public sealed class IndexGetCommand(ILogger<IndexGetCommand> logger) : GlobalCommand<IndexGetOptions>()
{
    private const string CommandTitle = "Get Azure AI Search (formerly known as \"Azure Cognitive Search\") Index Details";
    private readonly ILogger<IndexGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Gets the details of Azure AI Search indexes, including fields, description, and more. If a specific index name
        is not provided, the command will return details for all indexes within the specified service.
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
        command.Options.Add(SearchOptionDefinitions.Index.AsOptional());
    }

    protected override IndexGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Service.Name);
        options.Index = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Index.Name);
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

            var indexes = await searchService.GetIndexDetails(
                options.Service!,
                options.Index,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(indexes ?? []), SearchJsonContext.Default.IndexGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving search index definition");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record IndexGetCommandResult([property: JsonPropertyName("indexes")] List<IndexInfo> Indexes);
}
