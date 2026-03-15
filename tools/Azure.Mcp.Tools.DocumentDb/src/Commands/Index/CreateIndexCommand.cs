// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Index;

public sealed class CreateIndexCommand(ILogger<CreateIndexCommand> logger)
    : BaseDocumentDbCommand<CreateIndexOptions>()
{
    private readonly ILogger<CreateIndexCommand> _logger = logger;

    public override string Id => "a5b6c7d8-e9f0-4a5b-2c3d-4e5f6a7b8c9d";

    public override string Name => "create_index";

    public override string Description => "Create an index on a collection";

    public override string Title => "Create Index";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.Keys);
        command.Options.Add(DocumentDbOptionDefinitions.Options);
    }

    protected override CreateIndexOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.Keys = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Keys.Name);
        options.Options = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Options.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        CreateIndexOptions? commandOptions = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = commandOptions = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();

            var keys = DocumentDbHelpers.ParseBsonDocument(options.Keys);
            if (keys == null)
            {
                throw new ArgumentException("Invalid keys format");
            }

            var indexOptions = DocumentDbHelpers.ParseBsonDocument(options.Options);

            DocumentDbResponse result = await service.CreateIndexAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, keys, indexOptions, cancellationToken);

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create index on collection: {CollectionName}, database: {DbName}, keys: {Keys}", commandOptions?.CollectionName, commandOptions?.DbName, commandOptions?.Keys);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
