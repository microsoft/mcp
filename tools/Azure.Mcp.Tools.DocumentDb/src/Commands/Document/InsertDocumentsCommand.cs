// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class InsertDocumentsCommand(ILogger<InsertDocumentsCommand> logger)
    : BaseDocumentDbCommand<InsertDocumentsOptions>()
{
    private readonly ILogger<InsertDocumentsCommand> _logger = logger;

    public override string Id => "5217faf1-1323-4e68-b599-e680caff0c70";

    public override string Name => "insert_documents";

    public override string Description => "Insert one document or many documents into a collection. If --mode is omitted, the command auto-detects a single document versus a JSON array payload.";

    public override string Title => "Insert Documents";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
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
        command.Options.Add(DocumentDbOptionDefinitions.DocumentsPayload);
        command.Options.Add(DocumentDbOptionDefinitions.Mode);
    }

    protected override InsertDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Documents = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DocumentsPayload.Name);
        options.Mode = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Mode.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        InsertDocumentsOptions? options = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            options = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();
            var mode = ResolveMode(options.Documents, options.Mode, parseResult.CommandResult.HasOptionResult(DocumentDbOptionDefinitions.Mode));

            var result = mode switch
            {
                "many" => await service.InsertManyAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, ParseDocuments(options.Documents, mode), cancellationToken),
                _ => await service.InsertDocumentAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, ParseDocument(options.Documents, mode), cancellationToken)
            };

            DocumentDbResponseHelper.ProcessResponse(context, result);
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert documents into collection: {CollectionName}, database: {DbName}, mode: {Mode}", options?.CollectionName, options?.DbName, options?.Mode);
            HandleException(context, ex);
            return context.Response;
        }
    }

    private static string ResolveMode(string? payload, string? mode, bool modeSpecified)
    {
        var looksLikeArray = payload?.TrimStart().StartsWith('[') == true;

        if (!modeSpecified)
        {
            return looksLikeArray ? "many" : "single";
        }

        if (mode == "single" && looksLikeArray)
        {
            throw new ArgumentException("JSON array payloads require --mode many or omitting --mode for auto-detection.");
        }

        if (mode == "many" && !looksLikeArray)
        {
            throw new ArgumentException("--mode many requires a JSON array payload.");
        }

        return mode ?? "single";
    }

    private static BsonDocument ParseDocument(string? payload, string mode)
    {
        var document = DocumentDbHelpers.ParseBsonDocument(payload);

        if (document == null)
        {
            throw new ArgumentException($"Invalid {mode} document payload.");
        }

        return document;
    }

    private static List<BsonDocument> ParseDocuments(string? payload, string mode)
    {
        var documents = DocumentDbHelpers.ParseBsonDocumentList(payload);

        if (documents == null || documents.Count == 0)
        {
            throw new ArgumentException($"Invalid {mode} documents payload.");
        }

        return documents;
    }
}
