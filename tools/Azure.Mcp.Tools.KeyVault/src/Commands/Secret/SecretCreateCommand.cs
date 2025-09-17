// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Secret;

public sealed class SecretCreateCommand(ILogger<SecretCreateCommand> logger) : SubscriptionCommand<SecretCreateOptions>
{
    private const string CommandTitle = "Create Key Vault Secret";
    private readonly ILogger<SecretCreateCommand> _logger = logger;

    public override string Name => "create";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = true
    };

    public override string Description =>
        """
        Creates a new secret in an Azure Key Vault. This command creates a secret with the specified name and value
        in the given vault.
        """;

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var keyVaultService = context.GetService<IKeyVaultService>();
            var secret = await keyVaultService.CreateSecret(
                options.VaultName!,
                options.SecretName!,
                options.SecretValue!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new SecretCreateCommandResult(
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
