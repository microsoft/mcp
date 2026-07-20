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
    Id = "e4i6f8h0-2g5b-7e9f-1h3d-5g7b9e1g3f5h",
    Name = "delete",
    Title = "Delete Azure Managed Lustre Import Job",
    Description = """
        Deletes an import job for an Azure Managed Lustre filesystem. This removes the job record and history. The job must be completed or cancelled before it can be deleted.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: Name of the import job to delete
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ImportJobDeleteCommand(IManagedLustreService service, ILogger<ImportJobDeleteCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ImportJobDeleteOptions, ImportJobDeleteCommand.ImportJobDeleteResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ImportJobDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ImportJobDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {

            await _service.DeleteImportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName), ManagedLustreJsonContext.Default.ImportJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting import job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ImportJobDeleteResult(string JobName);
}
