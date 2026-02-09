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

public sealed class InsertManyCommand(ILogger<InsertManyCommand> logger)
    : BaseDocumentDbCommand<InsertManyOptions>()
{
    private readonly ILogger<InsertManyCommand> _logger = logger;

    public override string Id => "c5d6e7f8-a9b0-4c5d-2e3f-4a5b6c7d8e9f";

    public override string Name => "insert_many";

    public override string Description => "Insert multiple documents into a collection";

    public override string Title => "Insert Many Documents";

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
        command.Options.Add(DocumentDbOptionDefinitions.Documents);
    }

    protected override InsertManyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Documents = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Documents.Name);
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

            var documents = DocumentDbHelpers.ParseBsonDocumentList(options.Documents);
            if (documents == null || documents.Count == 0)
            {
                throw new ArgumentException("Invalid documents format or empty list");
            }

            var result = await service.InsertManyAsync(options.DbName!, options.CollectionName!, documents, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert multiple documents into collection: {CollectionName}, database: {DbName}", options.CollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
