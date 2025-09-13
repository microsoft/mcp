// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

public sealed class KnowledgeAgentRunRetrievalCommand(ILogger<KnowledgeAgentRunRetrievalCommand> logger) : GlobalCommand<BaseSearchOptions>()
{
    private readonly ILogger<KnowledgeAgentRunRetrievalCommand> _logger = logger;
    private readonly Option<string> _serviceOption = SearchOptionDefinitions.Service;
    private readonly Option<string> _agentOption = SearchOptionDefinitions.Agent;
    private readonly Option<string> _queryOption = SearchOptionDefinitions.OptionalQuery;
    private readonly Option<string[]> _messagesOption = SearchOptionDefinitions.Messages;

    public override string Name => "runretrieval";

    public override string Title => "Execute retrieval using a knowledge agent in Azure AI Search";

    public override string Description =>
        """
        Execute a retrieval operation using a specific Azure AI Search knowledge agent. Provide either a --query for single-turn
        retrieval or one or more conversational --messages in role:content form (e.g. user:What policies apply?). If both are supplied
        the messages take precedence and the optional --query is ignored.

        Required arguments:
        - service
        - agent
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true,
        OpenWorld = true, // interacts with evolving knowledge corpora
        Idempotent = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serviceOption);
        command.Options.Add(_agentOption);
        command.Options.Add(_queryOption);
        command.Options.Add(_messagesOption);
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

        var agentName = parseResult.GetValueOrDefault(_agentOption);
        var query = parseResult.GetValueOrDefault(_queryOption);
        var messages = parseResult.GetValueOrDefault(_messagesOption) ?? [];

        if (string.IsNullOrEmpty(query) && messages.Length == 0)
        {
            context.Response.Status = 400;
            context.Response.Message = "Either --query or at least one --messages entry must be provided.";
            return context.Response;
        }

        List<(string role, string message)>? parsedMessages = null;
        if (messages.Length > 0)
        {
            parsedMessages = new();
            foreach (var msg in messages)
            {
                var idx = msg.IndexOf(':');
                if (idx <= 0 || idx == msg.Length - 1)
                {
                    context.Response.Status = 400;
                    context.Response.Message = $"Invalid message format '{msg}'. Expected role:content.";
                    return context.Response;
                }
                var role = msg[..idx].Trim();
                var content = msg[(idx + 1)..].Trim();
                if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(content))
                {
                    context.Response.Status = 400;
                    context.Response.Message = $"Invalid message format '{msg}'. Role and content required.";
                    return context.Response;
                }
                parsedMessages.Add(new ValueTuple<string, string>(role, content));
            }
        }

        try
        {
            var searchService = context.GetService<ISearchService>();
            var result = await searchService.RetrieveFromKnowledgeAgent(options.Service!, agentName!, query, parsedMessages, options.RetryPolicy);
            context.Response.Results = ResponseResult.Create(new KnowledgeAgentRunRetrievalCommandResult(result), SearchJsonContext.Default.KnowledgeAgentRunRetrievalCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing knowledge agent retrieval");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record KnowledgeAgentRunRetrievalCommandResult(string RetrievalResult);
}
