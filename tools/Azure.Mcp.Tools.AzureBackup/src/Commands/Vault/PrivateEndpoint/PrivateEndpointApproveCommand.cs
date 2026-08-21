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
    Id = "3e7d8f61-cb45-4a29-9182-4d6b5e0a83c2",
    Name = "approve",
    Title = "Approve Private Endpoint Connection on RSV",
    Description = """
        Approves a pending Private Endpoint Connection (PEC) on a Recovery Services vault (RSV). If the
        PEC is already Approved, returns the current state unchanged. Requires
        Microsoft.RecoveryServices/vaults/privateEndpointConnectionsApproval/action on the vault.
        Backup vaults (DPP) are not supported.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointApproveCommand(
    ILogger<PrivateEndpointApproveCommand> logger,
    IAzureBackupService azureBackupService,
    ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PrivateEndpointApproveOptions, PrivateEndpointApproveCommand.PrivateEndpointApproveCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PrivateEndpointApproveCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointApproveOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.SetTag(AzureBackupTelemetryTags.PrivateEndpointAction, "approve");

        try
        {
            var result = await _azureBackupService.ApprovePrivateEndpointAsync(
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
                AzureBackupJsonContext.Default.PrivateEndpointApproveCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving Private Endpoint Connection. Vault: {Vault}, Name: {PrivateEndpointName}",
                options.Vault, options.PrivateEndpointName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        NotSupportedException nse => nse.Message,
        UnauthorizedAccessException => "Authorization failed. To approve you need Microsoft.RecoveryServices/vaults/privateEndpointConnectionsApproval/action on the vault.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault or Private Endpoint Connection not found. Verify --vault and --private-endpoint-name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed approving Private Endpoint Connection. Details: {reqEx.Message}",
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

    public sealed record PrivateEndpointApproveCommandResult(PrivateEndpointConnectionInfo Connection);
}
