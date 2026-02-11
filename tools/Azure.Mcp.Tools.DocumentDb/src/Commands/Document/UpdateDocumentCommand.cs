// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class UpdateDocumentCommand(ILogger<UpdateDocumentCommand> logger)
    : BaseDocumentDbCommand<UpdateDocumentOptions>()
{
    private readonly ILogger<UpdateDocumentCommand> _logger = logger;

    public override string Id => "d6e7f8a9-b0c1-4d6e-3f4a-5b6c7d8e9f0a";

    public override string Name => "update_document";

    public override string Description => "Update a document in a collection";

    public override string Title => "Update Single Document";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.Filter);
        command.Options.Add(DocumentDbOptionDefinitions.Update);
        command.Options.Add(DocumentDbOptionDefinitions.Upsert);
    }

    protected override UpdateDocumentOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Filter.Name);
        options.Update = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Update.Name);
        options.Upsert = parseResult.GetValueOrDefault<bool>(DocumentDbOptionDefinitions.Upsert.Name);
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

            var filter = DocumentDbHelpers.ParseBsonDocument(options.Filter);
            var update = DocumentDbHelpers.ParseBsonDocument(options.Update);

            if (filter == null || update == null)
            {
                throw new ArgumentException("Invalid filter or update format");
            }

            var result = await service.UpdateDocumentAsync(options.DbName!, options.CollectionName!, filter, update, options.Upsert, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document in collection: {CollectionName}, database: {DbName}, filter: {Filter}, update: {Update}, upsert: {Upsert}", options.CollectionName, options.DbName, options.Filter, options.Update, options.Upsert);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
