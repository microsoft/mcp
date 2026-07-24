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
    Id = "b1f3c5e7-9d2a-4b8f-6c3e-1a7b9d2f5e8c",
    Name = "create",
    Title = "Create Azure Managed Lustre Import Job",
    Description = """
        Creates a one-time import job for an Azure Managed Lustre filesystem to import files from the linked blob storage container. The import job performs a one-time sync of data from the configured HSM blob container to the Lustre filesystem. Use this to import specific prefixes or all data from blob storage into the filesystem at a point in time.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        Optional options:
        - job-name: Name for the import job (auto-generated if not provided)
        - conflict-resolution-mode: How to handle conflicting files (Fail, Skip, OverwriteIfDirty, OverwriteAlways, default: Fail)
        - import-prefixes: Blob prefixes to import (default: imports all data from root '/')
        - maximum-errors: Maximum errors allowed before job failure (-1: infinite, 0: fail on first error, default: use service default)
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ImportJobCreateCommand(IManagedLustreService service, ILogger<ImportJobCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ImportJobCreateOptions, ImportJobCreateCommand.ImportJobCreateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ImportJobCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ImportJobCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {

            // Log the prefixes for debugging
            if (options.ImportPrefixes != null && options.ImportPrefixes.Length > 0)
            {
                _logger.LogInformation("Import prefixes received: {Prefixes}", string.Join(", ", options.ImportPrefixes));
            }
            else
            {
                _logger.LogInformation("No import prefixes received, will import all data");
            }

            var job = await _service.CreateImportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.ConflictResolutionMode,
                options.ImportPrefixes,
                options.MaximumErrors,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.ImportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating import job for AMLFS filesystem {FileSystem}.",
                options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ImportJobCreateResult(string JobName);
}
