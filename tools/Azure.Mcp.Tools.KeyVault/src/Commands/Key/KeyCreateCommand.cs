// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Key;

public sealed class KeyCreateCommand(ILogger<KeyCreateCommand> logger) : SubscriptionCommand<KeyCreateOptions>
{
    private const string CommandTitle = "Create Key Vault Key";
    private readonly ILogger<KeyCreateCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;
    private readonly Option<string> _keyOption = KeyVaultOptionDefinitions.KeyName;
    private readonly Option<string> _keyTypeOption = KeyVaultOptionDefinitions.KeyType;

    public override string Name => "create";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        """
        Create a new key in an Azure Key Vault. This command creates a key with the specified name and type
        in the given vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_vaultOption);
        command.Options.Add(_keyOption);
        command.Options.Add(_keyTypeOption);
    }

    protected override KeyCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValue(_vaultOption);
        options.KeyName = parseResult.GetValue(_keyOption);
        options.KeyType = parseResult.GetValue(_keyTypeOption);
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
            var key = await keyVaultService.CreateKey(
                options.VaultName!,
                options.KeyName!,
                options.KeyType!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new KeyCreateCommandResult(
                    key.Name,
                    key.KeyType.ToString(),
                    key.Properties.Enabled,
                    key.Properties.NotBefore,
                    key.Properties.ExpiresOn,
                    key.Properties.CreatedOn,
                    key.Properties.UpdatedOn),
                KeyVaultJsonContext.Default.KeyCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating key {KeyName} in vault {VaultName}", options.KeyName, options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyCreateCommandResult(string Name, string KeyType, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn);
}
