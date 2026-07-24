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
    Id = "fb1322cd-05b0-4264-9e96-6a9b3d9291a0",
    Name = "create",
    Title = "Create Key Vault Secret",
    Description = "Create/set a secret in an Azure Key Vault with the specified name and value. Required: --vault <vault>, --secret <secret>, --subscription <subscription>. Optional: --tenant <tenant>. Creates a new secret version if it already exists.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class SecretCreateCommand(ILogger<SecretCreateCommand> logger, IKeyVaultService keyVaultService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SecretCreateOptions, SecretDetails>(subscriptionResolver)
{
    private readonly ILogger<SecretCreateCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SecretCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var secret = await _keyVaultService.CreateSecret(
                options.Vault!,
                options.Secret!,
                options.Value!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(SecretDetails.FromSecret(secret), KeyVaultJsonContext.Default.SecretDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret {SecretName} in vault {VaultName}", options.Secret, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
