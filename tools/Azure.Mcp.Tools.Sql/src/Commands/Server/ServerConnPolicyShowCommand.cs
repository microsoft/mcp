// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

public sealed class ServerConnPolicyShowCommand(ILogger<ServerConnPolicyShowCommand> logger)
    : BaseSqlCommand<ServerConnPolicyShowOptions>(logger)
{
    private const string CommandTitle = "Show SQL Server Connection Policy";

    public override string Name => "show";

    public override string Description =>
        """
        Retrieves the connection policy for an Azure SQL Server. The connection policy determines
        how clients connect to the SQL server and can be one of: Default (uses Azure defaults),
        Proxy (all connections are proxied through Azure gateway), or Redirect (connections are
        redirected directly to the database node).
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

            var connectionPolicy = await sqlService.GetServerConnectionPolicyAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(connectionPolicy), SqlJsonContext.Default.ServerConnPolicyShowResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving SQL server connection policy. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException =>
            "SQL server connection policy not found. Verify the server name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server or connection policy not found. Verify the server name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed retrieving the SQL server connection policy. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => HttpStatusCode.NotFound,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    internal record ServerConnPolicyShowResult(SqlServerConnectionPolicy ConnectionPolicy);
}
