// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class DeleteDocumentsCommand(ILogger<DeleteDocumentsCommand> logger)
    : BaseDocumentDbCommand<DeleteDocumentsOptions>()
{
    private readonly ILogger<DeleteDocumentsCommand> _logger = logger;

    public override string Id => "5c8844e8-07d8-461f-b640-d954a60d6420";

    public override string Name => "delete_documents";

    public override string Description => "Delete one or more documents from a collection matching a required filter. If --mode is omitted, the command defaults to single. Use --mode many to delete multiple documents.";

    public override string Title => "Delete Documents";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.RequiredFilter);
        command.Options.Add(DocumentDbOptionDefinitions.Mode);
    }

    protected override DeleteDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Filter.Name);
        options.Mode = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Mode.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        DeleteDocumentsOptions? options = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            options = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();
            var filter = DocumentDbHelpers.ParseBsonDocument(options.Filter);

            if (filter == null)
            {
                throw new ArgumentException("Invalid filter format");
            }

            var result = options.Mode switch
            {
                "many" => await service.DeleteManyAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, cancellationToken),
                _ => await service.DeleteDocumentAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, cancellationToken)
            };

            DocumentDbResponseHelper.ProcessResponse(context, result);
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete documents from collection: {CollectionName}, database: {DbName}, mode: {Mode}", options?.CollectionName, options?.DbName, options?.Mode);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
