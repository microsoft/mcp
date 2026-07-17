// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;

[CommandMetadata(
    Id = "fe3b07c3-9a11-465e-bfb6-6461b85b2e52",
    Name = "get",
    Title = "Get Registered Server",
    Description = "List all registered servers in a Storage Sync service or retrieve details about a specific registered server. Returns server properties including server ID, registration status, agent version, OS version, and last heartbeat. Use --server-id for a specific server.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RegisteredServerGetCommand(ILogger<RegisteredServerGetCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RegisteredServerGetOptions, RegisteredServerGetCommand.RegisteredServerGetCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RegisteredServerGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If server ID is provided, get specific server
            if (!string.IsNullOrEmpty(options.ServerId))
            {
                _logger.LogInformation("Getting registered server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                    options.Subscription, options.ResourceGroup, options.Name, options.ServerId);

                var server = await _service.GetRegisteredServerAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.ServerId,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (server == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Registered server not found";
                    return context.Response;
                }

                context.Response.Results = ResponseResult.Create(new([server]), StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
            }
            else
            {
                // List all registered servers
                _logger.LogInformation("Listing registered servers. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                    options.Subscription, options.ResourceGroup, options.Name);

                var servers = await _service.ListRegisteredServersAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(servers ?? []), StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registered server(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RegisteredServerGetCommandResult(List<RegisteredServerDataSchema> Results);
}
