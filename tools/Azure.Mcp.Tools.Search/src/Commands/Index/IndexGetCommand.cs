// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;
    private readonly Option<string> _optionalIndexOption = SearchOptionDefinitions.OptionalIndex;

    public override string Name => "describe";

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
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serviceOption);
        command.Options.Add(_optionalIndexOption);
    }

    protected override IndexGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault(_serviceOption);
        options.Index = parseResult.GetValueOrDefault(_optionalIndexOption);
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

            context.Response.Results = indexes is { Count: > 0 }
                ? ResponseResult.Create(new(indexes), SearchJsonContext.Default.IndexGetCommandResult)
                : null;
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
