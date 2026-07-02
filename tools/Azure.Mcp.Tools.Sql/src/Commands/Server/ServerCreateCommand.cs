// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

[CommandMetadata(
    Id = "43f5f55d-2f21-47ac-b7f3-53f5d51b5218",
    Name = "create",
    Title = "Create SQL Server",
    Description = """
        Creates a new Azure SQL server in the specified resource group and location, with the specified administrator
        credentials and optional configuration settings. Returns the created server with its properties including the
        fully qualified domain name.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerCreateCommand(ISqlService sqlService, ILogger<ServerCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerCreateOptions, ServerCreateCommand.ServerCreateResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<ServerCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var server = await _sqlService.CreateServerAsync(
                options.Server,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                options.AdministratorLogin,
                options.AdministratorPassword,
                options.Version,
                options.PublicNetworkAccess,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(server), SqlJsonContext.Default.ServerCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Location: {Location}",
                options.Server, options.ResourceGroup, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A SQL server with this name already exists. Choose a different server name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Invalid request parameters for SQL server creation: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ServerCreateResult(SqlServer Server);
}
