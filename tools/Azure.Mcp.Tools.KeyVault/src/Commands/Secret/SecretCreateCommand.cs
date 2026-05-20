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
public sealed class SecretCreateCommand(ILogger<SecretCreateCommand> logger, IKeyVaultService keyVaultService) : SubscriptionCommand<SecretCreateOptions>
{
    private readonly ILogger<SecretCreateCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.SecretName);
        command.Options.Add(KeyVaultOptionDefinitions.SecretValue);
    }

    protected override SecretCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.SecretName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.SecretName.Name);
        options.SecretValue = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.SecretValue.Name);
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
            var secret = await _keyVaultService.CreateSecret(
                options.VaultName!,
                options.SecretName!,
                options.SecretValue!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(
                    secret.Name,
                    secret.Value,
                    secret.Properties.Enabled,
                    secret.Properties.NotBefore,
                    secret.Properties.ExpiresOn,
                    secret.Properties.CreatedOn,
                    secret.Properties.UpdatedOn),
                KeyVaultJsonContext.Default.SecretCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret {SecretName} in vault {VaultName}", options.SecretName, options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SecretCreateCommandResult(string Name, string Value, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn);
}
