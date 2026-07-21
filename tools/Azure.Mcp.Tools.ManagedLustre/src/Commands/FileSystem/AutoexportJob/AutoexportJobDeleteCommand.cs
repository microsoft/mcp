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
    Id = "4c7a8e3d-9f2b-5a6e-c1d4-8b3e9a2f7c5d",
    Name = "delete",
    Title = "Delete Azure Managed Lustre Autoexport Job",
    Description = """
        Deletes an auto export job for an Azure Managed Lustre filesystem. This permanently removes the job record from the filesystem. Use this to clean up completed, failed, or cancelled autoexport jobs.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoexport job to delete
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoexportJobDeleteCommand(IManagedLustreService service, ILogger<AutoexportJobDeleteCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoexportJobDeleteOptions, AutoexportJobDeleteCommand.AutoexportJobDeleteResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoexportJobDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoexportJobDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAutoexportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!, "Deleted"), ManagedLustreJsonContext.Default.AutoexportJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting autoexport job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoexportJobDeleteResult(string JobName, string Status);
}
