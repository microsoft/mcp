// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class SecretGetCommand(ILogger<SecretGetCommand> logger, IKeyVaultService keyVaultService) : SubscriptionCommand<SecretGetOptions>
{
    private readonly ILogger<SecretGetCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.SecretName.AsOptional());
    }

    protected override SecretGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.SecretName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.SecretName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            if (string.IsNullOrEmpty(options.SecretName))
            {
                // List all secrets
                var secrets = await _keyVaultService.ListSecrets(
                    options.VaultName!,
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
                    options.VaultName!,
                    options.SecretName,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var secretDetails = new SecretDetails(
                    secret.Name,
                    secret.Value,
                    secret.Properties.Enabled,
                    secret.Properties.NotBefore,
                    secret.Properties.ExpiresOn,
                    secret.Properties.CreatedOn,
                    secret.Properties.UpdatedOn);

                context.Response.Results = ResponseResult.Create(new(Secrets: null, Secret: secretDetails), KeyVaultJsonContext.Default.SecretGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.SecretName))
            {
                _logger.LogError(ex, "Error listing secrets from vault {VaultName}", options.VaultName);
            }
            else
            {
                _logger.LogError(ex, "Error getting secret {SecretName} from vault {VaultName}", options.SecretName, options.VaultName);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SecretDetails(string Name, string Value, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn);
    internal record SecretGetCommandResult(List<string>? Secrets, SecretDetails? Secret);
}
