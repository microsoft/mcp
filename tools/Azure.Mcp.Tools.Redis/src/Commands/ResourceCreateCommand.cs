// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Options;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Redis.Commands;

/// <summary>
/// Creates a new Azure Managed Redis resource. Provisioning can take several minutes; the command waits
/// for the deployment to reach a terminal state and returns the actual resulting status, or an error if
/// the deployment failed.
/// </summary>
[CommandMetadata(
    Id = "750133dd-d57f-4ed4-9488-c1d406ad4a83",
    Name = "create",
    Title = "Create Redis Resource",
    Description = "Create/provision a new Azure Managed Redis resource in your Azure subscription. Provisioning is asynchronous and typically takes several minutes; the command waits for the deployment to complete and returns the resource's actual status, or an error if the deployment failed.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceCreateCommand(IRedisService redisService, ILogger<ResourceCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceCreateOptions, ResourceCreateCommand.ResourceCreateCommandResult>(subscriptionResolver)
{
    private readonly IRedisService _redisService = redisService;
    private readonly ILogger<ResourceCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resource = await _redisService.CreateResourceAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Resource,
                options.Location,
                options.Sku,
                options.AccessKeysAuthentication ?? false,
                options.PublicNetworkAccess ?? false,
                options.Modules,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(resource),
                RedisJsonContext.Default.ResourceCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Redis resource");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ResourceCreateCommandResult(Resource Resource);
}
