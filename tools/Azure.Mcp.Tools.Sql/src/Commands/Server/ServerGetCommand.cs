// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

public sealed class ServerGetCommand(ISqlService sqlService, ILogger<ServerGetCommand> logger)
    : SubscriptionCommand<ServerGetOptions, ServerGetCommand.ServerGetCommandResult>
{
    protected override JsonTypeInfo<ServerGetCommandResult> ResultTypeInfo => SqlJsonContext.Default.ServerGetCommandResult;

    private const string CommandTitle = "Get SQL Server";
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<ServerGetCommand> _logger = logger;

    public override string Id => "7f9a1c3e-5b7d-4a6c-8e0f-2b4d6a8c0e1f";

    public override string Name => "get";

    public override string Description =>
        """
        Show, get, or list Azure SQL servers in a resource group. Shows details for a specific Azure SQL server
        by name, or lists all Azure SQL servers in the specified resource group. Use to show, display, or
        retrieve Azure SQL server information. Equivalent to 'az sql server show' (show one Azure SQL server) or
        'az sql server list' (list all Azure SQL servers in a resource group). Returns server information
        including configuration details and current state.
        """;

    public override string Title => CommandTitle;

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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(SqlOptionDefinitions.Server.AsOptional());
    }

    protected override ServerGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Server = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.Server.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            if (!string.IsNullOrEmpty(options.Server))
            {
                var server = await _sqlService.GetServerAsync(
                    options.Server,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.RetryPolicy,
                    cancellationToken);

                SetResult(context, new ServerGetCommandResult([server]));
            }
            else
            {
                var servers = await _sqlService.ListServersAsync(
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.RetryPolicy,
                    cancellationToken);

                SetResult(context, new ServerGetCommandResult(servers ?? []));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL server(s). Server: {Server}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record ServerGetCommandResult(List<SqlServer> Servers);
}
