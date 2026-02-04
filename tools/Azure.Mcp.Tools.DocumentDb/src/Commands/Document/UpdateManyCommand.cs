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

public sealed class UpdateManyCommand(ILogger<UpdateManyCommand> logger)
    : BaseDocumentDbCommand<UpdateManyOptions>(logger)
{
    public override string Id => "e7f8a9b0-c1d2-4e7f-4a5b-6c7d8e9f0a1b";

    public override string Name => "update_many";

    public override string Description => "Update multiple documents in a collection";

    public override string Title => "Update Many Documents";

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

    protected override UpdateManyOptions BindOptions(ParseResult parseResult)
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

            var result = await service.UpdateManyAsync(options.DbName!, options.CollectionName!, filter, update, options.Upsert, cancellationToken);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update multiple documents in collection: {CollectionName}, database: {DbName}, filter: {Filter}, update: {Update}, upsert: {Upsert}", options.CollectionName, options.DbName, options.Filter, options.Update, options.Upsert);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
