// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;

/// <summary>
/// Consolidated recovery point command: when --recovery-point is supplied returns a single
/// recovery point's details; otherwise lists all recovery points for the protected item.
/// </summary>
[CommandMetadata(
    Id = "e930bbb6-b495-454b-bae4-46b9da14eb1c",
    Name = "get",
    Title = "Get Recovery Point",
    Description = """
        Retrieves recovery point information for a protected item. When --recovery-point is
        specified, returns detailed information about a single recovery point including time
        and type. When omitted, lists all available recovery points for the protected item.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPointGetCommand(ILogger<RecoveryPointGetCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<RecoveryPointGetOptions, RecoveryPointGetCommand.RecoveryPointGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RecoveryPointGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPointGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, string.IsNullOrEmpty(options.RecoveryPoint) ? "list" : "single");

        try
        {
            if (!string.IsNullOrEmpty(options.RecoveryPoint))
            {
                var rp = await _azureBackupService.GetRecoveryPointAsync(
                    options.Vault,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.ProtectedItem,
                    options.RecoveryPoint,
                    options.VaultType,
                    options.Container,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([rp]),
                    AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);
            }
            else
            {
                var points = await _azureBackupService.ListRecoveryPointsAsync(
                    options.Vault,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.ProtectedItem,
                    options.VaultType,
                    options.Container,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(points),
                    AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recovery point(s). RecoveryPoint: {RecoveryPoint}, Vault: {Vault}",
                options.RecoveryPoint, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery point not found. Verify the recovery point ID and protected item.",
        RequestFailedException reqEx when reqEx.ErrorCode == "BMSUserErrorContainerNameIncorrectFormat" =>
            $"Container name format is incorrect. Use the fully qualified container name from 'azurebackup protecteditem get' (e.g., 'IaasVMContainer;iaasvmcontainerv2;resourceGroup;vmName'). Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Ensure the caller has the Backup Reader or Backup Operator role on the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecoveryPointGetCommandResult(List<RecoveryPointInfo> RecoveryPoints);
}
