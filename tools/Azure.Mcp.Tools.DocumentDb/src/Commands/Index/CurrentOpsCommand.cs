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

public sealed class CurrentOpsCommand(ILogger<CurrentOpsCommand> logger)
    : BaseDocumentDbCommand<CurrentOpsOptions>(logger)
{
    public override string Id => "e9f0a1b2-c3d4-4e9f-6a7b-8c9d0e1f2a3b";

    public override string Name => "current_ops";

    public override string Description => "Get information about current DocumentDB operations";

    public override string Title => "Current Operations";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.Ops);
    }

    protected override CurrentOpsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Ops = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Ops.Name);
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

            var filter = DocumentDbHelpers.ParseBsonDocument(options.Ops);

            var result = await service.GetCurrentOpsAsync(filter, cancellationToken);

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current operations with filter: {Ops}", options.Ops);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
