// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Key;

public sealed class KeyListCommand(ILogger<KeyListCommand> logger) : SubscriptionCommand<KeyListOptions>
{
    private const string CommandTitle = "List Key Vault Keys";
    private readonly ILogger<KeyListCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;
    private readonly Option<bool> _includeManagedKeysOption = KeyVaultOptionDefinitions.IncludeManagedKeys;

    public override string Name => "list";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        """
        List all keys in an Azure Key Vault. This command retrieves and displays the names of all keys
        stored in the specified vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_vaultOption);
        command.Options.Add(_includeManagedKeysOption);
    }

    protected override KeyListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValue(_vaultOption);
        options.IncludeManagedKeys = parseResult.GetValue(_includeManagedKeysOption);
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
            var keys = await keyVaultService.ListKeys(
                options.VaultName!,
                options.IncludeManagedKeys,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = keys?.Count > 0 ?
                ResponseResult.Create(
                    new KeyListCommandResult(keys),
                    KeyVaultJsonContext.Default.KeyListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing keys from vault {VaultName}.", options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyListCommandResult(List<string> Keys);
}
