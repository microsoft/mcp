// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

public sealed class KnowledgeAgentListCommand(ILogger<KnowledgeAgentListCommand> logger) : GlobalCommand<BaseSearchOptions>()
{
    private readonly ILogger<KnowledgeAgentListCommand> _logger = logger;
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;

    public override string Name => "list";

    public override string Title => "List Azure AI Search knowledge agents";

    public override string Description =>
        """
        List all knowledge agents defined in an Azure AI Search service. Knowledge agents encapsulate retrieval and reasoning
        capabilities over one or more knowledge sources or indexes.

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
            var agents = await searchService.ListKnowledgeAgents(options.Service!, options.RetryPolicy);
            context.Response.Results = agents.Count > 0
                ? ResponseResult.Create(new KnowledgeAgentListCommandResult(agents), SearchJsonContext.Default.KnowledgeAgentListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing knowledge agents");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record KnowledgeAgentListCommandResult(List<Models.KnowledgeAgentInfo> KnowledgeAgents);
}
