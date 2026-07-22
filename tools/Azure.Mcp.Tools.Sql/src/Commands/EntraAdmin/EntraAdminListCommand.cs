// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.EntraAdmin;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.EntraAdmin;

[CommandMetadata(
    Id = "240aac03-0eb0-4cd3-91f8-475577289186",
    Name = "list",
    Title = "List SQL Server Entra ID Administrators",
    Description = """
        Gets a list of all Microsoft Entra ID administrators for a SQL server, including their display names,
        object IDs, and tenant information. Returns an array of Entra ID administrator objects with their properties.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class EntraAdminListCommand(ISqlService sqlService, ILogger<EntraAdminListCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<EntraAdminListOptions, EntraAdminListCommand.EntraAdminListResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<EntraAdminListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EntraAdminListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var administrators = await _sqlService.GetEntraAdministratorsAsync(
                options.Server,
                options.ResourceGroup,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(administrators ?? []), SqlJsonContext.Default.EntraAdminListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL server Entra ID administrators. Server: {Server}, ResourceGroup: {ResourceGroup}.",
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

    public sealed record EntraAdminListResult(List<SqlServerEntraAdministrator> Administrators);
}
