// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Backup;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Backup;

[CommandMetadata(
    Id = "f5612c55-054d-4fd8-964c-952e8e6b87f8",
    Name = "status",
    Title = "Check Backup Status",
    Description = """
        Checks the backup status of an Azure resource and returns whether it is protected,
        along with vault and policy details. Use this to verify if a VM, disk, storage account,
        or other datasource is currently backed up. Requires the datasource ARM resource ID
        and the Azure region (location) where the resource exists.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupStatusCommand(ILogger<BackupStatusCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupStatusOptions, BackupStatusCommand.BackupStatusCommandResult>(subscriptionResolver)
{
    private readonly ILogger<BackupStatusCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BackupStatusOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, "status-check");

        try
        {
            var result = await _azureBackupService.GetBackupStatusAsync(
                options.DatasourceId,
                options.Subscription!,
                options.Location,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.BackupStatusCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking backup status for datasource: {DatasourceId}", options.DatasourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the datasource ARM resource ID.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed checking backup status. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record BackupStatusCommandResult(BackupStatusResult Status);
}
