// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Backup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Backup;

[CommandMetadata(
    Id = "e7b3a1d5-9c4f-4e8a-b2d6-f1a5c3e7d9b4",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files backup in a specified backup vault under a NetApp account, and returns the updated backup details including name, location, resource group, provisioning state, volume resource ID, label, and backup type. Supports updating the backup label. Requires account name, backup vault name, backup name, resource group, location, and subscription. Optionally accepts a label.
        """,
    Title = "Update NetApp Files Backup",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class BackupUpdateCommand(ILogger<BackupUpdateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupUpdateOptions, BackupUpdateCommand.BackupUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<BackupUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BackupUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var backup = await _netAppFilesService.UpdateBackup(
                options.Account!,
                options.BackupVault!,
                options.Backup!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Label,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupUpdateCommandResult(backup),
                NetAppFilesJsonContext.Default.BackupUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files backup. Account: {Account}, BackupVault: {BackupVault}, Backup: {Backup}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Account, options.BackupVault, options.Backup, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A backup with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the backup. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Backup, account, backup vault, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record BackupUpdateCommandResult([property: JsonPropertyName("backup")] BackupCreateResult Backup);
}
