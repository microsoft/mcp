// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class FindDocumentsCommand(ILogger<FindDocumentsCommand> logger)
    : BaseDocumentDbCommand<FindDocumentsOptions>(logger)
{
    public override string Id => "f2a3b4c5-d6e7-4f2a-9b0c-1d2e3f4a5b6c";

    public override string Name => "find_documents";

    public override string Description => "Find documents in a collection. Supports consolidated \"options\" object (limit, skip, sort, projection)";

    public override string Title => "Find Documents";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        var dbNameOption = new Option<string>("--db-name")
        {
            Description = "Database name",
            Required = true
        };
        command.Options.Add(dbNameOption);

        var collectionNameOption = new Option<string>("--collection-name")
        {
            Description = "Collection name",
            Required = true
        };
        command.Options.Add(collectionNameOption);

        command.Options.Add(DocumentDbOptionDefinitions.Query);
        command.Options.Add(DocumentDbOptionDefinitions.Options);
    }

    protected override FindDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Query = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Query.Name);
        options.Options = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Options.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IDocumentDbService>();

            var query = DocumentDbHelpers.ParseBsonDocument(options.Query);
            var queryOptions = string.IsNullOrWhiteSpace(options.Options) ? null : DocumentDbResponseHelper.DeserializeFromJson<object>(options.Options);

            var result = await service.FindDocumentsAsync(options.DbName!, options.CollectionName!, query, queryOptions, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find documents in collection: {CollectionName}, database: {DbName}, query: {Query}", options.CollectionName, options.DbName, options.Query);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
