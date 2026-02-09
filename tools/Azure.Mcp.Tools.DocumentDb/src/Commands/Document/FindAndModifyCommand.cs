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

public sealed class FindAndModifyCommand(ILogger<FindAndModifyCommand> logger)
    : BaseDocumentDbCommand<FindAndModifyOptions>()
{
    private readonly ILogger<FindAndModifyCommand> _logger = logger;

    public override string Id => "c1d2e3f4-a5b6-4c1d-8e9f-0a1b2c3d4e5f";

    public override string Name => "find_and_modify";

    public override string Description => "Find and modify (update) a document atomically, returning the document before modification. Use this for atomic find-update-return operations in a single step";

    public override string Title => "Find And Modify Document";

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
        command.Options.Add(DocumentDbOptionDefinitions.Query);
        command.Options.Add(DocumentDbOptionDefinitions.Update);
        command.Options.Add(DocumentDbOptionDefinitions.Upsert);
    }

    protected override FindAndModifyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Query = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Query.Name);
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

            var query = DocumentDbHelpers.ParseBsonDocument(options.Query);
            var update = DocumentDbHelpers.ParseBsonDocument(options.Update);

            if (query == null || update == null)
            {
                throw new ArgumentException("Invalid query or update format");
            }

            var result = await service.FindAndModifyAsync(options.DbName!, options.CollectionName!, query, update, options.Upsert, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find and modify document in collection: {CollectionName}, database: {DbName}, query: {Query}, update: {Update}, upsert: {Upsert}", options.CollectionName, options.DbName, options.Query, options.Update, options.Upsert);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
