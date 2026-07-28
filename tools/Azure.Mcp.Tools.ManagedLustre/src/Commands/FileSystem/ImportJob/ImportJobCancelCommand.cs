// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;

[CommandMetadata(
    Id = "d3h5e7g9-1f4a-6d8e-0g2c-4f6a8d0f2e4g",
    Name = "cancel",
    Title = "Cancel Azure Managed Lustre Import Job",
    Description = """
        Cancels a running import job for an Azure Managed Lustre filesystem. This stops the import operation and prevents further processing. The job cannot be resumed after cancellation.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: Name of the import job to cancel
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ImportJobCancelCommand(IManagedLustreService service, ILogger<ImportJobCancelCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ImportJobCancelOptions, ImportJobCancelCommand.ImportJobCancelResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ImportJobCancelCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ImportJobCancelOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var cancelledJob = await _service.CancelImportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName, cancelledJob.Properties?.AdminStatus ?? "Unknown"), ManagedLustreJsonContext.Default.ImportJobCancelResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling import job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ImportJobCancelResult(string JobName, string Status);
}
