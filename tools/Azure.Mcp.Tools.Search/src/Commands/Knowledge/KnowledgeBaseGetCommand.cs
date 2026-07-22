// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Search.Options.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

[CommandMetadata(
    Id = "e0e7c288-8d16-4d11-811d-9236dc86d9a8",
    Name = "get",
    Title = "Get Azure AI Search Knowledge Base Details",
    Description = """
        Gets the details of Azure AI Search knowledge bases. Knowledge bases encapsulate retrieval and reasoning
        capabilities over one or more knowledge sources or indexes. If a specific knowledge base name is not provided,
        the command will return details for all knowledge bases within the specified service.

        Required arguments:
        - service
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class KnowledgeBaseGetCommand(ILogger<KnowledgeBaseGetCommand> logger, ISearchService searchService)
    : AuthenticatedCommand<KnowledgeBaseGetOptions, KnowledgeBaseGetCommand.KnowledgeBaseGetCommandResult>
{
    private readonly ILogger<KnowledgeBaseGetCommand> _logger = logger;
    private readonly ISearchService _searchService = searchService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KnowledgeBaseGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var bases = await _searchService.ListKnowledgeBases(options.Service, options.KnowledgeBase, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(bases ?? []), SearchJsonContext.Default.KnowledgeBaseGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge bases");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record KnowledgeBaseGetCommandResult(List<Models.KnowledgeBaseInfo> KnowledgeBases);
}
