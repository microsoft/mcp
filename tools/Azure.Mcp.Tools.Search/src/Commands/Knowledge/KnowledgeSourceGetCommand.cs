// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

[CommandMetadata(
    Id = "efc985cd-5381-4547-8ffb-89ffe992ea41",
    Name = "get",
    Title = "Get Azure AI Search Knowledge Source Details",
    Description = """
        Gets the details of Azure AI Search knowledge sources. A knowledge source may point directly at an
        existing Azure AI Search index, or may represent external data (e.g. a blob storage container) that has been
        indexed in Azure AI Search internally. These knowledge sources are used by knowledge bases during retrieval.
        If a specific knowledge source name is not provided, the command will return details for all knowledge sources
        within the specified service.

        Required arguments:
        - service
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class KnowledgeSourceGetCommand(ILogger<KnowledgeSourceGetCommand> logger, ISearchService searchService) : GlobalCommand<KnowledgeSourceGetOptions>()
{
    private readonly ILogger<KnowledgeSourceGetCommand> _logger = logger;
    private readonly ISearchService _searchService = searchService;

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var sources = await _searchService.ListKnowledgeSources(options.Service!, options.KnowledgeSource, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(sources ?? []), SearchJsonContext.Default.KnowledgeSourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge sources");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record KnowledgeSourceGetCommandResult(List<Models.KnowledgeSourceInfo> KnowledgeSources);
}
