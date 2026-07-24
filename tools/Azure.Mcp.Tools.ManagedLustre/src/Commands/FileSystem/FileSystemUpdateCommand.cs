// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

[CommandMetadata(
    Id = "db1bdf99-ac8a-4920-ab2e-15048623b2dc",
    Name = "update",
    Title = "Update Azure Managed Lustre FileSystem",
    Description = "Update maintenance window and/or root squash settings of an existing Azure Managed Lustre (AMLFS) file system. Provide either maintenance day and time or root squash fields (no-squash-nid-list, squash-uid, squash-gid). Root squash fields must be provided if root squash is not None. In case of maintenance window update, both maintenance day and maintenance time should be provided.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FileSystemUpdateCommand(IManagedLustreService service, ILogger<FileSystemUpdateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileSystemUpdateOptions, FileSystemUpdateCommand.FileSystemUpdateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<FileSystemUpdateCommand> _logger = logger;

    public override void ValidateOptions(FileSystemUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.MaintenanceDay) &&
            string.IsNullOrWhiteSpace(options.MaintenanceTime) &&
            string.IsNullOrWhiteSpace(options.RootSquashMode))
        {
            validationResult.Errors.Add("At least one of maintenance-day/time or root-squash fields must be provided.");
        }

        ManagedLustreCommonValidators.ValidateRootSquashOptions(validationResult, options.RootSquashMode, options.NoSquashNidList, options.SquashUid, options.SquashGid);

        var updateWithMaintenance = !string.IsNullOrWhiteSpace(options.MaintenanceDay) || !string.IsNullOrWhiteSpace(options.MaintenanceTime);
        if ((string.IsNullOrWhiteSpace(options.MaintenanceDay) || string.IsNullOrWhiteSpace(options.MaintenanceTime)) && updateWithMaintenance)
        {
            validationResult.Errors.Add("When updating maintenance window, both --maintenance-day and --maintenance-time must be specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileSystemUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var fs = await _service.UpdateFileSystemAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.MaintenanceDay,
                options.MaintenanceTime,
                options.RootSquashMode,
                options.NoSquashNidList,
                options.SquashUid,
                options.SquashGid,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(fs), ManagedLustreJsonContext.Default.FileSystemUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AMLFS.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileSystemUpdateResult(Models.LustreFileSystem FileSystem);
}
