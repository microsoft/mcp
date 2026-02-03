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

public sealed class AggregateCommand(ILogger<AggregateCommand> logger)
    : BaseDocumentDbCommand<AggregateOptions>(logger)
{
    public override string Id => "b0c1d2e3-f4a5-4b0c-7d8e-9f0a1b2c3d4e";

    public override string Name => "aggregate";

    public override string Description => "Run an aggregation pipeline on a collection";

    public override string Title => "Aggregate Pipeline";

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
        command.Options.Add(DocumentDbOptionDefinitions.AllowDiskUse);
    }

    protected override AggregateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Pipeline = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Pipeline.Name);
        options.AllowDiskUse = parseResult.GetValueOrDefault<bool>(DocumentDbOptionDefinitions.AllowDiskUse.Name);
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

            var result = await service.AggregateAsync(options.DbName!, options.CollectionName!, pipeline, options.AllowDiskUse);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run aggregation pipeline on collection: {CollectionName}, database: {DbName}, allowDiskUse: {AllowDiskUse}", options.CollectionName, options.DbName, options.AllowDiskUse);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
