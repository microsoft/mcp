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
    Id = "93d14ba8-5e75-4190-93dd-f47e932b849b",
    Name = "usage",
    Title = "Get File Share Usage Data",
    Description = "Get file share usage data for a subscription and location",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareGetUsageDataCommand(ILogger<FileShareGetUsageDataCommand> logger, IFileSharesService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareGetUsageDataOptions, FileShareUsageDataResult>(subscriptionResolver)
{
    private readonly ILogger<FileShareGetUsageDataCommand> _logger = logger;
    private readonly IFileSharesService _service = service;

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        FileShareGetUsageDataOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting file share usage data for subscription {Subscription} in location {Location}",
                options.Subscription, options.Location);

            var result = await _service.GetUsageDataAsync(
                options.Subscription!,
                options.Location,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareUsageDataResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file share usage data. Subscription: {Subscription}, Location: {Location}.", options.Subscription, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

