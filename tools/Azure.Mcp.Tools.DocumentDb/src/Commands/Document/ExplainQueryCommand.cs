// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using MongoDB.Bson;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class ExplainQueryCommand(ILogger<ExplainQueryCommand> logger)
    : BaseDocumentDbCommand<ExplainQueryOptions>()
{
    private readonly ILogger<ExplainQueryCommand> _logger = logger;

    public override string Id => "111d3104-aac1-49e4-b3c9-e75f74aca73d";

    public override string Name => "explain_query";

    public override string Description => "Explain a find, count, or aggregate operation for a collection. Use an optional --filter for find and count, or --pipeline for aggregate.";

    public override string Title => "Explain Query";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.Operation);
        command.Options.Add(DocumentDbOptionDefinitions.Filter);
        command.Options.Add(DocumentDbOptionDefinitions.Options);
        command.Options.Add(DocumentDbOptionDefinitions.Pipeline.AsOptional());
    }

    protected override ExplainQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Operation = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Operation.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Filter.Name);
        options.Options = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Options.Name);
        options.Pipeline = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Pipeline.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        ExplainQueryOptions? options = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            options = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();
            var filter = DocumentDbHelpers.ParseBsonDocument(options.Filter);
            var queryOptions = DocumentDbHelpers.ParseBsonDocument(options.Options);

            var result = options.Operation switch
            {
                "count" => await service.ExplainCountQueryAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, cancellationToken),
                "aggregate" => await service.ExplainAggregateQueryAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, ParsePipeline(options.Pipeline), cancellationToken),
                _ => await service.ExplainFindQueryAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, filter, queryOptions, cancellationToken)
            };

            DocumentDbResponseHelper.ProcessResponse(context, result);
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to explain operation on collection: {CollectionName}, database: {DbName}, operation: {Operation}", options?.CollectionName, options?.DbName, options?.Operation);
            HandleException(context, ex);
            return context.Response;
        }
    }

    private static List<BsonDocument> ParsePipeline(string? pipeline)
    {
        var parsedPipeline = DocumentDbHelpers.ParseBsonDocumentList(pipeline);

        if (parsedPipeline == null || parsedPipeline.Count == 0)
        {
            throw new ArgumentException("Invalid pipeline format or empty pipeline");
        }

        return parsedPipeline;
    }
}
