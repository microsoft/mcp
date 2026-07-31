// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

/// <summary>
/// Checks if a file share name is available.
/// </summary>
[CommandMetadata(
    Id = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
    Name = "check-name-availability",
    Title = "Check File Share Name Availability",
    Description = "Check if a file share name is available in the specified location in the subscription.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareCheckNameAvailabilityCommand(ILogger<FileShareCheckNameAvailabilityCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareCheckNameAvailabilityOptions, FileShareCheckNameAvailabilityCommand.FileShareCheckNameAvailabilityCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileShareCheckNameAvailabilityOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Checking name availability for file share {FileShareName} in location {Location}",
                options.Name,
                options.Location);

            var availabilityResult = await fileSharesService.CheckNameAvailabilityAsync(
                options.Subscription!,
                options.Name,
                options.Location,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(availabilityResult.IsAvailable, availabilityResult.Reason, availabilityResult.Message),
                FileSharesJsonContext.Default.FileShareCheckNameAvailabilityCommandResult);

            logger.LogInformation(
                "Name availability check completed. File share name {FileShareName} is {Status}",
                options.Name,
                availabilityResult.IsAvailable ? "available" : "not available");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking file share name availability. FileShareName: {FileShareName}, Location: {Location}.", options.Name, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileShareCheckNameAvailabilityCommandResult(bool IsAvailable, string? Reason, string? Message);
}
