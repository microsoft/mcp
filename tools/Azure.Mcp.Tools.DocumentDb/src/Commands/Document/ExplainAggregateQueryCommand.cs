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

public sealed class ExplainAggregateQueryCommand(ILogger<ExplainAggregateQueryCommand> logger)
    : BaseDocumentDbCommand<ExplainAggregateQueryOptions>(logger)
{
    public override string Id => "f4a5b6c7-d8e9-4f4a-1b2c-3d4e5f6a7b8c";

    public override string Name => "explain_aggregate_query";

    public override string Description => "Explain the execution plan with execution stats for an aggregation query on a given collection";

    public override string Title => "Explain Aggregate Query";

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
        command.Options.Add(DocumentDbOptionDefinitions.Pipeline);
    }

    protected override ExplainAggregateQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Pipeline = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Pipeline.Name);
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

            var pipeline = DocumentDbHelpers.ParseBsonDocumentList(options.Pipeline);

            if (pipeline == null || pipeline.Count == 0)
            {
                throw new ArgumentException("Invalid pipeline format or empty pipeline");
            }

            var result = await service.ExplainAggregateQueryAsync(options.DbName!, options.CollectionName!, pipeline);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to explain aggregate query on collection: {CollectionName}, database: {DbName}", options.CollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
