// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.RegularExpressions;
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
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed partial class BackupStatusCommand(ILogger<BackupStatusCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
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
            "Authorization failed checking backup status. " +
            "This tool requires the 'Microsoft.RecoveryServices/locations/backupStatus/action' permission " +
            "(included in 'Backup Reader' role at subscription scope). " +
            "Alternatively, use 'vault_get' + 'protecteditem_get' to check protection status.",
        RequestFailedException { ErrorCode: ProviderNotRegisteredErrorCode } reqEx =>
            $"The subscription is not registered to use the '{GetUnregisteredNamespace(reqEx)}' resource provider, so backup status cannot be checked. " +
            $"Register it (for example, 'az provider register --namespace {GetUnregisteredNamespace(reqEx)}'; requires permission to register resource providers) and retry.",
        _ => base.GetErrorMessage(ex)
    };

    private const string ProviderNotRegisteredErrorCode = "ProviderNotRegistered";
    private const string DefaultBackupNamespace = "Microsoft.RecoveryServices";

    // ARM reports "...not registered to use namespace 'Microsoft.X'". Only accept a well-formed namespace name.
    private static string GetUnregisteredNamespace(RequestFailedException ex)
    {
        var match = UnregisteredNamespaceRegex().Match(ex.Message ?? string.Empty);
        return match.Success ? match.Groups[1].Value : DefaultBackupNamespace;
    }

    [GeneratedRegex(@"namespace '(Microsoft\.[A-Za-z]+)'", RegexOptions.IgnoreCase)]
    private static partial Regex UnregisteredNamespaceRegex();

    public sealed record BackupStatusCommandResult(BackupStatusResult Status);
}
