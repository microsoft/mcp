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

namespace Azure.Mcp.Tools.DocumentDb.Commands.Connection;

public sealed class ConnectDocumentDbCommand(ILogger<ConnectDocumentDbCommand> logger)
    : BaseDocumentDbCommand<ConnectDocumentDbOptions>(logger)
{
    public override string Id => "a1b2c3d4-e5f6-4a1b-8c9d-0e1f2a3b4c5d";

    public override string Name => "connect";

    public override string Description => "Connect to an Azure Cosmos DB for MongoDB (vCore) instance with a connection string";

    public override string Title => "Connect to DocumentDB";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.ConnectionString);
        command.Options.Add(DocumentDbOptionDefinitions.TestConnection);
    }

    protected override ConnectDocumentDbOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ConnectionString = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.ConnectionString.Name);
        options.TestConnection = parseResult.GetValueOrDefault<bool>(DocumentDbOptionDefinitions.TestConnection.Name);
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

            var result = await service.ConnectAsync(options.ConnectionString!, options.TestConnection, cancellationToken);

            context.Response.Results = DocumentDbResponseHelper.CreateFromJson(
                DocumentDbResponseHelper.SerializeToJson(result));

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to DocumentDB with connection string: {ConnectionString}", options.ConnectionString);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
