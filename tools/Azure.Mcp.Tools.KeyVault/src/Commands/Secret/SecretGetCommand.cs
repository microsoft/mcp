// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.KeyVault.Models;
using Azure.Mcp.Tools.KeyVault.Options.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.KeyVault.Commands.Secret;

[CommandMetadata(
    Id = "933bcb29-87e6-4f78-94ad-8ad0c8c60002",
    Name = "get",
    Title = "Get Key Vault Secret",
    Description = """List all secrets in your Key Vault or get a specific secret by name. Shows all secret names in the vault (without values), or retrieves the secret value and full details including enabled status and expiration dates.""",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true,
    LocalRequired = false)]
public sealed class SecretGetCommand(ILogger<SecretGetCommand> logger, IKeyVaultService keyVaultService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SecretGetOptions, SecretGetCommand.SecretGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SecretGetCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SecretGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(options.Secret))
            {
                // List all secrets
                var secrets = await _keyVaultService.ListSecrets(
                    options.Vault,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(Secrets: secrets ?? [], Secret: null), KeyVaultJsonContext.Default.SecretGetCommandResult);
            }
            else
            {
                // Get specific secret
                var secret = await _keyVaultService.GetSecret(
                    options.Vault,
                    options.Secret,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, SecretDetails.FromSecret(secret)), KeyVaultJsonContext.Default.SecretGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.Secret))
            {
                _logger.LogError(ex, "Error listing secrets from vault {VaultName}", options.Vault);
            }
            else
            {
                _logger.LogError(ex, "Error getting secret {Secret} from vault {VaultName}", options.Secret, options.Vault);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SecretGetCommandResult(List<string>? Secrets, SecretDetails? Secret);
}
