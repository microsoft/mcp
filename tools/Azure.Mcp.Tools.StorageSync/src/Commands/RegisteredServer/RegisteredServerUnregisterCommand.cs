// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Options.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;

[CommandMetadata(
    Id = "346661e1-64be-463a-96c6-3626966f55fa",
    Name = "unregister",
    Title = "Unregister Server",
    Description = "Unregister a server from a Storage Sync service.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RegisteredServerUnregisterCommand(ILogger<RegisteredServerUnregisterCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RegisteredServerUnregisterOptions, RegisteredServerUnregisterCommand.RegisteredServerUnregisterCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerUnregisterCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RegisteredServerUnregisterOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Unregistering server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                options.Subscription, options.ResourceGroup, options.Name, options.ServerId);

            await _service.UnregisterServerAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.ServerId,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Server unregistered successfully";
            context.Response.Results = ResponseResult.Create(new("Server unregistered successfully"), StorageSyncJsonContext.Default.RegisteredServerUnregisterCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering server");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RegisteredServerUnregisterCommandResult(string Message);
}
