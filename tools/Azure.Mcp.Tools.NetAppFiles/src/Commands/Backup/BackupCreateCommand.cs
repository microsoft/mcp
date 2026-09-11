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
    Id = "a3d7e1f9-5b2c-4a8d-9e6f-c0d4b8a2f7e3",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files backup in a specified backup vault under a NetApp account, and returns the created backup details including name, location, resource group, provisioning state, volume resource ID, and backup type. Requires account name, backup vault name, backup name, resource group, location, volume resource ID, and subscription.
        """,
    Title = "Create NetApp Files Backup",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class BackupCreateCommand(ILogger<BackupCreateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupCreateOptions, BackupCreateCommand.BackupCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<BackupCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BackupCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var backup = await _netAppFilesService.CreateBackup(
                options.Account!,
                options.BackupVault!,
                options.Backup!,
                options.ResourceGroup!,
                options.Location!,
                options.VolumeResourceId!,
                options.Subscription!,
                options.Label,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(backup),
                NetAppFilesJsonContext.Default.BackupCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files backup. Account: {Account}, BackupVault: {BackupVault}, Backup: {Backup}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
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
            $"Authorization failed creating the backup. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account, backup vault, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record BackupCreateCommandResult([property: JsonPropertyName("backup")] BackupCreateResult Backup);
}
