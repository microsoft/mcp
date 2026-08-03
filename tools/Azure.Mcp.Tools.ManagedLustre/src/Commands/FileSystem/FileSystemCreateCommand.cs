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
    Id = "814acadf-ee84-47f9-ad68-2d65ec7dbb07",
    Name = "create",
    Title = "Create Azure Managed Lustre FileSystem",
    Description = """
        Create an Azure Managed Lustre (AMLFS) file system using the specified network, capacity, maintenance window and availability zone.
        Optionally provides possibility to define Blob Integration, customer managed key encryption and root squash configuration.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FileSystemCreateCommand(IManagedLustreService service, ILogger<FileSystemCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileSystemCreateOptions, FileSystemCreateCommand.FileSystemCreateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<FileSystemCreateCommand> _logger = logger;

    public override void ValidateOptions(FileSystemCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ManagedLustreCommonValidators.ValidateRootSquashOptions(validationResult, options.RootSquashMode, options.NoSquashNidList, options.SquashUid, options.SquashGid);

        if (options.CustomEncryption == true &&
            (string.IsNullOrWhiteSpace(options.KeyUrl) || string.IsNullOrWhiteSpace(options.SourceVault) || string.IsNullOrWhiteSpace(options.UserAssignedIdentityId)))
        {
            validationResult.Errors.Add("Missing Required options: key-url, source-vault, user-assigned-identity when custom-encryption is set");
        }

        var hsmEnabled = !string.IsNullOrWhiteSpace(options.HsmContainer) || !string.IsNullOrWhiteSpace(options.HsmLogContainer);

        // Always require both values if one is specified.
        if (hsmEnabled && (string.IsNullOrWhiteSpace(options.HsmContainer) || string.IsNullOrWhiteSpace(options.HsmLogContainer)))
        {
            validationResult.Errors.Add("When enabling Azure Blob Integration both data container and log container must be specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileSystemCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var fs = await _service.CreateFileSystemAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.Location,
                options.Sku,
                options.Size,
                options.SubnetId,
                options.Zone,
                options.MaintenanceDay,
                options.MaintenanceTime,
                options.HsmContainer,
                options.HsmLogContainer,
                options.ImportPrefix,
                options.RootSquashMode,
                options.NoSquashNidList,
                options.SquashUid,
                options.SquashGid,
                options.CustomEncryption ?? false,
                options.KeyUrl,
                options.SourceVault,
                options.UserAssignedIdentityId,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(fs), ManagedLustreJsonContext.Default.FileSystemCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AMLFS.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileSystemCreateResult(Models.LustreFileSystem FileSystem);
}
