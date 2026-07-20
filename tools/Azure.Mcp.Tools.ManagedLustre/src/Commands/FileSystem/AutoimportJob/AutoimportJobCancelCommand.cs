// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;

[CommandMetadata(
    Id = "9f3g1h2i-4d0b-5g8f-c3e6-8b9d4f6g7c8h",
    Name = "cancel",
    Title = "Cancel Azure Managed Lustre Autoimport Job",
    Description = """
        Cancels a running auto import job for an Azure Managed Lustre filesystem. This stops the ongoing sync operation from the linked blob storage container to the Lustre filesystem. Use this to terminate an autoimport job that is in progress.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoimport job to cancel
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoimportJobCancelCommand(IManagedLustreService service, ILogger<AutoimportJobCancelCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoimportJobCancelOptions, AutoimportJobCancelCommand.AutoimportJobCancelResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoimportJobCancelCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoimportJobCancelOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _service.CancelAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!, "Cancelled"), ManagedLustreJsonContext.Default.AutoimportJobCancelResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling autoimport job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoimportJobCancelResult(string JobName, string Status);
}
