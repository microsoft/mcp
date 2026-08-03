// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Search.Options.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Search.Commands.Knowledge;

[CommandMetadata(
    Id = "dcd2952d-02af-4ffc-a7a2-3c6d04251f66",
    Name = "retrieve",
    Title = "Execute retrieval using a knowledge base in Azure AI Search",
    Description = """
        Execute a retrieval operation using a specific Azure AI Search knowledge base, effectively searching and querying the underlying
        data sources as needed to find relevant information. Provide either a --query for single-turn retrieval or one or more
        conversational --messages in role:content form (e.g. user:What policies apply?). Specifying both --query and --messages is not
        allowed.

        Required arguments:
        - service
        - knowledge-base
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class KnowledgeBaseRetrieveCommand(ILogger<KnowledgeBaseRetrieveCommand> logger, ISearchService searchService)
    : AuthenticatedCommand<KnowledgeBaseRetrieveOptions, KnowledgeBaseRetrieveCommand.KnowledgeBaseRetrieveCommandResult>
{
    private readonly ILogger<KnowledgeBaseRetrieveCommand> _logger = logger;
    private readonly ISearchService _searchService = searchService;

    public override void ValidateOptions(KnowledgeBaseRetrieveOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.Query) && (options.Messages == null || options.Messages.Length == 0))
        {
            validationResult.Errors.Add("Either --query or at least one --messages entry must be provided.");
        }
        else if (!string.IsNullOrEmpty(options.Query) && options.Messages is { Length: > 0 })
        {
            validationResult.Errors.Add("Specifying both --query and --messages is not allowed.");
        }

        if (options.Messages is { Length: > 0 })
        {
            foreach ((var index, var message) in options.Messages.Index())
            {
                try
                {
                    ParseMessage(message);
                }
                catch (ArgumentException ex)
                {
                    validationResult.Errors.Add($"Message {index}: {ex.Message}");
                    continue;
                }
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KnowledgeBaseRetrieveOptions options, CancellationToken cancellationToken)
    {
        List<(string role, string message)>? parsedMessages = null;
        if (options.Messages is { Length: > 0 })
        {
            try
            {
                parsedMessages = [.. options.Messages.Select(ParseMessage)];
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
            var result = await _searchService.RetrieveFromKnowledgeBase(options.Service, options.KnowledgeBase, options.Query, parsedMessages, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), SearchJsonContext.Default.KnowledgeBaseRetrieveCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing knowledge base retrieval");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal static (string role, string content) ParseMessage(string message)
    {
        var idx = message.IndexOf(':');
        if (idx <= 0 || idx == message.Length - 1)
        {
            throw new ArgumentException($"Invalid message format, expected 'role:content'.");
        }
        var role = message[..idx].Trim();
        var content = message[(idx + 1)..].Trim();
        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(content))
        {
            throw new ArgumentException($"Invalid message format, both 'role' and 'content' must have a non-empty value.");
        }
        if (role != "user" && role != "assistant")
        {
            throw new ArgumentException($"Invalid message role '{role}', must be 'user' or 'assistant'.");
        }
        return (role, content);
    }

    public sealed record KnowledgeBaseRetrieveCommandResult(string RetrievalResult);
}
