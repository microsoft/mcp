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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Connection;

public sealed class DisconnectDocumentDbCommand(ILogger<DisconnectDocumentDbCommand> logger)
    : BaseDocumentDbCommand<DisconnectDocumentDbOptions>(logger)
{
    public override string Id => "b2c3d4e5-f6a7-4b2c-9d0e-1f2a3b4c5d6e";

    public override string Name => "disconnect";

    public override string Description => "Disconnect from the current DocumentDB instance";

    public override string Title => "Disconnect from DocumentDB";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false
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

            var result = await service.DisconnectAsync(cancellationToken);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from DocumentDB");
            HandleException(context, ex);
            return context.Response;
        }
    }
}
