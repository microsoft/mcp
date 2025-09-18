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

public sealed class SecretGetCommand(ILogger<SecretGetCommand> logger) : SubscriptionCommand<SecretGetOptions>
{
    private const string _commandTitle = "Get Key Vault Secret";
    private readonly ILogger<SecretGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = true
    };

    public override string Description =>
        """
        Gets a secret from an Azure Key Vault. This command retrieves and displays the value
        of a specific secret from the specified vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.SecretName);
    }

    protected override SecretGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.SecretName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.SecretName.Name);
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
            var secret = await keyVaultService.GetSecret(
                options.VaultName!,
                options.SecretName!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new(
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
