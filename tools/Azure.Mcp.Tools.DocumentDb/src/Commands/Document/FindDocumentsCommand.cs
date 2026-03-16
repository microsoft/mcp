// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class FindDocumentsCommand(ILogger<FindDocumentsCommand> logger)
    : BaseDocumentDbCommand<FindDocumentsOptions>()
{
    private readonly ILogger<FindDocumentsCommand> _logger = logger;

    public override string Id => "f2a3b4c5-d6e7-4f2a-9b0c-1d2e3f4a5b6c";

    public override string Name => "find_documents";

    public override string Description => "Find and retrieve documents from a collection matching an optional filter. Supports limit, skip, sort, and projection options.";

    public override string Title => "Find Documents";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.Filter);
        command.Options.Add(DocumentDbOptionDefinitions.Options);
    }

    protected override FindDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Filter.Name);
        options.Options = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Options.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        FindDocumentsOptions? options = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            options = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();

            var filter = DocumentDbHelpers.ParseBsonDocument(options.Filter);
            var queryOptions = DocumentDbHelpers.ParseBsonDocument(options.Options);

            var result = await service.FindDocumentsAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, queryOptions, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find documents in collection: {CollectionName}, database: {DbName}, filter: {Filter}", options?.CollectionName, options?.DbName, options?.Filter);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
