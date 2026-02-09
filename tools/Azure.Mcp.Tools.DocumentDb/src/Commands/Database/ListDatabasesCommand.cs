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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Database;

public sealed class ListDatabasesCommand(ILogger<ListDatabasesCommand> logger)
    : BaseDocumentDbCommand<ListDatabasesOptions>()
{
    private readonly ILogger<ListDatabasesCommand> _logger = logger;

    public override string Id => "d4e5f6a7-b8c9-4d4e-1f2a-3b4c5d6e7f8a";

    public override string Name => "list_databases";

    public override string Description => "List all databases in the DocumentDB instance";

    public override string Title => "List Databases";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IDocumentDbService>();

            var result = await service.ListDatabasesAsync(cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list databases");
            HandleException(context, ex);
            return context.Response;
        }
    }
}
