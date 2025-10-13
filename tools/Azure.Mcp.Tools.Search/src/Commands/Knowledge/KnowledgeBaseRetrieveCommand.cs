// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Search.Options;
using Azure.Mcp.Tools.Search.Options.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

public sealed class KnowledgeBaseRetrieveCommand(ILogger<KnowledgeBaseRetrieveCommand> logger) : GlobalCommand<KnowledgeBaseRetrieveOptions>()
{
    private const string CommandTitle = "Execute retrieval using a knowledge base in Azure AI Search";
    private readonly ILogger<KnowledgeBaseRetrieveCommand> _logger = logger;

    public override string Name => "retrieve";

    public override string Title => CommandTitle;

    public override string Description =>
        """
        Execute a retrieval operation using a specific Azure AI Search knowledge base, effectively searching and querying the underlying
        data sources as needed to find relevant information. Provide either a --query for single-turn retrieval or one or more
        conversational --messages in role:content form (e.g. user:What policies apply?). Specifying both --query and --messages is not
        allowed.

        Required arguments:
        - service
        - knowledge-base
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = false,
        ReadOnly = true,
        OpenWorld = true, // possibly interacts with Web content and federated data sources
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SearchOptionDefinitions.Service);
        command.Options.Add(SearchOptionDefinitions.KnowledgeBase);
        command.Options.Add(SearchOptionDefinitions.KnowledgeQuery.AsOptional());
        command.Options.Add(SearchOptionDefinitions.Messages.AsOptional());
        command.Validators.Add(commandResult =>
        {
            var query = commandResult.GetValueOrDefault<string>(SearchOptionDefinitions.KnowledgeQuery.Name);
            var messages = commandResult.GetValueOrDefault<string[]>(SearchOptionDefinitions.Messages.Name) ?? [];
            if (string.IsNullOrEmpty(query) && messages.Length == 0)
            {
                commandResult.AddError("Either --query or at least one --messages entry must be provided.");
            }
            else if (!string.IsNullOrEmpty(query) && messages.Length > 0)
            {
                commandResult.AddError("Specifying both --query and --messages is not allowed.");
            }
        });
    }

    protected override KnowledgeBaseRetrieveOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Service = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.Service.Name);
        options.KnowledgeBase = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.KnowledgeBase.Name);
        options.Query = parseResult.GetValueOrDefault<string>(SearchOptionDefinitions.KnowledgeQuery.Name);
        options.Messages = parseResult.GetValueOrDefault<string[]>(SearchOptionDefinitions.Messages.Name) ?? [];
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        List<(string role, string message)>? parsedMessages = null;
        if (options.Messages.Length > 0)
        {
            try
            {
                parsedMessages = [.. options.Messages.Select(m => ParseMessage(m))];
            }
            catch (ArgumentException ex)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = ex.Message;
                return context.Response;
            }
        }

        try
        {
            var searchService = context.GetService<ISearchService>();
            var result = await searchService.RetrieveFromKnowledgeBase(options.Service!, options.KnowledgeBase!, options.Query, parsedMessages, options.RetryPolicy);
            context.Response.Results = ResponseResult.Create(new(result), SearchJsonContext.Default.KnowledgeBaseRetrieveCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing knowledge base retrieval");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal (string role, string content) ParseMessage(string message)
    {
        var idx = message.IndexOf(':');
        if (idx <= 0 || idx == message.Length - 1)
        {
            throw new ArgumentException($"Invalid message format '{message}'. Expected role:content.");
        }
        var role = message[..idx].Trim();
        var content = message[(idx + 1)..].Trim();
        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(content))
        {
            throw new ArgumentException($"Invalid message format '{message}'. Role and content required.");
        }
        if (role != "user" && role != "assistant")
        {
            throw new ArgumentException($"Invalid message role '{role}', must be 'user' or 'assistant'.");
        }
        return (role, content);
    }

    internal sealed record KnowledgeBaseRetrieveCommandResult(string RetrievalResult);
}
