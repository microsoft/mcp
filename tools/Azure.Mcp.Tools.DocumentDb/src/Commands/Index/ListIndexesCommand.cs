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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Index;

public sealed class ListIndexesCommand(ILogger<ListIndexesCommand> logger)
    : BaseDocumentDbCommand<ListIndexesOptions>(logger)
{
    public override string Id => "b6c7d8e9-f0a1-4b6c-3d4e-5f6a7b8c9d0e";

    public override string Name => "list_indexes";

    public override string Description => "List all indexes on a collection";

    public override string Title => "List Indexes";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        var dbNameOption = new Option<string>("--db-name")
        {
            Description = "Database name",
            Required = true
        };
        command.Options.Add(dbNameOption);

        var collectionNameOption = new Option<string>("--collection-name")
        {
            Description = "Collection name",
            Required = true
        };
        command.Options.Add(collectionNameOption);
    }

    protected override ListIndexesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
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

            var result = await service.ListIndexesAsync(options.DbName!, options.CollectionName!, cancellationToken);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list indexes on collection: {CollectionName}, database: {DbName}", options.CollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
