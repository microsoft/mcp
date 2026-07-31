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
    Id = "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
    Name = "create",
    Title = "Create Azure Managed Lustre Autoimport Job",
    Description = """
        Creates an auto import job for an Azure Managed Lustre filesystem to continuously import new or modified files from the linked blob storage container. The auto import job syncs changes from the configured HSM blob container to the Lustre filesystem. Use this to keep the filesystem updated with changes in blob storage.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        Optional parameters:
        - job-name: Custom name for the job (default: autoimport-{timestamp})
        - conflict-resolution-mode: How to handle conflicts (Fail/Skip/OverwriteIfDirty/OverwriteAlways, default: Skip)
        - autoimport-prefixes: Array of blob paths/prefixes to auto import (default: '/', max: 100)
        - admin-status: Administrative status (Enable/Disable, default: Enable)
        - enable-deletions: Enable deletions during auto import (default: false)
        - maximum-errors: Max errors before failure (-1: infinite, 0: immediate exit, default: none)
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoimportJobCreateCommand(IManagedLustreService service, ILogger<AutoimportJobCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoimportJobCreateOptions, AutoimportJobCreateCommand.AutoimportJobCreateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoimportJobCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoimportJobCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Log the prefixes for debugging
            if (options.AutoimportPrefixes != null && options.AutoimportPrefixes.Length > 0)
            {
                _logger.LogInformation("Autoimport prefixes received: {Prefixes}", string.Join(", ", options.AutoimportPrefixes));
            }
            else
            {
                _logger.LogInformation("No autoimport prefixes received, will use default");
            }

            var job = await _service.CreateAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.ConflictResolutionMode,
                options.AutoimportPrefixes,
                options.AdminStatus,
                options.EnableDeletions,
                options.MaximumErrors,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.AutoimportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating autoimport job for AMLFS filesystem {FileSystem}.",
                options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoimportJobCreateResult(string JobName);
}
