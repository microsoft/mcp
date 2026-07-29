// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.Informational;

[CommandMetadata(
    Id = "3c5e1fb2-3a8d-4f8e-8b0a-1c2d3e4f5a6b",
    Name = "rec",
    Title = "Get File Share Provisioning Recommendation",
    Description = "Get provisioning parameter recommendations for a file share based on desired storage size",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareGetProvisioningRecommendationCommand(ILogger<FileShareGetProvisioningRecommendationCommand> logger, IFileSharesService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareGetProvisioningRecommendationOptions, FileShareProvisioningRecommendationResult>(subscriptionResolver)
{
    private readonly ILogger<FileShareGetProvisioningRecommendationCommand> _logger = logger;
    private readonly IFileSharesService _service = service;

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        FileShareGetProvisioningRecommendationOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting provisioning recommendation for subscription {Subscription} in location {Location} with storage {StorageGiB} GiB",
                options.Subscription, options.Location, options.ProvisionedStorageInGiB);

            var result = await _service.GetProvisioningRecommendationAsync(
                options.Subscription!,
                options.Location,
                options.ProvisionedStorageInGiB,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareProvisioningRecommendationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provisioning recommendation. Subscription: {Subscription}, Location: {Location}.", options.Subscription, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

