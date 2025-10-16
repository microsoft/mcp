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

public sealed class KnowledgeBaseGetCommand(ILogger<KnowledgeBaseGetCommand> logger) : GlobalCommand<KnowledgeBaseGetOptions>()
{
    private const string CommandTitle = "Get Azure AI Search Knowledge Base Details";
    private readonly ILogger<KnowledgeBaseGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Title => CommandTitle;

    public override string Description =>
        """
        Gets the details of Azure AI Search knowledge bases. Knowledge bases encapsulate retrieval and reasoning
        capabilities over one or more knowledge sources or indexes. If a specific knowledge base name is not provided,
        the command will return details for all knowledge bases within the specified service.

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
        command.Options.Add(SearchOptionDefinitions.KnowledgeBase.AsOptional());
    }

    protected override KnowledgeBaseGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Service.Name);
        options.KnowledgeBase = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.KnowledgeBase.Name);
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
            var bases = await searchService.ListKnowledgeBases(options.Service!, options.KnowledgeBase, options.RetryPolicy);
            context.Response.Results = ResponseResult.Create(new(bases ?? []), SearchJsonContext.Default.KnowledgeBaseGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge bases");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record KnowledgeBaseGetCommandResult([property: JsonPropertyName("knowledgeBases")] List<Models.KnowledgeBaseInfo> KnowledgeBases);
}
