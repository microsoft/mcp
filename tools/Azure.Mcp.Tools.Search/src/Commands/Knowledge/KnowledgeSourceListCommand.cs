// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

public sealed class KnowledgeSourceListCommand(ILogger<KnowledgeSourceListCommand> logger) : GlobalCommand<BaseSearchOptions>()
{
    private readonly ILogger<KnowledgeSourceListCommand> _logger = logger;
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;

    public override string Name => "list";

    public override string Title => "List Azure AI Search knowledge sources";

    public override string Description =>
        """
        List all knowledge sources defined in an Azure AI Search service. A knowledge source may point directly at an
        existing Azure AI Search index, or may represent external data (e.g. a blob storage container) that has been
        indexed in Azure AI Search internally. These knowledge sources are used by knowledge agents during retrieval.

        Required arguments:
        - service
        """;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serviceOption);
    }

    protected override BaseSearchOptions BindOptions(ParseResult parseResult)
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
            var sources = await searchService.ListKnowledgeSources(options.Service!, options.RetryPolicy);
            context.Response.Results = sources.Count > 0
                ? ResponseResult.Create(new KnowledgeSourceListCommandResult(sources), SearchJsonContext.Default.KnowledgeSourceListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing knowledge sources");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record KnowledgeSourceListCommandResult(List<Models.KnowledgeSourceInfo> KnowledgeSources);
}
