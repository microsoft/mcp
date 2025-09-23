// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Admin;

public sealed class AdminSettingsGetCommand(ILogger<AdminSettingsGetCommand> logger) : SubscriptionCommand<BaseKeyVaultOptions>
{
    private const string CommandTitle = "Get Key Vault Managed HSM Account Settings";
    private readonly ILogger<AdminSettingsGetCommand> _logger = logger;

    public override string Name => "get";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        OpenWorld = true,        // Command queries Azure resources (vault settings)
        Destructive = false,     // Command only reads settings, no modifications
        Idempotent = true,       // Same call produces same result
        ReadOnly = true,         // Only reads data, no state changes
        Secret = false,          // Returns configuration settings, not secrets
        LocalRequired = false    // Pure Azure API call, no local resources needed
    };

    public override string Description =>
        "Retrieves all Key Vault Managed HSM account settings for a given vault. This includes settings such as purge protection and soft-delete retention days. This tool ONLY applies to Managed HSM vaults.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName.AsRequired());
    }

    protected override BaseKeyVaultOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
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
            var settingsResult = await service.GetVaultSettings(options.VaultName!, options.Subscription!, options.Tenant, options.RetryPolicy);

            // Convert settings to a dictionary of strings for easier serialization in case the service adds new settings in the future.
            Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);
            if (settingsResult?.Settings != null)
            {
                foreach (var setting in settingsResult.Settings)
                {
                    settings[setting.Name] = setting.Value.ToString();
                }
            }

            var result = new AdminSettingsGetCommandResult(options.VaultName!, settings);
            context.Response.Results = ResponseResult.Create(result, KeyVaultJsonContext.Default.AdminSettingsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin settings for vault {VaultName}", options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AdminSettingsGetCommandResult(string Name, Dictionary<string, string> Settings);
}
