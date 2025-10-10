// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

public sealed class KnowledgeSourceGetCommand(ILogger<KnowledgeSourceGetCommand> logger) : GlobalCommand<KnowledgeSourceGetOptions>()
{
    private const string CommandTitle = "Get Azure AI Search Knowledge Source Details";
    private readonly ILogger<KnowledgeSourceGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Title => CommandTitle;

    public override string Description =>
        """
        Gets the details of Azure AI Search knowledge sources. A knowledge source may point directly at an
        existing Azure AI Search index, or may represent external data (e.g. a blob storage container) that has been
        indexed in Azure AI Search internally. These knowledge sources are used by knowledge bases during retrieval.
        If a specific knowledge source name is not provided, the command will return details for all knowledge sources
        within the specified service.

        Required arguments:
        - service
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SearchOptionDefinitions.Service);
        command.Options.Add(SearchOptionDefinitions.KnowledgeSource.AsOptional());
    }

    protected override KnowledgeSourceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Service.Name);
        options.KnowledgeSource = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.KnowledgeSource.Name);
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
            var sources = await searchService.ListKnowledgeSources(options.Service!, options.KnowledgeSource, options.RetryPolicy);
            context.Response.Results = ResponseResult.Create(new(sources ?? []), SearchJsonContext.Default.KnowledgeSourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge sources");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record KnowledgeSourceGetCommandResult([property: JsonPropertyName("knowledgeSources")] List<Models.KnowledgeSourceInfo> KnowledgeSources);
}
