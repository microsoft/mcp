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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Document;

public sealed class ExplainFindQueryCommand(ILogger<ExplainFindQueryCommand> logger)
    : BaseDocumentDbCommand<ExplainFindQueryOptions>()
{
    private readonly ILogger<ExplainFindQueryCommand> _logger = logger;

    public override string Id => "d2e3f4a5-b6c7-4d2e-9f0a-1b2c3d4e5f6a";

    public override string Name => "explain_find_query";

    public override string Description => "Explain the execution plan with execution stats for a find query using consolidated options (sort, projection, limit, skip)";

    public override string Title => "Explain Find Query";

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
        command.Options.Add(DocumentDbOptionDefinitions.Query);
        command.Options.Add(DocumentDbOptionDefinitions.Options);
    }

    protected override ExplainFindQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Query = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Query.Name);
        options.Options = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Options.Name);
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

            var query = DocumentDbHelpers.ParseBsonDocument(options.Query);
            var queryOptions = string.IsNullOrWhiteSpace(options.Options) ? null : DocumentDbResponseHelper.DeserializeFromJson<object>(options.Options);

            var result = await service.ExplainFindQueryAsync(options.DbName!, options.CollectionName!, query, queryOptions, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to explain find query on collection: {CollectionName}, database: {DbName}, query: {Query}", options.CollectionName, options.DbName, options.Query);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
