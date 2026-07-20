// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;

[CommandMetadata(
    Id = "8e2f6d1b-3c9a-4f7e-b2d5-7a8c3e4f5b6d",
    Name = "cancel",
    Title = "Cancel Azure Managed Lustre Autoexport Job",
    Description = """
        Cancels a running auto export job for an Azure Managed Lustre filesystem. This stops the ongoing sync operation from the Lustre filesystem to the linked blob storage container. Use this to terminate an autoexport job that is in progress.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoexport job to cancel
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoexportJobCancelCommand(IManagedLustreService service, ILogger<AutoexportJobCancelCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoexportJobCancelOptions, AutoexportJobCancelCommand.AutoexportJobCancelResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoexportJobCancelCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoexportJobCancelOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _service.CancelAutoexportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!, "Cancelled"), ManagedLustreJsonContext.Default.AutoexportJobCancelResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling autoexport job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoexportJobCancelResult(string JobName, string Status);
}
