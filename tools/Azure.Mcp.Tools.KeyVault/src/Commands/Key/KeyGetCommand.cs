// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.KeyVault.Commands.Key;

public sealed class KeyGetCommand(ILogger<KeyGetCommand> logger) : SubscriptionCommand<KeyGetOptions>
{
    private const string CommandTitle = "Get Key Vault Key";
    private readonly ILogger<KeyGetCommand> _logger = logger;

    public override string Id => "c19a45a0-b963-427d-a087-35560a7f4e5b";

    public override string Name => "get";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        """Retrieve detailed information about Azure Key Vault keys. If a specific key name is provided, returns full details for that key including type, enabled status, and expiration dates. If no key name is specified, returns a list of all key names in the vault. Use --include-managed to include managed keys in the list.""";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.KeyName.AsOptional());
        command.Options.Add(KeyVaultOptionDefinitions.IncludeManagedKeys);
    }

    protected override KeyGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.KeyName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.KeyName.Name);
        options.IncludeManagedKeys = parseResult.GetValueOrDefault<bool>(KeyVaultOptionDefinitions.IncludeManagedKeys.Name);
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
            var keyVaultService = context.GetService<IKeyVaultService>();

            if (string.IsNullOrEmpty(options.KeyName))
            {
                // List all keys
                var keys = await keyVaultService.ListKeys(
                    options.VaultName!,
                    options.IncludeManagedKeys,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new KeyGetCommandResult(Keys: keys ?? [], Key: null), KeyVaultJsonContext.Default.KeyGetCommandResult);
            }
            else
            {
                // Get specific key
                var key = await keyVaultService.GetKey(
                    options.VaultName!,
                    options.KeyName,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var keyDetails = new KeyDetails(
                    key.Name,
                    key.KeyType.ToString(),
                    key.Properties.Enabled,
                    key.Properties.NotBefore,
                    key.Properties.ExpiresOn,
                    key.Properties.CreatedOn,
                    key.Properties.UpdatedOn);

                context.Response.Results = ResponseResult.Create(new KeyGetCommandResult(Keys: null, Key: keyDetails), KeyVaultJsonContext.Default.KeyGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.KeyName))
            {
                _logger.LogError(ex, "Error listing keys from vault {VaultName}", options.VaultName);
            }
            else
            {
                _logger.LogError(ex, "Error getting key {KeyName} from vault {VaultName}", options.KeyName, options.VaultName);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyDetails(string Name, string KeyType, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn);
    internal record KeyGetCommandResult(List<string>? Keys, KeyDetails? Key);
}
