// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

[CommandMetadata(
    Id = "c6d7e8f9-a0b1-4c2d-3e4f-5a6b7c8d9e0f",
    Name = "update",
    Title = "Update Private Endpoint Connection",
    Description = "Update the state of a private endpoint connection for a file share. Use this to approve or reject private endpoint connection requests.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointConnectionUpdateCommand(ILogger<PrivateEndpointConnectionUpdateCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PrivateEndpointConnectionUpdateOptions, PrivateEndpointConnectionUpdateCommand.PrivateEndpointConnectionUpdateCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointConnectionUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Updating private endpoint connection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShare: {FileShareName}, Connection: {ConnectionName}, Status: {Status}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.ConnectionName, options.Status);

            var connection = await fileSharesService.UpdatePrivateEndpointConnectionAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FileShareName,
                options.ConnectionName,
                options.Status,
                options.Description,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(connection), FileSharesJsonContext.Default.PrivateEndpointConnectionUpdateCommandResult);

            logger.LogInformation(
                "Successfully updated private endpoint connection. Connection: {ConnectionName}, Status: {Status}",
                options.ConnectionName, options.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update private endpoint connection");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record PrivateEndpointConnectionUpdateCommandResult(PrivateEndpointConnectionInfo Connection);
}
