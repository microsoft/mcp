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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Collection;

public sealed class RenameCollectionCommand(ILogger<RenameCollectionCommand> logger)
    : BaseDocumentDbCommand<RenameCollectionOptions>(logger)
{
    public override string Id => "c9d0e1f2-a3b4-4c9d-6e7f-8a9b0c1d2e3f";

    public override string Name => "rename_collection";

    public override string Description => "Rename a collection";

    public override string Title => "Rename Collection";

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
        command.Options.Add(DocumentDbOptionDefinitions.NewCollectionName);
    }

    protected override RenameCollectionOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        options.NewCollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.NewCollectionName.Name);
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

            var result = await service.RenameCollectionAsync(options.DbName!, options.CollectionName!, options.NewCollectionName!, cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rename collection from {OldName} to {NewName} in database: {DbName}", options.CollectionName, options.NewCollectionName, options.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
