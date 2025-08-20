// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Secret;

public sealed class SecretCreateCommand(ILogger<SecretCreateCommand> logger) : SubscriptionCommand<SecretCreateOptions>
{
    private const string CommandTitle = "Create Key Vault Secret";
    private readonly ILogger<SecretCreateCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;
    private readonly Option<string> _secretOption = KeyVaultOptionDefinitions.SecretName;
    private readonly Option<string> _valueOption = KeyVaultOptionDefinitions.SecretValue;

    public override string Name => "create";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    public override string Description =>
        """
        Creates a new secret in an Azure Key Vault. This command creates a secret with the specified name and value
        in the given vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_vaultOption);
        command.AddOption(_secretOption);
        command.AddOption(_valueOption);
    }

    protected override SecretCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueForOption(_vaultOption);
        options.SecretName = parseResult.GetValueForOption(_secretOption);
        options.SecretValue = parseResult.GetValueForOption(_valueOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

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
