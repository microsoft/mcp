// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Connection;

public sealed class ConnectionToggleCommand(ILogger<ConnectionToggleCommand> logger)
    : BaseDocumentDbCommand<ConnectionToggleOptions>()
{
    private const string ConnectAction = "connect";
    private const string DisconnectAction = "disconnect";

    private readonly ILogger<ConnectionToggleCommand> _logger = logger;

    public override string Id => "a1b2c3d4-e5f6-4a1b-8c9d-0e1f2a3b4c5d";

    public override string Name => "toggle";

    public override string Description => "Open, connect, switch, close, or disconnect the active Azure DocumentDB session for Azure Cosmos DB for MongoDB (vCore). Use this when the user wants to connect with a connection string, reconnect to a different cluster, or end the current DocumentDB session before running database commands.";

    public override string Title => "Connect or disconnect DocumentDB";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.Action);
        command.Options.Add(DocumentDbOptionDefinitions.ConnectionString);
        command.Options.Add(DocumentDbOptionDefinitions.TestConnection);
        command.Validators.Add(commandResult =>
        {
            var action = commandResult.GetValueOrDefault(DocumentDbOptionDefinitions.Action);
            var connectionString = commandResult.GetValueOrDefault(DocumentDbOptionDefinitions.ConnectionString);

            if (string.Equals(action, ConnectAction, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(connectionString))
            {
                commandResult.AddError($"Missing Required option: {DocumentDbOptionDefinitions.ConnectionString.Name}");
            }
        });
    }

    protected override ConnectionToggleOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Action = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.Action.Name);
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

            var result = options.Action switch
            {
                ConnectAction => await service.ConnectAsync(options.ConnectionString!, options.TestConnection, cancellationToken),
                DisconnectAction => await service.DisconnectAsync(cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported connection action '{options.Action}'.")
            };

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute DocumentDB connection action {Action}", options.Action);
            HandleException(context, ex);
            return context.Response;
        }
    }
}