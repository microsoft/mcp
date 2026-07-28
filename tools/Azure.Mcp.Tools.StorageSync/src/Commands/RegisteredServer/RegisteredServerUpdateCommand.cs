// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    Id = "c443ed00-f17f-46a8-a5d3-df128aa1606b",
    Name = "update",
    Title = "Update Registered Server",
    Description = "Update properties of a registered server.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RegisteredServerUpdateCommand(ILogger<RegisteredServerUpdateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RegisteredServerUpdateOptions, RegisteredServerUpdateCommand.RegisteredServerUpdateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerUpdateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RegisteredServerUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating registered server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                options.Subscription, options.ResourceGroup, options.Name, options.ServerId);

            var server = await _service.UpdateServerAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.ServerId,
                null, // TODO (alzimmer): Doesn't appear this command actually updates anything.
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(server), StorageSyncJsonContext.Default.RegisteredServerUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registered server");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RegisteredServerUpdateCommandResult(RegisteredServerDataSchema Result);
}
