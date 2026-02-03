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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Collection;

public sealed class SampleDocumentsCommand(ILogger<SampleDocumentsCommand> logger)
    : BaseDocumentDbCommand<SampleDocumentsOptions>(logger)
{
    public override string Id => "e1f2a3b4-c5d6-4e1f-8a9b-0c1d2e3f4a5b";

    public override string Name => "sample_documents";

    public override string Description => "Retrieve sample documents from a specific collection. Useful for understanding data schema and query generation";

    public override string Title => "Sample Documents";

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
        command.Options.Add(DocumentDbOptionDefinitions.SampleSize);
    }

    protected override SampleDocumentsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.SampleSize = parseResult.GetValueOrDefault<int>(DocumentDbOptionDefinitions.SampleSize.Name);
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

            var result = await service.SampleDocumentsAsync(options.DbName!, options.CollectionName!, options.SampleSize);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result.Select(doc => DocumentDbHelpers.SerializeBsonToJson(doc))));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sample documents from collection: {CollectionName} in database: {DbName} with sample size: {SampleSize}", options.CollectionName, options.DbName, options.SampleSize);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
