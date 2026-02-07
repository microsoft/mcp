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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Connection;

public sealed class GetConnectionStatusCommand(ILogger<GetConnectionStatusCommand> logger)
    : BaseDocumentDbCommand<GetConnectionStatusOptions>(logger)
{
    public override string Id => "c3d4e5f6-a7b8-4c3d-0e1f-2a3b4c5d6e7f";

    public override string Name => "get_connection_status";

    public override string Description => "Get the current DocumentDB connection status and details";

    public override string Title => "Get Connection Status";

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
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IDocumentDbService>();

            var result = await service.GetConnectionStatusAsync(cancellationToken);

            // Process response using unified DocumentDbResponse type
            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get DocumentDB connection status");
            HandleException(context, ex);
            return context.Response;
        }
    }
}
