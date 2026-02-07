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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Index;

public sealed class DropIndexCommand(ILogger<DropIndexCommand> logger)
    : BaseDocumentDbCommand<DropIndexOptions>(logger)
{
    public override string Id => "c7d8e9f0-a1b2-4c7d-4e5f-6a7b8c9d0e1f";

    public override string Name => "drop_index";

    public override string Description => "Drop an index from a collection";

    public override string Title => "Drop Index";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName);
        command.Options.Add(DocumentDbOptionDefinitions.IndexName);
    }

    protected override DropIndexOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.IndexName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.IndexName.Name);
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

            var result = await service.DropIndexAsync(options.DbName!, options.CollectionName!, options.IndexName!, cancellationToken);

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to drop index: {IndexName} from collection: {CollectionName}, database: {DbName}", options.IndexName, options.CollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
