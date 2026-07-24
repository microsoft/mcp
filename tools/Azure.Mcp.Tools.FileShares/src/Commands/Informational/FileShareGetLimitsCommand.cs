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
    Id = "a9e1f0b2-c3d4-4e5f-a6b7-c8d9e0f1a2b3",
    Name = "limits",
    Title = "Get File Share Limits",
    Description = "Get file share limits for a subscription and location",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareGetLimitsCommand(ILogger<FileShareGetLimitsCommand> logger, IFileSharesService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareGetLimitsOptions, FileShareLimitsResult>(subscriptionResolver)
{
    private readonly ILogger<FileShareGetLimitsCommand> _logger = logger;
    private readonly IFileSharesService _service = service;

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        FileShareGetLimitsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting file share limits for subscription {Subscription} in location {Location}",
                options.Subscription, options.Location);

            var result = await _service.GetLimitsAsync(
                options.Subscription!,
                options.Location,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareLimitsResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file share limits. Subscription: {Subscription}, Location: {Location}.", options.Subscription, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
