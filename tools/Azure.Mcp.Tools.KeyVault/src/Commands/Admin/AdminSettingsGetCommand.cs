// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.KeyVault.Commands.Admin;

[CommandMetadata(
    Id = "2e89755e-8c64-4c08-ae10-8fd47aead570",
    Name = "get",
    Title = "Get Key Vault Managed HSM Account Settings",
    Description = "Retrieves all Managed HSM account settings for a Key Vault. Returns configuration setting values such as purge protection and soft-delete retention days. This is NOT for secrets, keys, or certificates — use this when the user asks about vault configuration settings or account-level settings. This tool ONLY applies to Managed HSM vaults.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AdminSettingsGetCommand(ILogger<AdminSettingsGetCommand> logger, IKeyVaultService keyVaultService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseKeyVaultOptions, AdminSettingsGetCommand.AdminSettingsGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AdminSettingsGetCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseKeyVaultOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var settingsResult = await _keyVaultService.GetVaultSettings(options.Vault, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);

            // Convert settings to a dictionary of strings for easier serialization in case the service adds new settings in the future.
            Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);
            if (settingsResult?.Settings != null)
            {
                foreach (var setting in settingsResult.Settings)
                {
                    settings[setting.Name] = setting.Value.ToString();
                }
            }

            context.Response.Results = ResponseResult.Create(new(options.Vault!, settings), KeyVaultJsonContext.Default.AdminSettingsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin settings for vault {VaultName}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AdminSettingsGetCommandResult(string Name, Dictionary<string, string> Settings);
}
