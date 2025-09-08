// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Admin; // removed AdminSettingsGetOptions; will use BaseKeyVaultOptions directly
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Admin;

public sealed class AdminSettingsGetCommand(ILogger<AdminSettingsGetCommand> logger) : SubscriptionCommand<BaseKeyVaultOptions>
{
    private const string CommandTitle = "Get Key Vault Administration Settings";
    private readonly ILogger<AdminSettingsGetCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;

    public override string Name => "get";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        "Retrieves Key Vault administration settings using the data plane Administration SDK: currently returns purge protection state and soft delete retention (days). Other fields are not yet included.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_vaultOption);
    }

    protected override BaseKeyVaultOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValue(_vaultOption);
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
            var service = context.GetService<IKeyVaultService>();
            var settings = await service.GetVaultSettings(options.VaultName!, options.Subscription!, options.Tenant, options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(settings, KeyVaultJsonContext.Default.VaultSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin settings for vault {VaultName}", options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
