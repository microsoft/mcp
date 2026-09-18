// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Core;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Backup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Backup;

[CommandMetadata(
    Id = "22cb5b1e-59f7-43a0-8fe8-343cc83cb4a7",
    Name = "create",
    Title = "Create Azure NetApp Files Backup",
    Description = "Creates an on-demand backup of an Azure NetApp Files volume in a backup vault. Requires the account, backup vault, backup name, volume resource ID, resource group, and subscription. Returns the created backup details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupCreateCommand(
    ILogger<BackupCreateCommand> logger,
    INetAppFilesBackupService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupCreateOptions, BackupCreateCommand.BackupCreateResult>(subscriptionResolver)
{
    private readonly ILogger<BackupCreateCommand> _logger = logger;
    private readonly INetAppFilesBackupService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backup = await _service.CreateBackupAsync(
                options.Account,
                options.BackupVault,
                options.Backup,
                new ResourceIdentifier(options.VolumeResourceId),
                options.Label,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupCreateResult(backup),
                NetAppFilesJsonContext.Default.BackupCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files backup. Account: {Account}, BackupVault: {BackupVault}, Backup: {Backup}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupVault,
                options.Backup,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!BackupNameValidator.IsValidBackupVault(options.BackupVault))
        {
            validationResult.Errors.Add(BackupNameValidator.BackupVaultErrorMessage);
        }

        if (!BackupNameValidator.IsValidBackup(options.Backup))
        {
            validationResult.Errors.Add(BackupNameValidator.BackupErrorMessage);
        }

        if (!ResourceIdentifier.TryParse(options.VolumeResourceId, out var volumeResourceId) || volumeResourceId is null ||
            !string.Equals(volumeResourceId.ResourceType.ToString(), "Microsoft.NetApp/netAppAccounts/capacityPools/volumes", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--volume-resource-id must be a valid Azure NetApp Files volume resource ID.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files backup could not be created because of a resource conflict. Verify the volume and backup vault configuration.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files backup. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, backup vault, or volume was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupCreateResult(NetAppFilesBackup Backup);
}
