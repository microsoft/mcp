// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Database;

public sealed class ListDatabasesCommand(ILogger<ListDatabasesCommand> logger)
    : BaseDocumentDbCommand<ListDatabasesOptions>()
{
    private readonly ILogger<ListDatabasesCommand> _logger = logger;

    public override string Id => "d4e5f6a7-b8c9-4d4e-1f2a-3b4c5d6e7f8a";

    public override string Name => "list_databases";

    public override string Description => "Return DocumentDB databases. If --db-name is omitted, returns an array of database names. If --db-name is provided, returns an array containing detailed information for that database.";

    public override string Title => "List Databases";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.DbName.AsOptional());
    }

    protected override ListDatabasesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        ListDatabasesOptions? options = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            options = BindOptions(parseResult);
            var dbName = options.DbName;

            var service = context.GetService<IDocumentDbService>();

            var result = await service.GetDatabasesAsync(dbName, cancellationToken);

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get DocumentDB database details. Database: {DbName}", options?.DbName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
