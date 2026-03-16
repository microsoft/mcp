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
using MongoDB.Bson.Serialization;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class ExplainQueryCommand(ILogger<ExplainQueryCommand> logger)
    : BaseDocumentDbCommand<ExplainQueryOptions>()
{
    private readonly ILogger<ExplainQueryCommand> _logger = logger;

    public override string Id => "111d3104-aac1-49e4-b3c9-e75f74aca73d";

    public override string Name => "explain_query";

    public override string Description => "Explain a find, count, or aggregate operation for a collection by passing an operation-specific JSON body with --query-body.";

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
        command.Options.Add(DocumentDbOptionDefinitions.QueryBody);
    }

    protected override ExplainQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Operation = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Operation.Name);
        options.QueryBody = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.QueryBody.Name);
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
            var queryBody = ParseQueryBody(options.QueryBody);

            var result = options.Operation switch
            {
                "count" => await service.ExplainCountQueryAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, GetCountFilter(queryBody), cancellationToken),
                "aggregate" => await service.ExplainAggregateQueryAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, GetAggregatePipeline(queryBody), cancellationToken),
                _ => await service.ExplainFindQueryAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, GetFindFilter(queryBody), GetFindOptions(queryBody), cancellationToken)
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

    private static BsonDocument? ParseQueryBody(string? queryBody)
    {
        return DocumentDbHelpers.ParseBsonDocument(queryBody);
    }

    private static BsonDocument? GetFindFilter(BsonDocument? queryBody)
    {
        EnsureAllowedFields(queryBody, "filter", "options");
        return GetOptionalDocument(queryBody, "filter");
    }

    private static BsonDocument? GetFindOptions(BsonDocument? queryBody)
    {
        EnsureAllowedFields(queryBody, "filter", "options");
        return GetOptionalDocument(queryBody, "options");
    }

    private static BsonDocument? GetCountFilter(BsonDocument? queryBody)
    {
        EnsureAllowedFields(queryBody, "filter");
        return GetOptionalDocument(queryBody, "filter");
    }

    private static List<BsonDocument> GetAggregatePipeline(BsonDocument? queryBody)
    {
        EnsureAllowedFields(queryBody, "pipeline");

        if (queryBody == null || !queryBody.TryGetValue("pipeline", out var pipelineValue))
        {
            throw new ArgumentException("The --query-body JSON must contain a 'pipeline' array when --operation is 'aggregate'.");
        }

        if (pipelineValue is not BsonArray pipelineArray)
        {
            throw new ArgumentException("The 'pipeline' field in --query-body must be a JSON array.");
        }

        try
        {
            return pipelineArray.Select(item => item.AsBsonDocument).ToList();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Each item in the 'pipeline' array must be a JSON document.", ex);
        }
    }

    private static BsonDocument? GetOptionalDocument(BsonDocument? queryBody, string fieldName)
    {
        if (queryBody == null || !queryBody.TryGetValue(fieldName, out var value))
        {
            return null;
        }

        if (value is not BsonDocument document)
        {
            throw new ArgumentException($"The '{fieldName}' field in --query-body must be a JSON document.");
        }

        return document;
    }

    private static void EnsureAllowedFields(BsonDocument? queryBody, params string[] allowedFields)
    {
        if (queryBody == null)
        {
            return;
        }

        var disallowedFields = queryBody.Names.Where(name => !allowedFields.Contains(name, StringComparer.Ordinal)).ToList();

        if (disallowedFields.Count > 0)
        {
            throw new ArgumentException($"The --query-body JSON contains unsupported fields for this operation: {string.Join(", ", disallowedFields)}.");
        }
    }
}
