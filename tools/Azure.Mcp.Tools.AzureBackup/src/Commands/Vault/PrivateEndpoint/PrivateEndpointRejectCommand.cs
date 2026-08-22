// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Vault.PrivateEndpoint;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault.PrivateEndpoint;

[CommandMetadata(
    Id = "b64f2a19-de53-4b97-a5c1-0a3d97e64b58",
    Name = "reject",
    Title = "Reject Private Endpoint Connection on RSV",
    Description = """
        Rejects a pending Private Endpoint Connection (PEC) on a Recovery Services vault (RSV). If the
        PEC is already Rejected, returns the current state unchanged. Requires
        Microsoft.RecoveryServices/vaults/privateEndpointConnectionsApproval/action on the vault.
        Backup vaults (DPP) are not supported.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointRejectCommand(
    ILogger<PrivateEndpointRejectCommand> logger,
    IAzureBackupService azureBackupService,
    ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PrivateEndpointRejectOptions, PrivateEndpointRejectCommand.PrivateEndpointRejectCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PrivateEndpointRejectCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointRejectOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.SetTag(AzureBackupTelemetryTags.PrivateEndpointAction, "reject");

        try
        {
            var result = await _azureBackupService.RejectPrivateEndpointAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.PrivateEndpointName,
                options.Description,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.PrivateEndpointRejectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting Private Endpoint Connection. Vault: {Vault}, Name: {PrivateEndpointName}",
                options.Vault, options.PrivateEndpointName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        NotSupportedException nse => nse.Message,
        UnauthorizedAccessException => "Authorization failed. To reject you need Microsoft.RecoveryServices/vaults/privateEndpointConnectionsApproval/action on the vault.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault or Private Endpoint Connection not found. Verify --vault and --private-endpoint-name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed rejecting Private Endpoint Connection. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        NotSupportedException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Forbidden,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record PrivateEndpointRejectCommandResult(PrivateEndpointConnectionInfo Connection);
}
