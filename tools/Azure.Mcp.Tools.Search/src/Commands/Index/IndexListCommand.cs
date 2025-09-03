// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Index;

public sealed class IndexListCommand(ILogger<IndexListCommand> logger) : GlobalCommand<IndexListOptions>()
{
    private const string CommandTitle = "List Azure AI Search (formerly known as \"Azure Cognitive Search\") Indexes";
    private readonly ILogger<IndexListCommand> _logger = logger;
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;
    public override string Name => "list";

    public override string Description =>
        """
        List all indexes in an Azure AI Search service.

        Required arguments:
        - service
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serviceOption);
    }

    protected override IndexListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault(_serviceOption);
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

            var indexes = await searchService.ListIndexes(
                options.Service!,
                options.RetryPolicy);

            context.Response.Results = indexes?.Count > 0
                ? ResponseResult.Create(
                    new IndexListCommandResult(indexes),
                    SearchJsonContext.Default.IndexListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing search indexes");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record IndexListCommandResult(List<IndexInfo> Indexes);
}
