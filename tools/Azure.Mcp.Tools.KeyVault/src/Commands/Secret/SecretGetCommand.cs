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

public sealed class SecretGetCommand(ILogger<SecretGetCommand> logger) : SubscriptionCommand<SecretGetOptions>
{
    private const string _commandTitle = "Get Key Vault Secret";
    private readonly ILogger<SecretGetCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;
    private readonly Option<string> _secretOption = KeyVaultOptionDefinitions.SecretName;

    public override string Name => "get";

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        """
        Gets a secret from an Azure Key Vault. This command retrieves and displays the value
        of a specific secret from the specified vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_vaultOption);
        command.AddOption(_secretOption);
    }

    protected override SecretGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueForOption(_vaultOption);
        options.SecretName = parseResult.GetValueForOption(_secretOption);
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
            var secret = await keyVaultService.GetSecret(
                options.VaultName!,
                options.SecretName!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new SecretGetCommandResult(
                    secret.Name,
                    secret.Value,
                    secret.Properties.Enabled,
                    secret.Properties.NotBefore,
                    secret.Properties.ExpiresOn,
                    secret.Properties.CreatedOn,
                    secret.Properties.UpdatedOn),
                KeyVaultJsonContext.Default.SecretGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secret {SecretName} from vault {VaultName}", options.SecretName, options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SecretGetCommandResult(string Name, string Value, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn);
}
