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
using MongoDB.Bson;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Collection;

public sealed class CollectionStatsCommand(ILogger<CollectionStatsCommand> logger)
    : BaseDocumentDbCommand<CollectionStatsOptions>(logger)
{
    public override string Id => "b8c9d0e1-f2a3-4b8c-5d6e-7f8a9b0c1d2e";

    public override string Name => "collection_stats";

    public override string Description => "Get detailed statistics about a collection's size and storage usage";

    public override string Title => "Collection Statistics";

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
    }

    protected override CollectionStatsOptions BindOptions(ParseResult parseResult)
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

            var result = await service.GetCollectionStatsAsync(options.DbName!, options.CollectionName!, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection statistics for database: {DbName}, collection: {CollectionName}", options.DbName, options.CollectionName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
