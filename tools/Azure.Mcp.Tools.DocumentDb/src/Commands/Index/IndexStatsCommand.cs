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

public sealed class IndexStatsCommand(ILogger<IndexStatsCommand> logger)
    : BaseDocumentDbCommand<IndexStatsOptions>(logger)
{
    public override string Id => "d8e9f0a1-b2c3-4d8e-5f6a-7b8c9d0e1f2a";

    public override string Name => "index_stats";

    public override string Description => "Get statistics for indexes on a collection";

    public override string Title => "Index Statistics";

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

    protected override IndexStatsOptions BindOptions(ParseResult parseResult)
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

            var result = await service.GetIndexStatsAsync(options.DbName!, options.CollectionName!, cancellationToken);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result.Select(doc => DocumentDbHelpers.SerializeBsonToJson(doc))));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get index statistics for collection: {CollectionName}, database: {DbName}", options.CollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
