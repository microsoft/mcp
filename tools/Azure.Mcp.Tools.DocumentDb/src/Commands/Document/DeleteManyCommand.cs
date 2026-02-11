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

public sealed class DeleteManyCommand(ILogger<DeleteManyCommand> logger)
    : BaseDocumentDbCommand<DeleteManyOptions>()
{
    private readonly ILogger<DeleteManyCommand> _logger = logger;

    public override string Id => "a9b0c1d2-e3f4-4a9b-6c7d-8e9f0a1b2c3d";

    public override string Name => "delete_many";

    public override string Description => "Delete multiple documents from a collection";

    public override string Title => "Delete Many Documents";

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
    }

    protected override DeleteManyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Filter.Name);
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

            if (filter == null)
            {
                throw new ArgumentException("Invalid filter format");
            }

            var result = await service.DeleteManyAsync(options.DbName!, options.CollectionName!, filter, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete multiple documents from collection: {CollectionName}, database: {DbName}, filter: {Filter}", options.CollectionName, options.DbName, options.Filter);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
