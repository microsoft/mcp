// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Key;

public sealed class KeyCreateCommand(ILogger<KeyCreateCommand> logger) : SubscriptionCommand<KeyCreateOptions>
{
    private const string CommandTitle = "Create Key Vault Key";
    private readonly ILogger<KeyCreateCommand> _logger = logger;

    public override string Name => "create";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        "Create a new key in an Azure Key Vault. This command creates a key with the specified name and type in the given vault. Supports types: RSA, RSA-HSM, EC, EC-HSM, oct, oct-HSM. Required: --vault <vault>, --key <key> --key-type <key-type> --subscription <subscription>. Optional: --tenant <tenant>. Returns: Returns: name, id, keyId, keyType, enabled, notBefore, expiresOn, createdOn, updatedOn. Creates a new key version if it already exists.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.KeyName);
        command.Options.Add(KeyVaultOptionDefinitions.KeyType);
    }

    protected override KeyCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.KeyName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.KeyName.Name);
        options.KeyType = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.KeyType.Name);
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
                new(
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
