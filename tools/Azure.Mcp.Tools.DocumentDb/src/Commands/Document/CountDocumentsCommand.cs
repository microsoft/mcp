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

public sealed class CountDocumentsCommand(ILogger<CountDocumentsCommand> logger)
    : BaseDocumentDbCommand<CountDocumentsOptions>()
{
    private readonly ILogger<CountDocumentsCommand> _logger = logger;

    public override string Id => "a3b4c5d6-e7f8-4a3b-0c1d-2e3f4a5b6c7d";

    public override string Name => "count_documents";

    public override string Description => "Count documents in a collection matching a query";

    public override string Title => "Count Documents";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.Query);
    }

    protected override CountDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Query = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Query.Name);
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

            var result = await service.CountDocumentsAsync(options.DbName!, options.CollectionName!, query, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count documents in collection: {CollectionName}, database: {DbName}, query: {Query}", options.CollectionName, options.DbName, options.Query);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
