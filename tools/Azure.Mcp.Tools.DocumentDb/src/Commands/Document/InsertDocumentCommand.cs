// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class InsertDocumentCommand(ILogger<InsertDocumentCommand> logger)
    : BaseDocumentDbCommand<InsertDocumentOptions>(logger)
{
    public override string Id => "b4c5d6e7-f8a9-4b4c-1d2e-3f4a5b6c7d8e";

    public override string Name => "insert_document";

    public override string Description => "Insert a single document into a collection";

    public override string Title => "Insert Document";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.Document);
    }

    protected override InsertDocumentOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Document = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Document.Name);
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

            var document = DocumentDbHelpers.ParseBsonDocument(options.Document);
            if (document == null)
            {
                throw new ArgumentException("Invalid document format");
            }

            var result = await service.InsertDocumentAsync(options.DbName!, options.CollectionName!, document, cancellationToken);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert document into collection: {CollectionName}, database: {DbName}", options.CollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
