// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

public sealed class ServerCreateCommand(ILogger<ServerCreateCommand> logger)
    : BaseSqlCommand<ServerCreateOptions>(logger)
{
    private const string CommandTitle = "Create SQL Server";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a new Azure SQL server in the specified resource group and location.
        The server will be created with the specified administrator credentials and
        optional configuration settings. Returns the created server with its properties
        including the fully qualified domain name.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.AdministratorLoginOption);
        command.Options.Add(SqlOptionDefinitions.AdministratorPasswordOption);
        command.Options.Add(SqlOptionDefinitions.LocationOption);
        command.Options.Add(SqlOptionDefinitions.VersionOption);
        command.Options.Add(SqlOptionDefinitions.PublicNetworkAccessOption);
    }

    protected override ServerCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AdministratorLogin = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.AdministratorLoginOption.Name);
        options.AdministratorPassword = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.AdministratorPasswordOption.Name);
        options.Location = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.LocationOption.Name);
        options.Version = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.VersionOption.Name);
        options.PublicNetworkAccess = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.PublicNetworkAccessOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var sqlService = context.GetService<ISqlService>();

            var server = await sqlService.CreateServerAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Location!,
                options.AdministratorLogin!,
                options.AdministratorPassword!,
                options.Version,
                options.PublicNetworkAccess,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(server), SqlJsonContext.Default.ServerCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Location: {Location}, Options: {@Options}",
                options.Server, options.ResourceGroup, options.Location, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "A SQL server with this name already exists. Choose a different server name.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed creating the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == 400 =>
            $"Invalid request parameters for SQL server creation: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    internal record ServerCreateResult(SqlServer Server);
}
