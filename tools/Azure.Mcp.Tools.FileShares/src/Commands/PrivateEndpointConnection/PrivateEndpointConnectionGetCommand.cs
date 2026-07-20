// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
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
    Id = "a8e9f7d6-c5b4-4a3d-9e2f-1c0b8a7d6e5f",
    Name = "get",
    Title = "Get Private Endpoint Connection",
    Description = "Get details of a specific private endpoint connection or list all private endpoint connections for a file share. If --connection-name is provided, returns a specific connection; otherwise, lists all connections.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointConnectionGetCommand(ILogger<PrivateEndpointConnectionGetCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PrivateEndpointConnectionGetOptions, PrivateEndpointConnectionGetCommand.PrivateEndpointConnectionGetCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointConnectionGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If connection name is provided, get specific connection
            if (!string.IsNullOrEmpty(options.ConnectionName))
            {
                logger.LogInformation(
                    "Getting private endpoint connection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShare: {FileShareName}, Connection: {ConnectionName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName, options.ConnectionName);

                var connection = await fileSharesService.GetPrivateEndpointConnectionAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FileShareName,
                    options.ConnectionName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var singleResult = new PrivateEndpointConnectionGetCommandResult([connection]);
                context.Response.Results = ResponseResult.Create(singleResult, FileSharesJsonContext.Default.PrivateEndpointConnectionGetCommandResult);

                logger.LogInformation("Successfully retrieved private endpoint connection. Connection: {ConnectionName}", options.ConnectionName);
            }
            else
            {
                // List all connections
                logger.LogInformation(
                    "Listing private endpoint connections. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShare: {FileShareName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName);

                var connections = await fileSharesService.ListPrivateEndpointConnectionsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FileShareName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var result = new PrivateEndpointConnectionGetCommandResult(connections ?? []);
                context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.PrivateEndpointConnectionGetCommandResult);

                logger.LogInformation("Successfully listed private endpoint connections. Count: {Count}", connections?.Count ?? 0);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get private endpoint connection(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record PrivateEndpointConnectionGetCommandResult([property: JsonPropertyName("connections")] List<PrivateEndpointConnectionInfo> Connections);
}
