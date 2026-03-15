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

public sealed class ListIndexesCommand(ILogger<ListIndexesCommand> logger)
    : BaseDocumentDbCommand<ListIndexesOptions>()
{
    private readonly ILogger<ListIndexesCommand> _logger = logger;

    public override string Id => "b6c7d8e9-f0a1-4b6c-3d4e-5f6a7b8c9d0e";

    public override string Name => "list_indexes";

    public override string Description => "List all indexes on a collection";

    public override string Title => "List Indexes";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
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
        ListIndexesOptions? commandOptions = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = commandOptions = BindOptions(parseResult);

            var service = context.GetService<IDocumentDbService>();

            DocumentDbResponse result = await service.ListIndexesAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, cancellationToken);

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list indexes on collection: {CollectionName}, database: {DbName}", commandOptions?.CollectionName, commandOptions?.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}