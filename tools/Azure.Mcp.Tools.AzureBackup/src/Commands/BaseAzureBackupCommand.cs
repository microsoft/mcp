// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AzureBackup.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

public abstract class BaseAzureBackupCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseAzureBackupOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Vault.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.VaultType);
        command.Validators.Add(commandResult =>
        {
            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.VaultType.Name))
            {
                var value = commandResult.GetValue<string>(AzureBackupOptionDefinitions.VaultType.Name);
                if (!string.IsNullOrEmpty(value) &&
                    !value.Equals("rsv", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("dpp", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--vault-type must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
                }
            }
        });
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Vault = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Vault.Name);
        options.VaultType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.VaultType.Name);
        return options;
    }
}
