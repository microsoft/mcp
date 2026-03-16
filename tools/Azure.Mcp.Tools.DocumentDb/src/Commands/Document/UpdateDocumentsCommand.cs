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

public sealed class UpdateDocumentsCommand(ILogger<UpdateDocumentsCommand> logger)
    : BaseDocumentDbCommand<UpdateDocumentsOptions>()
{
    private readonly ILogger<UpdateDocumentsCommand> _logger = logger;

    public override string Id => "16632ed3-edcc-44c5-aab6-38ca63a15521";

    public override string Name => "update_documents";

    public override string Description => "Update one or more documents in a collection matching a required filter. If --mode is omitted, the command defaults to single. Use --mode many to update multiple documents.";

    public override string Title => "Update Documents";

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
        command.Options.Add(DocumentDbOptionDefinitions.Update);
        command.Options.Add(DocumentDbOptionDefinitions.Upsert);
        command.Options.Add(DocumentDbOptionDefinitions.Mode);
    }

    protected override UpdateDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Filter.Name);
        options.Update = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Update.Name);
        options.Upsert = parseResult.GetValueOrDefault<bool>(DocumentDbOptionDefinitions.Upsert.Name);
        options.Mode = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Mode.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        UpdateDocumentsOptions? options = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            options = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();
            var filter = DocumentDbHelpers.ParseBsonDocument(options.Filter);
            var update = DocumentDbHelpers.ParseBsonDocument(options.Update);

            if (filter == null || update == null)
            {
                throw new ArgumentException("Invalid filter or update format");
            }

            var result = options.Mode switch
            {
                "many" => await service.UpdateManyAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, update, options.Upsert, cancellationToken),
                _ => await service.UpdateDocumentAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, update, options.Upsert, cancellationToken)
            };

            DocumentDbResponseHelper.ProcessResponse(context, result);
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update documents in collection: {CollectionName}, database: {DbName}, mode: {Mode}, upsert: {Upsert}", options?.CollectionName, options?.DbName, options?.Mode, options?.Upsert);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
