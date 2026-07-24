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
    Id = "0h4i2j3k-5e1c-6h9g-d4f7-9c0e5g7h8d9i",
    Name = "delete",
    Title = "Delete Azure Managed Lustre Autoimport Job",
    Description = """
        Deletes an auto import job for an Azure Managed Lustre filesystem. This permanently removes the job record from the filesystem. Use this to clean up completed, failed, or cancelled autoimport jobs.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoimport job to delete
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoimportJobDeleteCommand(IManagedLustreService service, ILogger<AutoimportJobDeleteCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoimportJobDeleteOptions, AutoimportJobDeleteCommand.AutoimportJobDeleteResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoimportJobDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoimportJobDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!, "Deleted"), ManagedLustreJsonContext.Default.AutoimportJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting autoimport job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoimportJobDeleteResult(string JobName, string Status);
}
