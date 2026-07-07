// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Security;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Security;

[CommandMetadata(
    Id = "d4b32779-ac6f-5e2b-8a4d-8f3b1b9e5c30",
    Name = "configure-encryption",
    Title = "Configure Customer-Managed Key Encryption",
    Description = """
        Configures Customer-Managed Key (CMK) encryption on a vault using a key from Azure Key Vault.
        Supports both Recovery Services vaults (RSV) and Backup vaults (DPP). The vault's managed
        identity must have the Key Vault Crypto Service Encryption User role on the Key Vault.
        Use --identity-type to specify SystemAssigned or UserAssigned identity, and
        --user-assigned-identity-id when using a user-assigned identity.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SecurityConfigureEncryptionCommand(ILogger<SecurityConfigureEncryptionCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<SecurityConfigureEncryptionOptions, SecurityConfigureEncryptionCommand.SecurityConfigureEncryptionCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SecurityConfigureEncryptionCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;
    private string? _lastVaultType;

    public override void ValidateOptions(SecurityConfigureEncryptionOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.IdentityType) &&
            !options.IdentityType.Equals("SystemAssigned", StringComparison.OrdinalIgnoreCase) &&
            !options.IdentityType.Equals("UserAssigned", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--identity-type must be 'SystemAssigned' or 'UserAssigned' for CMK encryption.");
        }

        if (string.Equals(options.IdentityType, "UserAssigned", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(options.UserAssignedIdentityId))
            {
                validationResult.Errors.Add("--user-assigned-identity-id is required when --identity-type is 'UserAssigned'.");
            }
            else if (!options.UserAssignedIdentityId.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
            {
                validationResult.Errors.Add("--user-assigned-identity-id must be a valid ARM resource ID starting with '/subscriptions/'.");
            }
        }

        if (!string.IsNullOrEmpty(options.KeyVaultUri))
        {
            if (!Uri.TryCreate(options.KeyVaultUri, UriKind.Absolute, out var uri))
            {
                validationResult.Errors.Add("--key-vault-uri must be a valid URI (e.g., 'https://kv-name.vault.azure.net/').");
            }
            else
            {
                if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    validationResult.Errors.Add("--key-vault-uri must use HTTPS (e.g., 'https://kv-name.vault.azure.net/').");
                }

                if (uri.AbsolutePath != "/" && !string.IsNullOrEmpty(uri.AbsolutePath.TrimEnd('/')))
                {
                    validationResult.Errors.Add("--key-vault-uri must be the Key Vault base URI without path segments (e.g., 'https://kv-name.vault.azure.net/'). Do not include '/keys/...' in the URI.");
                }
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SecurityConfigureEncryptionOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        _lastVaultType = options.VaultType;

        try
        {
            var result = await _azureBackupService.ConfigureEncryptionAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.KeyVaultUri,
                options.KeyName,
                options.IdentityType,
                options.KeyVersion,
                options.UserAssignedIdentityId,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.SecurityConfigureEncryptionCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring CMK encryption. Vault: {Vault}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        UnauthorizedAccessException => "Authorization failed. Verify your RBAC permissions on the vault and Key Vault, or specify --vault-type to skip auto-detection.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault or Key Vault key not found. Verify the vault name, resource group, Key Vault URI, and key name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            string.Equals(_lastVaultType, "rsv", StringComparison.OrdinalIgnoreCase)
                ? $"Bad request configuring CMK encryption. For RSV, CMK can only be enabled on new vaults with no registered items. Ensure the vault has a managed identity enabled and the Key Vault Crypto Service Encryption User role is assigned. Details: {reqEx.Message}"
                : $"Bad request configuring CMK encryption. Ensure the vault has a managed identity enabled and the Key Vault Crypto Service Encryption User role is assigned. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. The vault's managed identity needs Key Vault Crypto Service Encryption User role on the Key Vault. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Encryption configuration conflict. The vault may already have CMK configured or an operation is in progress.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        UnauthorizedAccessException => HttpStatusCode.Forbidden,
        ArgumentException or FormatException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record SecurityConfigureEncryptionCommandResult(OperationResult Result);
}
