// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

[CommandMetadata(
    Id = "381bd0ef-5bb4-45ed-ae51-d129dcc044b2",
    Name = "delete",
    Title = "Delete SQL Server",
    Description = """
        Remove the specified SQL server from your Azure subscription, including all associated databases.
        This operation permanently deletes all server data and cannot be reversed.
        Use --force to bypass confirmation.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerDeleteCommand(ISqlService sqlService, ILogger<ServerDeleteCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerDeleteOptions, ServerDeleteCommand.ServerDeleteResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<ServerDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Show warning about destructive operation unless force is specified
            if (!options.Force)
            {
                context.Response.Status = HttpStatusCode.OK;
                context.Response.Message =
                    $"WARNING: This operation will permanently delete the SQL server '{options.Server}' " +
                    $"and ALL its databases in resource group '{options.ResourceGroup}'. " +
                    $"This action cannot be undone. Use --force to confirm deletion.";
                return context.Response;
            }

            var deleted = await _sqlService.DeleteServerAsync(
                options.Server,
                options.ResourceGroup,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            if (deleted)
            {
                context.Response.Results = ResponseResult.Create(new($"SQL server '{options.Server}' was successfully deleted.", true), SqlJsonContext.Default.ServerDeleteResult);
            }
            else
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = $"SQL server '{options.Server}' not found in resource group '{options.ResourceGroup}'.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"The given SQL server not found. It may have already been deleted.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed deleting the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Cannot delete SQL server due to a conflict. It may be in use or have dependent resources. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ServerDeleteResult(string Message, bool Success);
}
