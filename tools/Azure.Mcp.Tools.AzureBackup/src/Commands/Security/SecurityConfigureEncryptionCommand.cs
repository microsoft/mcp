// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Security;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class SecurityConfigureEncryptionCommand(ILogger<SecurityConfigureEncryptionCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<SecurityConfigureEncryptionOptions>()
{
    private readonly ILogger<SecurityConfigureEncryptionCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;
    private string? _lastVaultType;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.KeyVaultUri);
        command.Options.Add(AzureBackupOptionDefinitions.KeyNameOption);
        command.Options.Add(AzureBackupOptionDefinitions.KeyVersion);
        command.Options.Add(AzureBackupOptionDefinitions.IdentityType.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.UserAssignedIdentityId);

        command.Validators.Add(commandResult =>
        {
            var identityType = commandResult.GetValue<string>(AzureBackupOptionDefinitions.IdentityType.Name);
            if (!string.IsNullOrEmpty(identityType) &&
                !identityType.Equals("SystemAssigned", StringComparison.OrdinalIgnoreCase) &&
                !identityType.Equals("UserAssigned", StringComparison.OrdinalIgnoreCase))
            {
                commandResult.AddError("--identity-type must be 'SystemAssigned' or 'UserAssigned' for CMK encryption.");
            }

            if (string.Equals(identityType, "UserAssigned", StringComparison.OrdinalIgnoreCase))
            {
                var uaId = commandResult.GetValue<string>(AzureBackupOptionDefinitions.UserAssignedIdentityId.Name);
                if (string.IsNullOrEmpty(uaId))
                {
                    commandResult.AddError("--user-assigned-identity-id is required when --identity-type is 'UserAssigned'.");
                }
                else if (!uaId.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--user-assigned-identity-id must be a valid ARM resource ID starting with '/subscriptions/'.");
                }
            }

            var keyVaultUri = commandResult.GetValue<string>(AzureBackupOptionDefinitions.KeyVaultUri.Name);
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                if (!Uri.TryCreate(keyVaultUri, UriKind.Absolute, out var uri))
                {
                    commandResult.AddError("--key-vault-uri must be a valid URI (e.g., 'https://kv-name.vault.azure.net/').");
                }
                else
                {
                    if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                    {
                        commandResult.AddError("--key-vault-uri must use HTTPS (e.g., 'https://kv-name.vault.azure.net/').");
                    }

                    if (uri.AbsolutePath != "/" && !string.IsNullOrEmpty(uri.AbsolutePath.TrimEnd('/')))
                    {
                        commandResult.AddError("--key-vault-uri must be the Key Vault base URI without path segments (e.g., 'https://kv-name.vault.azure.net/'). Do not include '/keys/...' in the URI.");
                    }
                }
            }
        });
    }

    protected override SecurityConfigureEncryptionOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.KeyVaultUri = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.KeyVaultUri.Name);
        options.KeyName = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.KeyNameOption.Name);
        options.KeyVersion = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.KeyVersion.Name);
        options.IdentityType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.IdentityType.Name);
        options.UserAssignedIdentityId = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.UserAssignedIdentityId.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        _lastVaultType = options.VaultType;

        try
        {
            var result = await _azureBackupService.ConfigureEncryptionAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.KeyVaultUri!,
                options.KeyName!,
                options.IdentityType!,
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

    internal record SecurityConfigureEncryptionCommandResult(OperationResult Result);
}
