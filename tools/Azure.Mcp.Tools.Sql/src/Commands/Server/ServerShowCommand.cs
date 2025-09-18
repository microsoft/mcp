// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

public sealed class ServerShowCommand(ILogger<ServerShowCommand> logger)
    : BaseSqlCommand<ServerShowOptions>(logger)
{
    private const string CommandTitle = "Show SQL Server";

    public override string Name => "show";

    public override string Description =>
        """
        Retrieves detailed information about an Azure SQL server including its configuration,
        status, and properties such as the fully qualified domain name, version,
        administrator login, and network access settings.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

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

            var server = await sqlService.GetServerAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new ServerShowResult(server),
                SqlJsonContext.Default.ServerShowResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException =>
            "SQL server not found in the specified resource group. Verify the server name and resource group.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server not found in the specified resource group. Verify the server name and resource group.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed retrieving the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => 404,
        RequestFailedException reqEx => reqEx.Status,
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    internal record ServerShowResult(SqlServer Server);
}
