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
    Id = "5c8a2b47-e91d-4f68-b34a-2f7c85b90d3a",
    Name = "delete",
    Title = "Delete Private Endpoint Connection on RSV",
    Description = """
        Deletes a Private Endpoint Connection (PEC) from a Recovery Services vault (RSV). This removes
        the vault-side connection object. The underlying Microsoft.Network/privateEndpoints resource must
        be deleted separately if it is no longer needed. Backup vaults (DPP) are not supported.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointDeleteCommand(
    ILogger<PrivateEndpointDeleteCommand> logger,
    IAzureBackupService azureBackupService,
    ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PrivateEndpointDeleteOptions, PrivateEndpointDeleteCommand.PrivateEndpointDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PrivateEndpointDeleteCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointDeleteOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.SetTag(AzureBackupTelemetryTags.PrivateEndpointAction, "delete");

        try
        {
            var result = await _azureBackupService.DeletePrivateEndpointAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.PrivateEndpointName,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.PrivateEndpointDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Private Endpoint Connection. Vault: {Vault}, Name: {PrivateEndpointName}",
                options.Vault, options.PrivateEndpointName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        NotSupportedException nse => nse.Message,
        UnauthorizedAccessException => "Authorization failed. Verify your RBAC permissions on the vault, or specify --vault-type to skip auto-detection.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault or Private Endpoint Connection not found. It may have already been deleted; verify --vault and --private-endpoint-name.",
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

    public sealed record PrivateEndpointDeleteCommandResult(OperationResult Result);
}
