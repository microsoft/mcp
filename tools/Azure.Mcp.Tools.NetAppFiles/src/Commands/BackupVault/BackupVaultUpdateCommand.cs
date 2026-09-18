// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;

[CommandMetadata(
    Id = "00498a85-2ad0-45f9-b5ba-84b35ee9436e",
    Name = "update",
    Title = "Update Azure NetApp Files Backup Vault",
    Description = "Updates the tags of an Azure NetApp Files backup vault. Requires the account, backup vault, tags, resource group, and subscription. Returns the updated backup vault details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupVaultUpdateCommand(
    ILogger<BackupVaultUpdateCommand> logger,
    INetAppFilesBackupVaultService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupVaultUpdateOptions, BackupVaultUpdateCommand.BackupVaultUpdateResult>(subscriptionResolver)
{
    private readonly ILogger<BackupVaultUpdateCommand> _logger = logger;
    private readonly INetAppFilesBackupVaultService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupVaultUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = JsonSerializer.Deserialize(
                options.Tags,
                NetAppFilesJsonContext.Default.DictionaryStringString)!;

            var backupVault = await _service.UpdateBackupVaultAsync(
                options.Account,
                options.BackupVault,
                tags,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupVaultUpdateResult(backupVault),
                NetAppFilesJsonContext.Default.BackupVaultUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files backup vault. Account: {Account}, BackupVault: {BackupVault}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupVault,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupVaultUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!BackupVaultNameValidator.IsValid(options.BackupVault))
        {
            validationResult.Errors.Add(BackupVaultNameValidator.ErrorMessage);
        }

        try
        {
            if (JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString) is null)
            {
                validationResult.Errors.Add("--tags must be a JSON key-value object.");
            }
        }
        catch (JsonException)
        {
            validationResult.Errors.Add("--tags must be a JSON key-value object with string values.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files backup vault could not be updated because of a resource conflict. Verify the backup vault state and tag values.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files backup vault. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The Azure NetApp Files account or backup vault was not found. Verify the account, backup vault, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupVaultUpdateResult(NetAppFilesBackupVault BackupVault);
}
